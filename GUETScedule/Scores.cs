using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFGUETSchedule
{
    class Scores
    {
        public string Term { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Score { get; set; }
        public string Credit { get; set; }
        public string Type { get; set; }

        public Scores(string term, string name, string code, string score, string credit, string type ) 
        {
            Term = term;
            Name = name;
            Code = code;
            Score = score;
            Credit = credit;
            Type = type;
        }
    }
}
