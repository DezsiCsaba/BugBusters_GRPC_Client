using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugBusters_GRPC_Client.Models {
    [Serializable]
    public  class ItemLocationModel
    {
        public int Value { get; set; }
        public int DestinationX { get; set; }
        public int DestinationY { get; set; }
        public string Type { get; set; }
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int OwnerId { get; set; }
    }
}
