using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using NSoup;
using NSoup.Nodes;
using NSoup.Select;

namespace WPFGUETSchedule
{
    class GUETSchedule
    {
        private string webVpnLoginUrl = "https://v.guet.edu.cn/do-confirm-login";
        static private string eduSysRootUrl = "https://v.guet.edu.cn/http/77726476706e69737468656265737421f2fc4b8b757e6f457b1cc7a99c406d36f0/student";
        private string eduSysLoginUrl = eduSysRootUrl + "/public/login.asp";
        private string eduSysSelectedCourseUrl = eduSysRootUrl + "/Selected.asp";
        private string eduSysCourseScheduleUrl = eduSysRootUrl + "/coursetable.asp";
        private string eduSysCreditUrl = eduSysRootUrl + "/credits.asp";
        private string eduSysScoreUrl = eduSysRootUrl + "/score.asp";
        private string eduSysInfoUrl = eduSysRootUrl + "/info.asp";

        private bool loginStatus = false;
        public string WebVpnUrl { get => webVpnLoginUrl; }
        public string EduSysUrl { get => eduSysLoginUrl; }
        public bool LoginStatus { get => loginStatus; }

        public Dictionary<string, string> StudentInfo = new Dictionary<string, string>();

        private CookieAwareWebClient webClient = new CookieAwareWebClient();
        public bool Login(string[] webVpn, string[] eduSys)
        {
            byte[] requestData;
            //Define POST data
            var webVpnLoginData = new NameValueCollection();
            webVpnLoginData.Add("username", webVpn[0]);
            webVpnLoginData.Add("password", webVpn[1]);
            var eduSysLoginData = new NameValueCollection();
            eduSysLoginData.Add("username", eduSys[0]);
            eduSysLoginData.Add("passwd", eduSys[1]);
            eduSysLoginData.Add("login", "%EF%BF%BD%C7%A1%EF%BF%BD%C2%BC");

            //Define web request with cookie
            requestData = webClient.UploadValues(webVpnLoginUrl, "POST", webVpnLoginData);

            //fail login
            if (Encoding.Default.GetString(requestData).LastIndexOf("true") == -1)
            {
                loginStatus = false;
                return false;
            }

            //request of eduSys
            requestData = webClient.UploadValues(eduSysLoginUrl, "POST", eduSysLoginData);

            if (Encoding.Default.GetString(requestData).LastIndexOf(eduSys[0]) == -1)
            {
                loginStatus = false;
                return false;
            }
            Document doc = NSoupClient.Parse(new MemoryStream(webClient.DownloadData(eduSysInfoUrl)), "gb2312");
            var elements = doc.GetElementsByTag("p");
            StudentInfo["matricNumber"] = Regex.Replace(elements[0].Text(),"学号:","");
            StudentInfo["name"] = Regex.Replace(elements[1].Text(), "姓名:", "");
            StudentInfo["class"] = Regex.Replace(elements[2].Text(), "班级:", "");
            StudentInfo["grade"] = Regex.Replace(elements[3].Text(), "年级:", "");

            webClient.DownloadData(eduSysInfoUrl);

            loginStatus = true;
            return true;    //Fix
        }


