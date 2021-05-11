using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPConsumerGit
{
    class MotionModel
    {

        public int MotionId { get; set; }
        public int SensorId { get; set; }
        public string Status { get; set; }
        public DateTime TimeOfDetection { get; set; }
    }
}
