using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NSoup;
using NSoup.Nodes;
using NSoup.Select;

namespace WPFGUETSchedule
{
    class HtmlToTuple
    {
        static public Dictionary<string, string> SemesterGenerate(string username)
        {
            Dictionary<string, string> semesterToPara = new Dictionary<string, string>();
            int year = Convert.ToInt32(username.Substring(0, 2));
            int currentYear;
            if (DateTime.Now.Month >= 7) { currentYear = DateTime.Now.Year - 2000 - 1; }
            else { currentYear = DateTime.Now.Year - 2000; }
            for (int index = year; index < currentYear; index++)
            {
                string key = "20" + index + "年-20" + (index + 1) + "第一学期";
                string value = "20" + index + "-20" + (index + 1) + "_1";
                semesterToPara.Add(key, value);
                key = "20" + index + "年-20" + (index + 1) + "第二学期";
                value = "20" + index + "-20" + (index + 1) + "_2";
                semesterToPara.Add(key, value);
            }
            return semesterToPara;
        }

        static public Tuple<List<string>, List<List<string>>>  SelectedCourseConverter(string htmlData) 
        {
            Document doc = NSoupClient.Parse(htmlData);
            Elements elements = doc.GetElementsByTag("tr");
            List<string> selectedCourseHeader = new List<string>();
            List<List<string>> selectedCourseData = new List<List<string>>();
            foreach (var text in elements[0].GetElementsByTag("th")) { selectedCourseHeader.Add(text.Text()); }
            for (int index = 1; index < elements.Count - 1; index++)  //Dismiss first and last item
            {
                List<string> tempList = new List<string>();
                foreach (var element in elements[index].GetElementsByTag("td")) { tempList.Add(element.Text()); }
                selectedCourseData.Add(tempList);
            }

            Tuple<List<string>, List<List<string>>> output = new Tuple<List<string>, List<List<string>>>(selectedCourseHeader, selectedCourseData);
            return output;
        }

        static public Tuple<List<string>, List<List<List<string>>>> CourseScheduleConverter(string htmlData)
        {
            /*
             * Tuple.Item1: Header
             * Tuple.Item2: [Week, Time(from morning to evening), DayInWeek] Course String
             */
            Document doc = NSoupClient.Parse(htmlData);
            Elements elements = doc.GetElementsByTag("tr");
            List<string> courseScheduleHeader = new List<string>();
            string[,,] courseScheduleData = new string[22, 5, 7];   //星期 节数 课程
            for (int i = 1; i < 10; i+=2)
            {
                courseScheduleHeader.Add("第" + i + "," + (i + 1) + "节");
                if (i == 9)
                    courseScheduleHeader.Add("晚上");
            }
            for (int index = 1; index < 6; index++) //课程 （每1.2节）
            {
                int startWeek, endWeek;
                List<string> weekList = new List<string>();
                foreach (var text in elements[index].GetElementsByTag("td"))
                {
                    weekList.Add(text.Text());
                }
                for (int jndex = 0; jndex < weekList.Count; jndex++)
                {
                    if (weekList[jndex] == "")
                        continue;
                    var matches = Regex.Split(weekList[jndex], @"(?<=\d{7,7})");
                    foreach (var match in matches)
                    {
                        string newMatch; 
                        if(match == "")
                            break;
                        startWeek = int.Parse(Regex.Match(match, @"(?<=\()\d{0,2}(?=\-)").Value);
                        endWeek = int.Parse(Regex.Match(match, @"(?<=\-)\d{0,2}(?=\))").Value);
                        newMatch = Regex.Replace(match, @"\(\d{0,}\-\d{0,}\)", "");
                        newMatch = Regex.Replace(newMatch, @" ", "\n");
                        for (int kndex = startWeek - 1; kndex <= endWeek - 1; kndex++)
                        {
                            courseScheduleData[kndex, index - 1, weekList.IndexOf(weekList[jndex])] = newMatch;
                        }
                    }

                }

            }
            List<List<List<string>>> matrix = new List<List<List<string>>>();
            for (int i = 0; i < 22; i++)
            {
                List<List<string>> outterTemp = new List<List<string>>();
                for (int j = 0; j < 5; j++)
                {
                    List<string> innerTemp = new List<string>();
                    for (int k = 0; k < 7; k++)
                    {
                        innerTemp.Add(courseScheduleData[i, j, k]);
                    }
                    outterTemp.Add(innerTemp);
                }
                matrix.Add(outterTemp);
            }
            Tuple<List<string>, List<List<List<string>>>> output = new Tuple<List<string>, List<List<List<string>>>>(courseScheduleHeader, matrix);
            return output;
        }
         
        static public Tuple<List<string>, List<List<string>>> ScoreConverter(string htmlData)
        {
            Document doc = NSoupClient.Parse(htmlData);
            Elements table = doc.GetElementsByTag("tr");
            List<string> scoreHeaders = new List<string>();
            List<List<string>> scoreData = new List<List<string>>();
            foreach (var th in table.First.GetElementsByTag("th"))
            {
                string str = th.Text();
                Regex.Replace(str, " ", "");
                scoreHeaders.Add(str);
            }
            for (int i = 1; i < table.Count; i++)
            {
                List<string> tempList = new List<string>();
                foreach (var element in table[i].GetElementsByTag("td"))
                {

                    Console.WriteLine(element.Text());
                    tempList.Add(element.Text());
                }
                scoreData.Add(tempList);
            }
            Tuple<List<string>, List<List<string>>> output = new Tuple<List<string>, List<List<string>>>(scoreHeaders, scoreData);
            return output;
        }
        static public Tuple<List<string>, List<List<string>>> CreditConverter(string htmlData)
        {
            Document doc = NSoupClient.Parse(htmlData);
            var table = doc.GetElementsByTag("table");
            var creditTable = table[table.Count - 1].GetElementsByTag("tr");
            List<string> creditHeader = new List<string>();
            List<List<string>> creditData = new List<List<string>>();
            foreach (var th in creditTable.First.GetElementsByTag("th"))
            {
                string str = "";
                str = Regex.Replace(str, " ", "");
                creditHeader.Add(str);
            }
            for (int i = 1; i < creditTable.Count; i++)
            {
                List<string> tempList = new List<string>();
                foreach (var element in creditTable[i].GetElementsByTag("td"))
                {
                    Console.WriteLine(element.Text());
                    tempList.Add(element.Text());
                }
                creditData.Add(tempList);
            }
            Tuple<List<string>, List<List<string>>> output = new Tuple<List<string>, List<List<string>>>(creditHeader, creditData);
            return output;
        }
    }
}
