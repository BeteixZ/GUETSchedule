using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFGUETSchedule
{
    class TodaySchedule
    {
        public string Header { get; set; }
        public string Course { get; set; }


        public TodaySchedule(string header,string course)
        {
            Header = header;
            Course = course;

        }
    }
}
