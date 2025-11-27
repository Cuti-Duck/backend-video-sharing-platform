using backend_video_sharing_platform.Application.DTOs.Search;

namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface ISearchService
    {
        Task<SearchResponse> SearchAsync(SearchRequest request);
    }
}