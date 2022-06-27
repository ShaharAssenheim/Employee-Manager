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
    public partial class EmpInOut : UserControl
    {
        List<InOut> list = new List<InOut>();
        InOut Current;
        bool IsModal = false;

        public EmpInOut()
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
            Grid1.ItemsSource = list.OrderBy(x => x.WN);
            Mouse.OverrideCursor = null;

        }

        private void Grid_SelectionChanged(object sender, SelectedCellsChangedEventArgs e)//when the user press on specific row
        {
            Current = (InOut)Grid1.SelectedItem;
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

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (Current != null)
            {
                var result = MessageBox.Show("Are you sure you want to delete?", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk);
                if (result == MessageBoxResult.Yes)
                {
                    SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "EmployeeSMT", "aoi", "$Flex2016");
                    string query = string.Format("UPDATE Employee_InOut SET Deleted='1' WHERE WN ='{0}' AND CAST(Date as date)='{1}'", WNTxt.Text, Current.Date.ToString("yyyy-MM-dd"));
                    sql.Update(query);
                    MessageBox.Show("Delete Successfully!", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            GetData();
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

            if (Dep == "Warehouse")
                filtered = list.Where(x => x.Department.Contains("מחסן"));
            else if (Dep == "Lines")
                filtered = list.Where(x => x.Department.Contains("SMT") && !x.Department.Contains("מחסן"));
            else if (Dep == "Setup")
                filtered = list.Where(x => x.Department.Contains("SET"));
            else if (Dep == "Wave soldering")
                filtered = list.Where(x => x.Department.Contains("הלחמת"));
            else if (Dep == "All")
                filtered = list;
            Grid1.ItemsSource = filtered;
        }

        private void Wave_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (IsModal)
            {
                var window = Application.Current.Windows.OfType<Window>().Where(w => w.Title == "Wave Shift Duration");
                foreach(var i in window)
                {
                    i.Close();
                }
                IsModal = false;
                Wave_Btn.Background = Brushes.White;
            }
            else
            {
                Wave W = new Wave(dp1.SelectedDate.Value.Date);
                W.Show();
                IsModal = true;
                Wave_Btn.Background = Brushes.LightBlue;
            }
        }
    }
}
