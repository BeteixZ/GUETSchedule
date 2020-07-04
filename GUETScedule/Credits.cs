using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFGUETSchedule
{
    class Credits
    {
        public string Term { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Credit { get; set; }
        public string Score { get; set; }
        public string PlannedTerm { get; set; }
        public string PlannedCode { get; set; }
        public string PlannedCredit { get; set; }
        public string Type { get; set; }
        public Credits(string term,string code, string name, string credit, string score, string plannedTerm,
                                    string plannedCode,string plannedCredit,string type)
        {
            Term = term;
            Code = code;
            Name = name;
            Credit = credit;
            Score = score;
            PlannedTerm = plannedTerm;
            PlannedCredit = plannedCredit;
            PlannedCode = plannedCode;
            Type = type;
        }
    }
}
