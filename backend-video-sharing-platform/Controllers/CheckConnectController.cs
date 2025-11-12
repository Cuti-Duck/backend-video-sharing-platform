using Amazon.DynamoDBv2;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;

namespace backend_video_sharing_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckConnectController : ControllerBase
    {
        private readonly ILogger<CheckConnectController> _logger;
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly IAmazonS3 _s3Client;

        public CheckConnectController(ILogger<CheckConnectController> logger, IAmazonDynamoDB dynamoDbClient, IAmazonS3 s3client)
        {
            _logger = logger;
            _dynamoDbClient = dynamoDbClient;
            _s3Client = s3client;
        }

        [HttpGet("pingDynamoDB")]
        public async Task<bool> PingDynamoDB()
        {
            try
            {
                _logger.LogInformation("Pinging DynamoDB...");
                var response = await _dynamoDbClient.ListTablesAsync();
                if (response.TableNames.Count > 0)
                {
                    _logger.LogInformation($"✅ Connected! Found {response.TableNames.Count} tables. First table: {response.TableNames[0]}");
                }
                else
                {
                    _logger.LogInformation("✅ Connected, but no tables found.");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error pinging DynamoDB: {ex.Message}");
                return false;
            }
        }

        [HttpGet("pingS3")]

        public async Task<bool> PingS3()
        {
            try
            {
                _logger.LogInformation("Pinging S3...");
                var response = await _s3Client.ListBucketsAsync(); // Placeholder for actual S3 call
                if (response.Buckets != null && response.Buckets.Count >= 0)
                {
                    _logger.LogInformation($"S3 ping successful, found {response.Buckets.Count} buckets.");
                    return true;
                }
                _logger.LogWarning("S3 ping returned no buckets.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error pinging S3: {ex.Message}");
                return false;
            }
        }
    }
}
