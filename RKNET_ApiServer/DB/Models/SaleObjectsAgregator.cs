using System.ComponentModel.DataAnnotations.Schema;

namespace RKNET_ApiServer.DB.Models
{
    public class SaleObjectsAgregator
    {
        public int Id { get; set; }
        public int Midserver { get; set; }
        public int Code { get; set; }
        [Column(TypeName = "money")]
        public decimal SumWithDiscount { get; set; }
        [Column(TypeName = "money")]
        public decimal SumWithoutDiscount { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public int OrderType { get; set; }
        public string OrderNumber { get; set; }
        public int Currency { get; set; }
        public int? Restaurant { get; set; }
        public int Deleted { get; set; }
        public int Hour { get; set; }
        public int Time { get; set; }
    }
}
