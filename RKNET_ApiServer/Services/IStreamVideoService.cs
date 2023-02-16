namespace RKNET_ApiServer.Services
{
    public interface IStreamVideoService
    {
        Task<Stream> GetVideoStream(string url);
    }
}
