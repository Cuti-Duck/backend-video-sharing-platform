using Amazon.CognitoIdentityProvider;
using backend_video_sharing_platform.Application.Interfaces;
using backend_video_sharing_platform.Application.Validators;
using backend_video_sharing_platform.Infrastructure.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Amazon.DynamoDBv2;
using Amazon.IVS;
using backend_video_sharing_platform.Infrastructure.Services;
using backend_video_sharing_platform.Application.Interfaces;


var builder = WebApplication.CreateBuilder(args);

// Configure file upload limits
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 200_000_000; // 200 MB
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 200_000_000; // 200 MB
});

// Add Controllers with FluentValidation
builder.Services.AddControllers()
    .AddFluentValidation(fv =>
        fv.RegisterValidatorsFromAssemblyContaining<RegisterUserRequestValidator>());

// AWS Cognito Configuration
var cognitoConfig = builder.Configuration.GetSection("AWS:Cognito");
var authority = cognitoConfig["Authority"];

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidIssuer = authority,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        // Log để debug
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// AWS Services
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonCognitoIdentityProvider>();
builder.Services.AddScoped<ICognitoAuthService, CognitoAuthService>();
builder.Services.AddAWSService<IAmazonIVS>();
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddScoped<IIVSService, IVSService>();

// Swagger Configuration with JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Video Sharing Platform API",
        Version = "v1",
        Description = "API với AWS Cognito Authentication"
    });

    // Định nghĩa Security Scheme cho JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập Access Token từ AWS Cognito. VD: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'"
    });

    // Yêu cầu JWT cho tất cả endpoints có [Authorize]
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Video Sharing Platform API V1");
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// QUAN TRỌNG: Authentication phải đứng trước Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();