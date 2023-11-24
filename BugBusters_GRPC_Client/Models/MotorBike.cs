using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugBusters_GRPC_Client.Models {
    public class MotorBike {
        public MotorBike(int Id, int minecount) {
            id = id;
            mineCount = minecount;
        }

        public int id { get; set; }
        public int mineCount {get;set;}
    }
}
