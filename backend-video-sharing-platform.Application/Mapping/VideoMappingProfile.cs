using Amazon;
using AutoMapper;
using backend_video_sharing_platform.Application.DTOs.Video;
using backend_video_sharing_platform.Domain.Entities;

namespace backend_video_sharing_platform.Application.Mapping
{
    public class VideoMappingProfile : AutoMapper.Profile
    {
        public VideoMappingProfile()
        {
            CreateMap<Video, VideoResponse>();
        }
    }
}
