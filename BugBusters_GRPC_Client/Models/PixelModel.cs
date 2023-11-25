using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugBusters_GRPC_Client.Models {
    public class PixelModel {
        public int X { get; set; }
        public int Y { get; set; }
        public bool walkable { get; set; }
    }
}
