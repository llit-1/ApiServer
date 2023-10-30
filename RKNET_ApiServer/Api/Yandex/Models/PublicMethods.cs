using Microsoft.EntityFrameworkCore;
using RKNET_ApiServer.DB;
using RKNET_ApiServer.DB.Models;
using RKNet_Model.MSSQL;

namespace RKNET_ApiServer.Api.Yandex.Models
{
    public class PublicMethods
    {

        public static List<RKNet_Model.MSSQL.MarketOrder.OrderItem> OrderItems(Api.Yandex.Models.Order newOrder, RknetDbContext rknetdb)
        {
            var orderItems = new List<RKNet_Model.MSSQL.MarketOrder.OrderItem>();
            try
            {
                var menu = new R_Keeper.Actions(rknetdb).GetRkMenu();
                var rkCodes = RkCodes(menu.Data);

                var firstItem = true;
                foreach (var yandexItem in newOrder.items)
                {
                    int itemId;
                    var isItemOk = int.TryParse(yandexItem.id, out itemId);
                    var menuItem = new RKNet_Model.Menu.Item();

                    // проверяем корректность переданного id блюда (целое число)
                    if (isItemOk)
                    {
                        menuItem = rknetdb.MenuItems.FirstOrDefault(i => i.Id == itemId);
                    }
                    // пытаемся обработать заказ по названию позиции
                    else
                    {
                        menuItem = rknetdb.MenuItems.FirstOrDefault(i => i.marketName == yandexItem.name);
                        if (menuItem != null & firstItem)
                        {
                            firstItem = false;
                        }
                    }

                    // добавляем позицию в OrderItems
                    if (menuItem != null)
                    {
                        var orderItem = new RKNet_Model.MSSQL.MarketOrder.OrderItem();
                        orderItem.MenuItemId = menuItem.Id;
                        orderItem.RkCode = menuItem.rkCode;
                        orderItem.RkName = menuItem.rkName;
                        orderItem.MarketName = yandexItem.name;
                        orderItem.MarketPrice = (int)yandexItem.price;
                        orderItem.MenuPrice = (int)menuItem.rkDeliveryPrice;
                        orderItem.Quantity = (int)yandexItem.quantity;
                        orderItem.TotalCost = orderItem.MarketPrice * orderItem.Quantity;
                        orderItems.Add(orderItem);                     
                    }
                }
            }
            catch (Exception ex)
            {
                RKNET_ApiServer.Models.Logging.LocalLog($"ошибка RKNET_ApiServer.Api.Yandex.Actions.OrderItems (OrderAdd): {ex.Message}");
            }
            return orderItems;
        }

        private static List<int> RkCodes(List<RKNet_Model.Menu.rkMenuItem> rkItems)
        {
            var rkCodes = new List<int>();
            foreach (var item in rkItems)
            {
                if (!item.isCategory)
                {
                    rkCodes.Add(item.rkCode);
                }
                else
                {
                    var subItems = RkCodes(item.rkMenuItems);
                    rkCodes.AddRange(subItems);
                }
            }
            return rkCodes;
        }


        public static void SaleObjectsAgregatorSave(MSSQLDBContext mssqldb, RknetDbContext rknetdb, RKNet_Model.MSSQL.MarketOrder order, int code)
        {

            var saleObjectsAgregators = mssqldb.SaleObjectsAgregators.Where(c => c.OrderNumber == order.OrderNumber);
            if (saleObjectsAgregators.ToList().Count != 0)
            {
                foreach (var item in saleObjectsAgregators)
                {
                    item.Deleted = code;
                    mssqldb.SaleObjectsAgregators.Update(item);
                }
                mssqldb.SaveChanges();
            }
            // пропускаем если в базе уже есть этот заказ
            else
            {
                List<MarketOrder.OrderItem> orderItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MarketOrder.OrderItem>>(order.OrderItems);
                foreach (var item in orderItems)
                {
                    SaleObjectsAgregator saleObjectsAgregator = new SaleObjectsAgregator();
                    saleObjectsAgregator.Midserver = order.FirstMidserver;
                    saleObjectsAgregator.Code = item.RkCode;
                    saleObjectsAgregator.SumWithDiscount = item.MenuPrice * item.Quantity;
                    saleObjectsAgregator.SumWithoutDiscount = item.MenuPrice * item.Quantity;
                    saleObjectsAgregator.Quantity = item.Quantity;
                    saleObjectsAgregator.Date = order.Created;
                    saleObjectsAgregator.OrderType = 1014626;
                    saleObjectsAgregator.OrderNumber = order.OrderNumber;
                    if (order.YandexOrder != null)
                    {
                        saleObjectsAgregator.Currency = 1010536;
                    }
                    else
                    {
                        saleObjectsAgregator.Currency = 1013070;
                    }
                    RKNet_Model.Rk7XML.CashStation cashStation = rknetdb.CashStations
                                                    .Include(c => c.TT)
                                                    .AsNoTracking()
                                                    .FirstOrDefault(c => c.Midserver == order.FirstMidserver);
                    saleObjectsAgregator.Restaurant = cashStation?.TT?.Restaurant_Sifr;
                    saleObjectsAgregator.Deleted = code;
                    saleObjectsAgregator.Hour = order.Created.Hour;
                    saleObjectsAgregator.Time = order.Created.Hour * 10000 + order.Created.Minute * 100 + order.Created.Second;
                    mssqldb.SaleObjectsAgregators.Add(saleObjectsAgregator);
                }
                mssqldb.SaveChanges();
            }
        }
    }
}