        public Tuple<List<string>, List<List<string>>> GetSelcetedCourse(string term)
        {
            if (!loginStatus)
                return null;

            Regex regex = new Regex(@"\b20[0-9][0-9]-20[0-9][0-9]_[1-2]");
            if (!regex.Match(term).Success) { return null; }

            byte[] requestData;
            var termData = new NameValueCollection();
            termData.Add("term", term);
            requestData = webClient.UploadValues(eduSysSelectedCourseUrl, "POST", termData);

            Document doc = NSoupClient.Parse(new MemoryStream(requestData), "gb2312");
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

        public string GetSelectedCourseRawData(string term)
        {
            if (!loginStatus)
                return null;

            Regex regex = new Regex(@"\b20[0-9][0-9]-20[0-9][0-9]_[1-2]");
            if (!regex.Match(term).Success) { return null; }

            byte[] requestData;
            var termData = new NameValueCollection();
            termData.Add("term", term);
            requestData = webClient.UploadValues(eduSysSelectedCourseUrl, "POST", termData);

            return Encoding.GetEncoding("gb2312").GetString(requestData);

        }

        public bool? GetSchedule(string term)
        {
            if (!loginStatus)
                return null;

            Regex regex = new Regex(@"\b20[0-9][0-9]-20[0-9][0-9]_[1-2]");
            if (!regex.Match(term).Success) { return null; }

            byte[] requestData;
            var termData = new NameValueCollection();
            termData.Add("term", term);
            requestData = webClient.UploadValues(eduSysCourseScheduleUrl, "POST", termData);

            StreamWriter streamWriter = new StreamWriter(@"C:\Users\b\Desktop\data.html");
            streamWriter.Write(Encoding.GetEncoding("gb2312").GetString(requestData));
            streamWriter.Close();
            
            return true;
        }
        
        public string GetScheduleRawData(string term)
        {
            if (!loginStatus)
                return null;

            Regex regex = new Regex(@"\b20[0-9][0-9]-20[0-9][0-9]_[1-2]");
            if (!regex.Match(term).Success) { return null; }

            byte[] requestData;
            var termData = new NameValueCollection();
            termData.Add("term", term);
             requestData = webClient.UploadValues(eduSysCourseScheduleUrl, "POST", termData);

            return Encoding.GetEncoding("gb2312").GetString(requestData);
        }

        public string GetScoreRawData( )
        {
            if (!loginStatus)
                return null;

            byte[] requestData;
            var termData = new NameValueCollection();
            termData.Add("ckind", "");
            termData.Add("lwPageSize", "1000");
            termData.Add("lwBtnquery", "%B2%E9%D1%AF");
            requestData = webClient.UploadValues(eduSysScoreUrl, "POST", termData);
            return Encoding.GetEncoding("gb2312").GetString(requestData);
        }

        public Tuple<List<string>, List<List<string>>> GetScore()
        {
            if (!loginStatus)
                return null;

            byte[] requestData;
            var termData = new NameValueCollection();
            termData.Add("ckind", "");
            termData.Add("lwPageSize", "1000");
            termData.Add("lwBtnquery", "%B2%E9%D1%AF");
            requestData = webClient.UploadValues(eduSysScoreUrl, "POST", termData);

            List<string> scoreHeaders = new List<string>();
            List<List<string>> scoreData = new List<List<string>>();
            Document doc = NSoupClient.Parse(Encoding.GetEncoding("gb2312").GetString(requestData));
            Elements table = doc.GetElementsByTag("tr");

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
                    tempList.Add(element.Text());
                }
                scoreData.Add(tempList);
            }
            Tuple<List<string>, List<List<string>>> output = new Tuple<List<string>, List<List<string>>>(scoreHeaders, scoreData);
            return output;
        }

        public string GetCreditRawData()
        {
            if (!loginStatus)
                return null;

            byte[] requestData;
            requestData = webClient.DownloadData(eduSysCreditUrl);
            return Encoding.GetEncoding("gb2312").GetString(requestData);
        }

        public Tuple<List<string>, List<List<string>>> GetCredit()
        {
            if (!loginStatus)
                return null;

            byte[] requestData;
            requestData = webClient.DownloadData(eduSysCreditUrl);
            Document doc = NSoupClient.Parse(Encoding.GetEncoding("gb2312").GetString(requestData));
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
        public bool Logout()
        {
            var nullData = new NameValueCollection();
            WebClient webClient = new WebClient();
            byte[] requesetData = webClient.UploadValues("https://v.guet.edu.cn/logout", "POST", nullData);
            if (requesetData.Length == 0)
                return false;
            return true;
        }

    }
}
