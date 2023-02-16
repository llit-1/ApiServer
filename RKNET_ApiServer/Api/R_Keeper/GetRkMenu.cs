using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace RKNET_ApiServer.Api.R_Keeper
{    
    public partial class Actions
    {
        // Запрос меню Р-Кипер
        [HttpGet]
        public RKNet_Model.Result<List<RKNet_Model.Menu.rkMenuItem>> GetRkMenu()
        {
            var result = new RKNet_Model.Result<List<RKNet_Model.Menu.rkMenuItem>>();
            var rkSettings = rknetdb.RKSettings.FirstOrDefault();
            
            if(rkSettings == null)
            {
                result.Ok = false;
                result.ErrorMessage = "нет настроек подключения к Р-Кипер в базе данных";
                return result;
            }

            // получаем меню Р-Кипер
            var menuRkResult = new Rk7XML.Response.GetMenuResponse.RK7QueryResult();
            
            try
            {
                // Создание экземпляра класса запроса XML
                var xml = new Rk7XML.Request.GetMenu.GetRefData();
                var xmlRequest = Rk7XML.Request.Serialize.ToString(xml);

                // Запрос и получение XML ответа от сервера справочников Р-Кипер             
                var rk = new Rk7XML.RK7();

                var responseResult = rk.SendRequest(rkSettings.RefServerIp, xmlRequest, rkSettings.RefServerPort, rkSettings.User, rkSettings.Password);

                // Разбор полученного XML
                if (responseResult.Ok)
                {
                    menuRkResult = Rk7XML.Response.GetMenuResponse.RK7QueryResult.DeSerializeQueryResult(responseResult.Data, rkSettings.DeliveryPriceType);

                    // рекурсивный перебор Меню Р-Кипер и формирование Меню для Combo-Tree
                    var rkMenu = new List<RKNet_Model.Menu.rkMenuItem>();
                    foreach (var rkCat in menuRkResult.rk7Reference.rIChildItems.tCategListItemList)
                    {
                        var category = new RKNet_Model.Menu.rkMenuItem();
                        category.isCategory = true;
                        category.rkName = rkCat.Name;
                        category.rkCode = int.Parse(rkCat.Code);
                        category.rkMenuItems = GetItems(rkCat);
                        rkMenu.Add(category);
                    }
                    result.Data = rkMenu;                    
                }
                else
                {
                    result.Ok = false;
                    result.ErrorMessage = responseResult.ErrorMessage;                    
                }
            }
            catch (Exception e)
            {
                result.Ok = false;
                result.ErrorMessage = e.Message;
                result.ExceptionText = e.ToString();
            }
            return result;
        }       
        [HttpGet]
        public IActionResult GetRkMenuXml()
        {
            var result = new RKNet_Model.Result<string>();
            var rkSettings = rknetdb.RKSettings.FirstOrDefault();

            if (rkSettings == null)
            {
                result.Ok = false;
                result.ErrorMessage = "нет настроек подключения к Р-Кипер в базе данных";
                return new ObjectResult(result);
            }

            // получаем меню Р-Кипер
            var menuRkResult = new Rk7XML.Response.GetMenuResponse.RK7QueryResult();

            try
            {
                // Создание экземпляра класса запроса XML
                var xml = new Rk7XML.Request.GetMenu.GetRefData();
                var xmlRequest = Rk7XML.Request.Serialize.ToString(xml);
                result.Data = xmlRequest;
                return new ObjectResult(result);
                // Запрос и получение XML ответа от сервера справочников Р-Кипер             
                var rk = new Rk7XML.RK7();

                var responseResult = rk.SendRequest(rkSettings.RefServerIp, xmlRequest, rkSettings.RefServerPort, rkSettings.User, rkSettings.Password);

                // Разбор полученного XML
                if (responseResult.Ok)
                {
                    result.Data = responseResult.Data;
                }
                else
                {
                    result.Ok = false;
                    result.ErrorMessage = responseResult.ErrorMessage;
                }
            }
            catch (Exception e)
            {
                result.Ok = false;
                result.ErrorMessage = e.Message;
                result.ExceptionText = e.ToString();
            }
            return new ObjectResult(result);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // метод рекурсивного перебора элементов меню Р-Кипер
        private List<RKNet_Model.Menu.rkMenuItem> GetItems(Rk7XML.Response.GetMenuResponse.TCategListItem rkCategory)
        {
            var items = new List<RKNet_Model.Menu.rkMenuItem>();
            foreach (var rkItem in rkCategory.childItems.MenuItems)
            {
                var item = new RKNet_Model.Menu.rkMenuItem();                
                item.isCategory = false;
                item.rkName = rkItem.Name;
                item.rkCode = int.Parse(rkItem.Code);

                var price = rkItem.deliveryPrice;
                if(price == "9223372036854775807")
                {
                    item.deliveryPrice = 0;
                }
                else
                {
                    item.deliveryPrice = int.Parse(price)/100;
                }

                items.Add(item);
            }

            foreach (var rkCat in rkCategory.childItems.tCategListItemList)
            {
                var category = new RKNet_Model.Menu.rkMenuItem();
                category.isCategory = true;
                category.rkName = rkCat.Name;
                category.rkCode = int.Parse(rkCat.Code);
                category.rkMenuItems = GetItems(rkCat);
                items.Add(category);
            }

            return items;
        }
    }
}
