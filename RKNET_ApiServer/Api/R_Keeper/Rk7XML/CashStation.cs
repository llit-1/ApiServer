using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML
{
    public class CashStation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public RKNet_Model.TT.TT TT { get; set; }
    }
}
