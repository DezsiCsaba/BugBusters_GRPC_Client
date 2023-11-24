using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugBusters_GRPC_Client.Models {
    internal class SteerBikeInputModel {
        public int bikeId { get; set; }
        public int isActive { get; set; }
        public double degree { get; set; }
    }
}
