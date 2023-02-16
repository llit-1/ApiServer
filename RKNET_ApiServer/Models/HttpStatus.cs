namespace RKNET_ApiServer.Models
{
    public class HttpStatus
    {
        public int Code;
        public string Name;

        public HttpStatus(int code)
        {
            Code = code;
            var status = HttpStatuses.FirstOrDefault(s => s.Code == code);
            if (status != null)
            {
                Name = status.Name;
            }
            else
            {
                Name = String.Empty;
            }
        }

        private HttpStatus() { }
        
        // справочник статусов http
        private static List<HttpStatus> HttpStatuses = new List<HttpStatus>()
        {
            new HttpStatus{ Code = 100, Name = "продолжение" },
            new HttpStatus{ Code = 101, Name = "смена протокола" },
            new HttpStatus{ Code = 102, Name = "обработка" },
            new HttpStatus{ Code = 103, Name = "ранняя метаинформация" },
            new HttpStatus{ Code = 200, Name = "успешно" },
            new HttpStatus{ Code = 400, Name = "неверный запрос" },
            new HttpStatus{ Code = 401, Name = "не авторизован" },
            new HttpStatus{ Code = 403, Name = "запрещено" },
            new HttpStatus{ Code = 404, Name = "не найдено" },
            new HttpStatus{ Code = 500, Name = "внутренняя ошибка сервера" },
            new HttpStatus{ Code = 502, Name = "неверный шлюз" },
            new HttpStatus{ Code = 503, Name = "сервис недоступен" },
            new HttpStatus{ Code = 505, Name = "версия HTTP не поддерживается" },
            new HttpStatus{ Code = 522, Name = "превышено время ожидания" }
        };
    }
}
