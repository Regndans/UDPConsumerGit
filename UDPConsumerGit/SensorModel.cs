using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPConsumerGit
{
    public class SensorModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string Status { get; set; }
        public DateTime TimeOfDetection { get; set; }
    }
}