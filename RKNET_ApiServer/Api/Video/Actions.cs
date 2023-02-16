using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace RKNET_ApiServer.Api.Video
{
    [BasicAuth.BasicAuthorization]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Видеонаблюдение")]
    [Route("Video/[action]")]
    public partial class Actions : ControllerBase
    {
        private DB.RknetDbContext rknetdb;
        private RKNET_ApiServer.Services.IStreamVideoService streamingService;

        public Actions(DB.RknetDbContext rknetdbContext, RKNET_ApiServer.Services.IStreamVideoService streamVideoService)
        {
            rknetdb = rknetdbContext;
            streamingService = streamVideoService;
        }

        // вычисление MD5 хэша
        public static string CreateMD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // преобразуем массив байт в шестнадцетеричную строку
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
    }
}
