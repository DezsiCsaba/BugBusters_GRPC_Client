using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugBusters_GRPC_Client.Models {
    public class MotorBike {
        public MotorBike(int Id, int minecount, bool isActive, double degree) {
            id = id;
            mineCount = minecount;
            Packets = new List<Packet>();
            onOrOff = isActive ? 1 : 0;
            this.degree = degree;
        }

        public int id { get; set; }
        public int mineCount { get; set; }
        public List<Packet> Packets {get;set;}
        public int onOrOff { get; set;}
        public double degree { get; set; }

    }
}
