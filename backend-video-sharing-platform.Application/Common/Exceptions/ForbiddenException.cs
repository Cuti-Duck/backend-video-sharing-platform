

namespace backend_video_sharing_platform.Application.Common.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
