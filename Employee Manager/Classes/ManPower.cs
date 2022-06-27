using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee_Manager.Classes
{
    public class ManPower
    {
        public string Name { get; set; }
        public string WN { get; set; }
        public string Shift { get; set; }
        public string MainRole { get; set; }
        public string RoleType { get; set; }
        public string BackUpRole { get; set; }
        public string BackUpRole2 { get; set; }
        public string NextRole { get; set; }
        public string StartDate { get; set; }
        public string TotalYears { get; set; }
        public int RoleCapacity { get; set; }
        public int BackUpCapacity { get; set; }
        public int NextCapacity { get; set; }
    }
}
