using Employee_Manager.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Employee_Manager
{
    public partial class NotRported_UC : UserControl
    {
        List<InOut> list = new List<InOut>();
        List<Break> list2 = new List<Break>();
        InOut Current;

        public NotRported_UC()
        {
            InitializeComponent();
            dp1.SelectedDate = DateTime.Now;
            GetData();
        }

        private void GetData()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Clear();
            SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "EmployeeSMT", "aoi", "$Flex2016");
            SQLClass sql2 = new SQLClass("MIGSQLCLU4\\SMT", "HC_Visualisation", "aoi", "$Flex2016");
            DateTime Today = (DateTime)dp1.SelectedDate;
            Today = new DateTime(Today.Year, Today.Month, Today.Day, 07, 00, 00);
            DateTime Tommorow = Today.AddDays(+1);
            Tommorow = new DateTime(Tommorow.Year, Tommorow.Month, Tommorow.Day, 07, 00, 00);
            string qry = string.Format(@"SELECT A.Date, A.WN, A.Name, A.Department, A.SMT_In, A.SMT_Out, B.LineName, CAST(B.StartDate as time), CAST(B.EndDate as time)
                                         FROM [EmployeeSMT].[dbo].[Employee_InOut] A
                                         LEFT JOIN [HC_Visualisation].[dbo].[PresenceRegister] B
                                         ON A.WN = B.HR AND CAST(A.Date as date)  = CAST(B.StartDate as date)
										 WHERE CAST(A.Date as date)='{0}' AND A.Deleted is null
                                         ORDER BY A.Date DESC", dp1.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            list = sql.SelectInOut(qry);

            qry = string.Format(@"SELECT A.ID, A.StartDate, A.EndDate, B.FirstName+' '+ B.LastName, B.HR, B.LineName
                                         FROM BreakRegister AS A
                                         LEFT Join PresenceRegister AS B ON A.PresenceID=B.ID
                                         WHERE A.StartDate>='{0}' AND A.StartDate<'{1}'
                                         ORDER BY A.StartDate ASC", Today.ToString("yyyy-MM-dd HH:mm:ss"), Tommorow.ToString("yyyy-MM-dd HH:mm:ss"));
            list2 = sql2.SelectBreaks(qry);
            Grid1.ItemsSource = list.Where(x => x.Line == "").OrderBy(x => x.WN);
            Grid2.ItemsSource = list.Where(x => x.Line != "" && !list2.Exists(y => y.WN == x.WN)).OrderBy(x => x.WN);
            Mouse.OverrideCursor = null;
        }

        private void Grid_SelectionChanged(object sender, SelectedCellsChangedEventArgs e)//when the user press on specific row
        {
            DataGrid dg = (DataGrid)sender;
            Current = (InOut)dg.SelectedItem;
            if (Current != null)
            {
                WNTxt.Text = Current.WN;
                NameTxt.Text = Current.Name;
                InWorkTxt.Text = Current.In_Work;
                OutWorkTxt.Text = Current.Out_Work;
                InLineTxt.Text = Current.In_Line;
                OutLineTxt.Text = Current.Out_Line;
                LineTxt.Text = Current.Line;
                DepTxt.Text = Current.Department;
                GetPhoto(Current.WN);
            }
        }

        private void GetPhoto(string p)//get the photo of the operator
        {
            try
            {
                ProfileElipse.Visibility = Visibility.Visible;
                string path = @"\\mignt002\Private\falcon\" + p + ".jpg";
                BitmapImage bitmap = new BitmapImage();
                if (File.Exists(path))
                {
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path);
                    bitmap.EndInit();
                    ProfileImg.ImageSource = bitmap;
                }
                else
                {
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(@"pack://application:,,,/Resources/profile.jpg", UriKind.Absolute);
                    bitmap.EndInit();
                    ProfileImg.ImageSource = bitmap;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

        }

        private void Clear()
        {
            NameTxt.Text = "";
            LineTxt.Text = "";
            WNTxt.Text = "";
            InWorkTxt.Text = "";
            OutWorkTxt.Text = "";
            InLineTxt.Text = "";
            OutLineTxt.Text = "";
            DepTxt.Text = "";
            ProfileElipse.Visibility = Visibility.Hidden;
            Current = null;
        }


        private void WNTxtBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var filtered = list.Where(x => x.WN.StartsWith(WNTxt.Text));
            Grid1.ItemsSource = filtered;
        }

        private void NameTxtBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var filtered = list.Where(x => x.Name.ToLower().StartsWith(NameTxt.Text.ToLower()));
            Grid1.ItemsSource = filtered;
        }

        private void dp1_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dp1_CalendarClosed(object sender, RoutedEventArgs e)
        {
            GetData();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (list.Count == 0) return;

            ComboBoxItem cbi = (ComboBoxItem)DepCB.SelectedItem;
            string Dep = cbi.Content.ToString();
            IEnumerable<InOut> filtered = Enumerable.Empty<InOut>();
            IEnumerable<InOut> filtered2 = Enumerable.Empty<InOut>();

            if (Dep == "Warehouse")
            {
                filtered = list.Where(x => x.Department.Contains("מחסן") && x.Line == "");
                filtered2 = list.Where(x => x.Department.Contains("מחסן") && x.Line != "" && !list2.Exists(y => y.WN == x.WN));
            }
            else if (Dep == "Lines")
            {
                filtered = list.Where(x => x.Department.Contains("SMT") && !x.Department.Contains("מחסן") && x.Line == "");
                filtered2 = list.Where(x => x.Department.Contains("SMT") && !x.Department.Contains("מחסן") && x.Line != "" && !list2.Exists(y => y.WN == x.WN));
            }
            else if (Dep == "Setup")
            {
                filtered = list.Where(x => x.Department.Contains("SET") && x.Line == "");
                filtered2 = list.Where(x => x.Department.Contains("SET") && x.Line != "" && !list2.Exists(y => y.WN == x.WN));

            }
            else if (Dep == "Wave soldering")
            {
                filtered = list.Where(x => x.Department.Contains("הלחמת") && x.Line == "");
                filtered2 = list.Where(x => x.Department.Contains("הלחמת") && x.Line != "" && !list2.Exists(y => y.WN == x.WN));
            }
            else if (Dep == "All")
            {
                filtered = list.Where(x => x.Line == "");
                filtered2 = list.Where(x => x.Line != "" && !list2.Exists(y => y.WN == x.WN));
            }

            Grid1.ItemsSource = filtered;
            Grid2.ItemsSource = filtered2;
        }
    }
}
