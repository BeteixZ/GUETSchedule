using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFGUETSchedule
{
    class CourseSchedule
    {
        public string Header { get; set; }
        public string Monday { get; set; }
        public string Tuesday { get; set; }
        public string Wednesday { get; set; }
        public string Thursday { get; set; }
        public string Friday { get; set; }
        public string Saturday { get; set; }
        public string Sunday { get; set; }

        public CourseSchedule(string header,string mon,string tue,string wed,string thu,string fri,string sat,string sun)
        {
            Header = header;
            Monday = mon;
            Tuesday = tue;
            Wednesday = wed;
            Thursday = thu;
            Friday = fri;
            Saturday = sat;
            Sunday = sun;
        }
    }
}
