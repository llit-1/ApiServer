using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using System.IO;


namespace zabbix_lib
{                  
    public static class ZabbixSender
    {
        // путь к zabbix_sender.exe
        private static readonly string ZabbixSenderPath = @"\\dc1.shzhleb.ru\LogonAudit\zabbix_sender.exe";

        // ip сервера zabbix
        private static readonly string ZabbixServerIp = "10.140.0.118";

        // имя временного файла с конфигурацией
        private static readonly string TempFileName = "zabconf.txt";

        // каталог для размещения временного файла
        private static readonly string TempFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\zabbix_lib";
        
                      
        // Отправка данных в Zabbix
        public static void Send(AgregatorError agregatorError) 
            
        {
            try
            {
                //Получаем строку в json из экземпляра класса AgregatorsError
                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(agregatorError);

                #region На случай если нужны двойные кавычки
                ////Для значений экземпляра класса AgregatorsError
                //agregatorError.agregatorName = Double_q(agregatorError.agregatorName);
                //agregatorError.dateTime = Double_q(agregatorError.dateTime);
                //agregatorError.ttName = Double_q(agregatorError.ttName);
                //agregatorError.requestUrl = Double_q(agregatorError.requestUrl);
                //agregatorError.requestName = Double_q(agregatorError.requestName);
                //agregatorError.errorMessage = Double_q(agregatorError.errorMessage);
                //agregatorError.requestBody = Double_q(agregatorError.requestBody);

                ////Для имен объектов
                //string jstr = Double_q(jsonString, "agregatorName");
                //string jsr1 = Double_q(jstr, "dateTime");
                //string jsr2 = Double_q(jsr1, "ttName");
                //string jsr3 = Double_q(jsr2, "requestUrl");
                //string jsr4 = Double_q(jsr3, "resultCode");
                //string jsr5 = Double_q(jsr4, "requestName");
                //string jsr6 = Double_q(jsr5, "errorMessage");
                //string jsr7 = Double_q(jsr6, "requestBody");
                //string jsr8 = Double_q(jsr7, "ttCode");
                //string jstr1 = jsr8;
                #endregion

                //Создаем и подготавливаем файл конфигурации для отправки
                if(!Directory.Exists(TempFilePath))
                {
                    Directory.CreateDirectory(TempFilePath);
                }               

                string temp = $@"{TempFilePath}\{TempFileName}";
                if (File.Exists(temp))
                {
                    File.WriteAllText(temp, "");
                }

                string config = $"\"Agregat_error_Monitor\" agr_Json {jsonString}";
                System.IO.File.WriteAllText($@"{temp}", config);

                //Формируем строку аргументов для запуска процесса
                String arguments = $@"-z {ZabbixServerIp} -i {temp}";

                //Запускаем отправку данных
                Process.Start(ZabbixSenderPath, arguments);
            }
            catch { }
        }
      
        
        // Заключения строки в двойные кавычки (str - исходная строка, value - значение для заключения в кавычки)
        private static string Double_q(string str,string value)
        {
            string result = str.Replace(value,$"\"{value}\"");
            return result;
        }
        private static string Double_q(string value)
        {
            string result =  $"\"{value}\"";
            return result;
        }
    }      
}






