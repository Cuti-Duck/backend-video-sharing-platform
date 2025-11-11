using System;
using System.Threading.Tasks;
using Amazon.IVS;
using Amazon.IVS.Model;
using Amazon.DynamoDBv2.DataModel;
using backend_video_sharing_platform.Application.DTOs.Livestream;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.Extensions.Configuration;

// Alias để tránh conflict với Amazon.IVS.Model.Channel
using DbChannel = backend_video_sharing_platform.Domain.Entities.Channel;

namespace backend_video_sharing_platform.Infrastructure.Services
{
    public class IVSService : IIVSService
    {
        private readonly IAmazonIVS _ivs;
        private readonly IDynamoDBContext _db;
        private readonly IConfiguration _config;

        public IVSService(IAmazonIVS ivs, IDynamoDBContext db, IConfiguration config)
        {
            _ivs = ivs;
            _db = db;
            _config = config;
        }

        public async Task<CreateLivestreamResponse> CreateLivestreamAsync(string userId)
        {
            //  Load channel theo userId (channelId = userId)
            var channel = await _db.LoadAsync<DbChannel>(userId);

            if (channel == null)
                throw new Exception($"Channel for userId {userId} not found. Ensure PostConfirmation trigger created it.");

            // Nếu channel đã có IVS Arn → return luôn (idempotent)
            if (!string.IsNullOrEmpty(channel.ChannelArn))
            {
                var streamKeyValue = await TryGetStreamKeyAsync(channel.StreamKeyArn);

                return new CreateLivestreamResponse
                {
                    Message = "Channel already exists",
                    ChannelArn = channel.ChannelArn!,
                    PlaybackUrl = channel.PlaybackUrl!,
                    IngestServer = BuildRtmps(channel.IngestEndpoint!),
                    StreamKeyArn = channel.StreamKeyArn ?? "",
                    StreamKey = streamKeyValue ?? ""
                };
            }

            //  Tạo mới channel IVS
            var recordArn = _config["AWS:RecordingConfigurationArn"]
                            ?? throw new Exception("Missing RecordingConfigurationArn in appsettings.json");

            var create = await _ivs.CreateChannelAsync(new CreateChannelRequest
            {
                Name = $"user-{userId}-channel",
                Type = ChannelType.STANDARD,
                Authorized = false,
                LatencyMode = ChannelLatencyMode.LOW,
                RecordingConfigurationArn = recordArn,
                Tags = new() { { "UserId", userId }, { "Project", "VideoSharing" } }
            });

            //  Cập nhật lại Channel record trong DynamoDB
            channel.ChannelArn = create.Channel.Arn;
            channel.PlaybackUrl = create.Channel.PlaybackUrl;
            channel.IngestEndpoint = create.Channel.IngestEndpoint;
            channel.StreamKeyArn = create.StreamKey.Arn;

            await _db.SaveAsync(channel);

            return new CreateLivestreamResponse
            {
                Message = "Channel created successfully",
                ChannelArn = create.Channel.Arn,
                PlaybackUrl = create.Channel.PlaybackUrl,
                IngestServer = BuildRtmps(create.Channel.IngestEndpoint),
                StreamKeyArn = create.StreamKey.Arn,
                StreamKey = create.StreamKey.Value
            };
        }

        //  Helper functions
        private static string BuildRtmps(string endpoint) =>
            $"rtmps://{endpoint}:443/app/";

        private async Task<string?> TryGetStreamKeyAsync(string? streamKeyArn)
        {
            if (string.IsNullOrEmpty(streamKeyArn)) return null;

            try
            {
                var res = await _ivs.GetStreamKeyAsync(new GetStreamKeyRequest { Arn = streamKeyArn });
                return res.StreamKey?.Value;
            }
            catch
            {
                return null;
            }
        }
    }
}
