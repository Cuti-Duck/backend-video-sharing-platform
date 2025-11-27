using backend_video_sharing_platform.Application.DTOs.Search;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend_video_sharing_platform.Application.Services
{
    public class SearchService : ISearchService
    {
        private readonly IVideoRepository _videoRepo;
        private readonly IChannelRepository _channelRepo;
        private readonly ILogger<SearchService> _logger;

        public SearchService(
            IVideoRepository videoRepo,
            IChannelRepository channelRepo,
            ILogger<SearchService> logger)
        {
            _videoRepo = videoRepo;
            _channelRepo = channelRepo;
            _logger = logger;
        }

        public async Task<SearchResponse> SearchAsync(SearchRequest request)
        {
            var query = request.Query?.Trim().ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(query))
            {
                return new SearchResponse
                {
                    Query = request.Query,
                    Videos = new List<VideoSearchResult>(),
                    Channels = new List<ChannelSearchResult>(),
                    TotalVideos = 0,
                    TotalChannels = 0,
                    TotalResults = 0
                };
            }

            var videoResults = new List<VideoSearchResult>();
            var channelResults = new List<ChannelSearchResult>();

            // Search Videos (reuse existing GetAllVideosAsync)
            if (string.IsNullOrEmpty(request.Type) || request.Type.ToLower() == "video")
            {
                videoResults = await SearchVideosAsync(query);

                // Sort videos
                videoResults = request.SortBy.ToLower() switch
                {
                    "views" => videoResults.OrderByDescending(v => v.ViewCount).ToList(),
                    "date" => videoResults.OrderByDescending(v => v.CreatedAt).ToList(),
                    _ => videoResults // relevance
                };
            }

            // Search Channels (reuse existing GetAllChannelsAsync)
            if (string.IsNullOrEmpty(request.Type) || request.Type.ToLower() == "channel")
            {
                channelResults = await SearchChannelsAsync(query);

                // Sort channels
                channelResults = request.SortBy.ToLower() switch
                {
                    "subscribers" => channelResults.OrderByDescending(c => c.SubscriberCount).ToList(),
                    "date" => channelResults.OrderByDescending(c => c.CreatedAt).ToList(),
                    _ => channelResults // relevance
                };
            }

            // Pagination
            var paginatedVideos = videoResults.Skip(request.Offset).Take(request.Limit).ToList();
            var paginatedChannels = channelResults.Skip(request.Offset).Take(request.Limit).ToList();

            _logger.LogInformation(
                "Search query: '{Query}', Type: '{Type}', Found: {VideoCount} videos, {ChannelCount} channels",
                query, request.Type ?? "all", videoResults.Count, channelResults.Count
            );

            return new SearchResponse
            {
                Query = request.Query,
                Videos = paginatedVideos,
                Channels = paginatedChannels,
                TotalVideos = videoResults.Count,
                TotalChannels = channelResults.Count,
                TotalResults = videoResults.Count + channelResults.Count
            };
        }

        private async Task<List<VideoSearchResult>> SearchVideosAsync(string query)
        {
            // Reuse existing GetAllVideosAsync - NO CHANGES NEEDED
            var allVideos = await _videoRepo.GetAllVideosAsync();
            var results = new List<VideoSearchResult>();

            foreach (var video in allVideos)
            {
                // Skip videos that are not ready
                if (video.Status?.ToUpper() != "COMPLETE")
                    continue;

                // Calculate relevance score
                var score = CalculateVideoRelevance(video, query);

                if (score > 0)
                {
                    // Get channel info (reuse existing method)
                    var channel = await _channelRepo.GetByIdAsync(video.ChannelId);

                    results.Add(new VideoSearchResult
                    {
                        VideoId = video.VideoId,
                        Title = video.Title,
                        Description = video.Description ?? string.Empty,
                        ThumbnailUrl = video.ThumbnailUrl,
                        ChannelId = video.ChannelId,
                        ChannelName = channel?.Name ?? "Unknown",
                        ViewCount = video.ViewCount,
                        LikeCount = video.LikeCount,
                        Duration = video.Duration,
                        CreatedAt = video.CreatedAt
                    });
                }
            }

            // Sort by relevance
            return results.OrderByDescending(v =>
                v.Title.ToLower().Contains(query) ? 2 : 1
            ).ToList();
        }

        private async Task<List<ChannelSearchResult>> SearchChannelsAsync(string query)
        {
            // Reuse existing GetAllChannelsAsync - NO CHANGES NEEDED
            var allChannels = await _channelRepo.GetAllChannelsAsync();
            var results = new List<ChannelSearchResult>();

            foreach (var channel in allChannels)
            {
                // Check if channel name matches
                if (channel.Name.ToLower().Contains(query))
                {
                    // Get video count for this channel (reuse existing method)
                    var videos = await _videoRepo.GetVideosByChannelIdAsync(channel.ChannelId);

                    results.Add(new ChannelSearchResult
                    {
                        ChannelId = channel.ChannelId,
                        ChannelName = channel.Name,
                        //AvatarUrl = channel.AvatarUrl,
                        SubscriberCount = channel.SubscriberCount,
                        VideoCount = videos.Count,
                        CreatedAt = channel.CreatedAt
                    });
                }
            }

            return results.OrderByDescending(c => c.SubscriberCount).ToList();
        }

        private int CalculateVideoRelevance(Domain.Entities.Video video, string query)
        {
            var score = 0;
            var title = video.Title.ToLower();
            var description = (video.Description ?? string.Empty).ToLower();

            // Exact match in title
            if (title == query)
                score += 100;

            // Title contains query
            if (title.Contains(query))
                score += 50;

            // Description contains query
            if (description.Contains(query))
                score += 25;

            // Check individual words
            var queryWords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in queryWords)
            {
                if (title.Contains(word))
                    score += 10;
                if (description.Contains(word))
                    score += 5;
            }

            return score;
        }
    }
}