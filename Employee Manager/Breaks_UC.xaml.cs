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
    public partial class Breaks_UC : UserControl
    {
        List<Break> list = new List<Break>();
        Break Current = new Break();

        public Breaks_UC()
        {
            InitializeComponent();
            dp1.SelectedDate = DateTime.Now;
            GetData();
        }

        private void GetData()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "HC_Visualisation", "aoi", "$Flex2016");
            DateTime Today = (DateTime)dp1.SelectedDate;
            Today = new DateTime(Today.Year, Today.Month, Today.Day, 07, 00, 00);
            DateTime Tommorow = Today.AddDays(+1);
            Tommorow = new DateTime(Tommorow.Year, Tommorow.Month, Tommorow.Day, 07, 00, 00);

            string qry = string.Format(@"SELECT A.ID, A.StartDate, A.EndDate, B.FirstName+' '+ B.LastName, B.HR, B.LineName
                                         FROM BreakRegister AS A
                                         LEFT Join PresenceRegister AS B ON A.PresenceID=B.ID
                                         WHERE A.StartDate>='{0}' AND A.StartDate<'{1}'
                                         ORDER BY A.StartDate ASC", Today.ToString("yyyy-MM-dd HH:mm:ss"), Tommorow.ToString("yyyy-MM-dd HH:mm:ss"));
            list = sql.SelectBreaks(qry);
            Clear();
            Grid1.ItemsSource = list;
            Mouse.OverrideCursor = null;
        }

        private void Grid_SelectionChanged(object sender, SelectedCellsChangedEventArgs e)//when the user press on specific row
        {
            Current = (Break)Grid1.SelectedItem;
            if (Current != null)
            {
                WNTxt.Text = Current.WN;
                NameTxt.Text = Current.Name;
                InTxt.Text = Current.BreakStart;
                OutTxt.Text = Current.BreakEnd;
                DurationTxt.Text = Current.Duration.ToString() + " Min";
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

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }

        private void Clear()
        {
            NameTxt.Text = "";
            DurationTxt.Text = "";
            WNTxt.Text = "";
            InTxt.Text = "";
            OutTxt.Text = "";
            ProfileElipse.Visibility = Visibility.Hidden;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (WNTxt.Text != "")
            {
                DeleteBreak Del = new DeleteBreak(Current);
                Del.ShowDialog();
                GetData();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (WNTxt.Text != "" && OutTxt.Text.Contains(":"))
            {
                SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "HC_Visualisation", "aoi", "$Flex2016");
                DateTime Today = (DateTime)dp1.SelectedDate;
                int h = Int32.Parse(OutTxt.Text.Substring(0, OutTxt.Text.IndexOf(":")));
                int m = Int32.Parse(OutTxt.Text.Substring(OutTxt.Text.IndexOf(":") + 1));
                if (Current.BreakEnd_Full.Year == 1)
                    Today = new DateTime(Today.Year, Today.Month, Today.Day, h, m, 00);

                else
                    Today = new DateTime(Current.BreakEnd_Full.Year, Current.BreakEnd_Full.Month, Current.BreakEnd_Full.Day, h, m, 00);
                string query = string.Format("Update BreakRegister SET EndDate='{1}' WHERE ID ='{0}'", Current.Break_ID, Today.ToString("yyyy-MM-dd HH:mm:ss"));
                sql.Update(query);
                GetData();
            }
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
            GetData();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (list.Count == 0) return;

            ComboBoxItem cbi = (ComboBoxItem)DepCB.SelectedItem;
            string Dep = cbi.Content.ToString();
            IEnumerable<Break> filtered = Enumerable.Empty<Break>();

            if (Dep == "Warehouse")
                filtered = list.Where(x => x.Department.Contains("WAREHOUSE"));
            else if (Dep == "Lines")
                filtered = list.Where(x => x.Department.ToLower().Contains("line") || x.Department.ToLower().Contains("smt") || x.Department.ToLower().Contains("support"));
            else if (Dep == "Setup")
                filtered = list.Where(x => x.Department.ToLower().Contains("setup") && !x.Department.ToLower().Contains("wave"));
            else if (Dep == "Wave soldering")
                filtered = list.Where(x => x.Department.ToLower().Contains("wave"));
            else if (Dep == "All")
                filtered = list;
            Grid1.ItemsSource = filtered;
        }
    }
}
