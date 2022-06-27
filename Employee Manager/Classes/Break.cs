using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee_Manager.Classes
{
    public class Break
    {
        public int Break_ID { get; set; }
        public DateTime BreakStart_Full { get; set; }
        public DateTime BreakEnd_Full { get; set; }
        public string WN { get; set; }
        public string Name { get; set; }
        public string BreakStart { get; set; }
        public string BreakEnd { get; set; }
        public double Duration { get; set; }
        public string RowColor { get; set; }
        public double Allowed { get; set; }
        public double TimeLeft { get; set; }
        public int LateLessFive { get; set; }
        public int LateMoreFive { get; set; }
        public int AllLates { get; set; }
        public string Note { get; set; }
        public string DeletedBy { get; set; }
        public string Department { get; set; }
    }
}
