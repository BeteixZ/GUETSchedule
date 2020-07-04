using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFGUETSchedule
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        GUETSchedule schedule = new GUETSchedule();
        bool loginStatus = false;
        int clickTime = 0;
        Dictionary<string, string> studentInfo = new Dictionary<string, string>();
        public MainWindow()
        {
            InitializeComponent();
            selectCourseListView.Items.Clear();
            UpdatedLogTextBox("很开心地推出V1.0版供大家测试~");
            UpdatedLogTextBox(@"登录时，依据你的网络状况，会有一定的卡顿(10秒左右)，请耐心等待 _(:з」∠)_
");
            loginStatus = CanLoadFromLast();

            UpdatedTabitem();
            todayLabel.Content = "请选择当前周数 --->";
            RandomizedLabel();
        }

        Dictionary<string, string> semesterToPara = new Dictionary<string, string>();//Key: X年X月 Value: X-X_X
        private bool CanLoadFromLast()
        {
            if (!Directory.Exists(".\\data"))
            {
                Directory.CreateDirectory(".\\data");
                return false;
            }
            if (!File.Exists(".\\data\\status.dat"))
                return false;
            //if (!File.Exists(".\\data\\time.dat"))
            //    return false;
            StreamReader reader = new StreamReader(".\\data\\status.dat");
            webVpnUsername.Text = reader.ReadToEnd();
            reader.Close();
            UpdatedLogTextBox("已经登录啦~");
            ComboBoxBinding();
            UpdatedTabitem();

            return true;
        }
        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            loginStatus = schedule.Login(new string[] { webVpnUsername.Text, webVpnPassword.Text },
            new string[] { webVpnUsername.Text, eduSysPassword.Text });
            if (loginStatus)
            {
                SaveLoginStatus();
                UpdatedLogTextBox("登录成功,记录已保存，请勿删除Data文件夹~");
                ComboBoxBinding();
                DownloadAll();
                UpdatedTabitem();
                UpdatedLogTextBox(studentInfo["name"] + "，欢迎使用~");
                loginButton.IsEnabled = false;
            }
            else { UpdatedLogTextBox("登录失败"); }
            UpdatedTabitem();
        }
        private void SaveLoginStatus()
        {
            File.Create(".\\data\\status.dat").Close();
            StreamWriter writer = new StreamWriter(".\\data\\status.dat");
            writer.Write(webVpnUsername.Text);
            writer.Flush();
            writer.Close();
        }
        private void UpdatedTabitem()   //登录状态与Tabitem与登录按钮可用性绑定
        {
            selectedCourseTabitem.IsEnabled = loginStatus;
            todayScheduleTabitem.IsEnabled = loginStatus;
            scheduleTabitem.IsEnabled = loginStatus;
            scoreTabitem.IsEnabled = loginStatus;
            creditTabitem.IsEnabled = loginStatus;
            UpdatedList();
        }
        private void UpdatedList()
        {
            List<Scores> scoreList = new List<Scores>();
            scoreList.Add(new Scores("点击空白处以加载", null, null, null, null, null));
            scoreListView.ItemsSource = scoreList;

            List<Credits> creditList = new List<Credits>();
            creditList.Add(new Credits("点击空白处以加载", null, null, null, null, null, null, null, null));
            creditListView.ItemsSource = creditList;
        }
        private void UpdatedLogTextBox(string log) //输出日志
        {
            if (logTextBox.Text != null)
                logTextBox.Text = logTextBox.Text + log + "\n";
            logTextBox.ScrollToEnd();
        }
        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            webVpnUsername.Text = "";
            webVpnPassword.Text = "";
            eduSysPassword.Text = "";
            if (!loginStatus)
            {
                UpdatedLogTextBox("您还未登录!");
                return;
            }

            bool logoutStatus = schedule.Logout();
            if (logoutStatus) { UpdatedLogTextBox("注销成功"); loginStatus = false; }
            else { UpdatedLogTextBox("注销失败"); loginStatus = true; }
            UpdatedTabitem();

            DirectoryInfo folder = new DirectoryInfo(".\\data");

            FileInfo[] fileList = folder.GetFiles();
            foreach (FileInfo file in fileList)
            {
                if (file.Extension == ".mt" || file.Extension == ".dat")
                {
                    file.Delete();  // 删除
                }
            }
        }
        private void ComboBoxBinding() {
            semesterToPara = HtmlToTuple.SemesterGenerate(webVpnUsername.Text);
            selectedCourseComboBox.ItemsSource = semesterToPara.Keys;
            semesterComboBox2.ItemsSource = semesterToPara.Keys;

            List<int> weekList = new List<int>();
            for (int i = 1; i < 23; i++)
                weekList.Add(i);
            weekComboBox.ItemsSource = weekList;
            weekComboBox2.ItemsSource = weekList;
        }
        //为ComboBox创建数据绑定列表
        private void semesterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string key = selectedCourseComboBox.SelectedItem.ToString();
            if (!File.Exists(".\\data\\SC-" + semesterToPara[key] + ".mt"))
            {
                File.Create(".\\data\\SC-" + semesterToPara[key] + ".mt").Close();
                string rawData = schedule.GetSelectedCourseRawData(semesterToPara[key]);
                StreamWriter streamWriter = new StreamWriter(".\\data\\SC-" + semesterToPara[key] + ".mt");
                streamWriter.Write(rawData);
                streamWriter.Close();
            }
            StreamReader reader = new StreamReader(".\\data\\SC-" + semesterToPara[key] + ".mt");
            var courseData = HtmlToTuple.SelectedCourseConverter(reader.ReadToEnd());
            reader.Close();
            List<SelectedCourse> courseList = new List<SelectedCourse>();
            foreach (var course in courseData.Item2)
            {
                courseList.Add(new SelectedCourse(course[0], course[1], course[2],
                    course[3], course[4], course[5], course[6]));
            }
            selectCourseListView.ItemsSource = courseList;
        }   
        private void semesterComboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            weekComboBox.IsEnabled = true;
            string key = semesterComboBox2.SelectedItem.ToString();
            if (!File.Exists(".\\data\\CC-" + semesterToPara[key] + ".mt"))
            {
                File.Create(".\\data\\CC-" + semesterToPara[key] + ".mt").Close();
                string rawData = schedule.GetScheduleRawData(semesterToPara[key]);
                StreamWriter streamWriter = new StreamWriter(".\\data\\CC-" + semesterToPara[key] + ".mt");
                streamWriter.Write(rawData);
                streamWriter.Close();
            }

            if (weekComboBox.SelectedIndex == -1)
                weekComboBox.SelectedIndex = 1;

            StreamReader reader = new StreamReader(".\\data\\CC-" + semesterToPara[key] + ".mt");
            Tuple<List<string>, List<List<List<string>>>> tuple = HtmlToTuple.CourseScheduleConverter(
               reader.ReadToEnd());
            reader.Close();

            List<CourseSchedule> schedulesList = new List<CourseSchedule>();
            int index = 0;
            foreach (var timeSchedule in tuple.Item2[weekComboBox.SelectedIndex])
            {
                schedulesList.Add(new CourseSchedule(tuple.Item1[index],
                    timeSchedule[0], timeSchedule[1], timeSchedule[2],
                    timeSchedule[3], timeSchedule[4], timeSchedule[5], timeSchedule[6]));
                index++;
            }
            weekScheduleListView.ItemsSource = schedulesList;
        }
        public bool DownloadAll()
        {
            if (!loginStatus)
                return false;

            studentInfo = schedule.StudentInfo;
            File.Create(".\\data\\score.mt").Close();
            StreamWriter writer = new StreamWriter(".\\data\\score.mt");
            string rawData = schedule.GetScoreRawData();
            if (rawData == "")
                return false;
            writer.Write(rawData);
            writer.Close();
            File.Create(".\\data\\credit.mt").Close();
            writer = new StreamWriter(".\\data\\credit.mt");
            rawData = schedule.GetCreditRawData();
            if (rawData == "")
                return false;
            writer.Write(rawData);
            writer.Close();

            foreach (var semester in semesterToPara)
            {
                File.Create(".\\data\\SC-" + semester.Value + ".mt").Close();
                writer = new StreamWriter(".\\data\\SC-" + semester.Value + ".mt");
                rawData = schedule.GetSelectedCourseRawData(semester.Value);
                if (rawData == "")
                    return false;
                writer.Write(rawData);
                writer.Close();
                File.Create(".\\data\\CC-" + semester.Value + ".mt").Close();
                writer = new StreamWriter(".\\data\\CC-" + semester.Value + ".mt");
                rawData = schedule.GetScheduleRawData(semester.Value);
                if (rawData == "")
                    return false;
                writer.Write(rawData);
                writer.Close();
            }

            return true;
        }
        private void weekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            semesterComboBox2_SelectionChanged(sender, e);
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            File.Delete(".\\data\\time.dat");
            weekComboBox2.IsEnabled = true;
            resetButton.IsEnabled = false;
            chooseWeekLabel.Content = "请选择周数:";
            todayScheduleListView.ItemsSource = null;
        }
        private void weekComboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (weekComboBox2.SelectedIndex == -1)
            {
                File.Delete(".\\data\\time.dat");
                return;
            }
            int week = weekComboBox2.SelectedIndex + 1;
            DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;
            File.Create(".\\data\\time.dat").Close();
            StreamWriter writer = new StreamWriter(".\\data\\time.dat");
            writer.Write(DateTime.Today.AddDays(-(7 * (week - 1) + ((int)dayOfWeek - 1))).ToShortDateString());
            writer.Close();

            weekComboBox2.IsEnabled = false;
            resetButton.IsEnabled = true;
            chooseWeekLabel.Content = "当前周数为:";



        }
        private void scoreTabitem_MouseDown(object sender,MouseButtonEventArgs e)
        {

            if (!File.Exists(".\\data\\score.mt"))
                return;
            StreamReader reader = new StreamReader(".\\data\\score.mt");
            var scoreTuple = HtmlToTuple.ScoreConverter(reader.ReadToEnd());
            reader.Close();
            List<Scores> scoreList = new List<Scores>();
            foreach (var item in scoreTuple.Item2)
            {
                scoreList.Add(new Scores(item[0], item[1], item[2], item[3], item[4], item[5]));
            }
            scoreListView.ItemsSource = scoreList;
        }
        private void creditTabitem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!File.Exists(".\\data\\credit.mt"))
                return;
            StreamReader reader = new StreamReader(".\\data\\credit.mt");
            var creditTuple = HtmlToTuple.CreditConverter(reader.ReadToEnd());
            reader.Close();
            List<Credits> creditList = new List<Credits>();
            foreach (var item in creditTuple.Item2)
            {
                creditList.Add(new Credits(item[0], item[1], item[2], item[3], item[4], item[5],item[6],item[7],item[8]));
            }
            creditListView.ItemsSource = creditList;
        }
        private void scoreStackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            scoreTabitem_MouseDown(sender, e);
        }
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            creditTabitem_MouseDown(sender, e);
        }
        private void RandomizedLabel()
        {
            Random random = new Random();
            string[] label = { "列表加载不出来？再点一次吧~By CharmBracelet",
                                         "遇到Bug了？那么请注销试试吧？ By CharmBracelet",
                                        "没错，我的存在没什么必要...",
                                        "你今天有没有好好上课呢？",
                                        "说不定你的成绩能被我看到？！",
                                        @"“我也想在Android和iOS上出现！“（你别想了" };
            statusLabel.Content = label[random.Next(0, 5)];
        }
        private void myImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            clickTime++;
            if (clickTime >= 5)
            {
                clickTime = 0;
                hiddenImage.Visibility = Visibility.Visible;
            }
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            todayScheduleTabitem_MouseDown(sender,e
        );
        }

        private void todayScheduleTabitem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!File.Exists(".\\data\\time.dat"))
                return;
            weekComboBox2.IsEnabled = false;
            resetButton.IsEnabled = true;
            List<string> header = new List<string> { "第1,2节", "第3,4节", "第5,6节", "第7,8节", "晚上" };
            StreamReader reader = new StreamReader(".\\data\\time.dat");
            DateTime firstDayOfSemester = DateTime.Parse(reader.ReadToEnd());
            reader.Close();
            DateTime today = DateTime.Now.Date;
            int span = today.Subtract(firstDayOfSemester).Days;
            int remainer = span % 7;
            reader = new StreamReader(".\\data\\CC-" + semesterToPara.Last().Value + ".mt");
            var courseSchedule = HtmlToTuple.CourseScheduleConverter(
                reader.ReadToEnd());
            reader.Close();
            List<TodaySchedule> todaySchedules = new List<TodaySchedule>();
            int temp = 0, count = 0;
            foreach (var time in courseSchedule.Item2[span / 7])
            {
                todaySchedules.Add(new TodaySchedule(header[temp], time[remainer]));
                temp++;
                if (time[remainer] != null)
                    count++;
            }
            todayScheduleListView.ItemsSource = todaySchedules;
            todayLabel.Content = "今天是" + today.ToShortDateString() + " " + today.DayOfWeek + ", 你有" + count + "节课";
        }
    }
}
