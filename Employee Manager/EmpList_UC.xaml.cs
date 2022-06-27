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
    public partial class EmpList_UC : UserControl
    {
        List<Worker> list = new List<Worker>();

        public EmpList_UC()
        {
            InitializeComponent();
            GetData();
        }

        private void GetData()
        {
            SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "AOI", "aoi", "$Flex2016");
            Clear();
            string qry = string.Format(@"SELECT A.work_number, A.name, A.shift, A.password, A.lvl, B.StartDate, B.TotalYears
                                         FROM [AOI].[dbo].[employers] A
                                         LEFT JOIN [EmployeeSMT].[dbo].[SMT_ManPower] B ON RTRIM(LTRIM(A.work_number)) = RTRIM(LTRIM(B.WN))");
            list = sql.SelectWorkers(qry);
            Grid1.ItemsSource = list;

        }

        private void Grid_SelectionChanged(object sender, SelectedCellsChangedEventArgs e)//when the user press on specific row
        {
            Worker row = (Worker)Grid1.SelectedItem;
            if (row != null)
            {
                NameTxt.Text = row.Name;
                ShiftTxt.Text = row.Shift;
                WNTxt.Text = row.WN;
                PassTxt.Text = row.Password;
                LevelTxt.Text = row.Level.ToString();
                GetPhoto(row.WN);
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

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (WNTxt.Text != "")
            {
                SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "AOI", "aoi", "$Flex2016");

                if (list.Any(x => x.WN == WNTxt.Text))
                {
                    MessageBox.Show("There Is An Employee With That Work Number!!!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string qry = string.Format(@"INSERT INTO employers (work_number, name, shift, password, lvl)
                                             VALUES('{0}',N'{1}','{2}','{3}','{4}')", WNTxt.Text, NameTxt.Text.Replace("'", "''"), ShiftTxt.Text, PassTxt.Text, LevelTxt.Text);
                sql.InsertNonQuery(qry);
                GetData();
                MessageBox.Show("The Employee Added Succesfully.", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }

        private void Clear()
        {
            NameTxt.Text = "";
            ShiftTxt.Text = "";
            WNTxt.Text = "";
            PassTxt.Text = "";
            LevelTxt.Text = "";
            ProfileElipse.Visibility = Visibility.Hidden;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (WNTxt.Text != "")
            {
                var result = MessageBox.Show("Are you sure you want to delete?", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk);
                if (result == MessageBoxResult.Yes)
                {
                    string query = string.Format("DELETE FROM employers WHERE work_number ='{0}'", WNTxt.Text);
                    SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "AOI", "aoi", "$Flex2016");
                    sql.Update(query);
                    MessageBox.Show("The Employee Deleted Succesfully.", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            GetData();
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            string query = string.Format("UPDATE employers SET name=N'{0}',shift='{1}',password='{2}',lvl='{4}' WHERE work_number='{3}'", NameTxt.Text.Replace("'", "''"), ShiftTxt.Text, PassTxt.Text, WNTxt.Text, LevelTxt.Text);
            SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "AOI", "aoi", "$Flex2016");
            sql.Update(query);
            MessageBox.Show("The Employee Updated Succesfully.", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
            GetData();
        }

        private void WNTxtBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var filtered = list.Where(x => x.WN.StartsWith(WNTxt.Text));
            Grid1.ItemsSource = filtered;
        }

        private void LvlTxtBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var filtered = list.Where(x => x.Level.ToString() == LevelTxt.Text);
            Grid1.ItemsSource = filtered;
        }

        private void ShiftTxt_KeyUp(object sender, KeyEventArgs e)
        {
            var filtered = list.Where(x => x.Shift.ToLower().StartsWith(ShiftTxt.Text.ToLower()));
            Grid1.ItemsSource = filtered;
        }

        private void NameTxt_KeyUp(object sender, KeyEventArgs e)
        {
            var filtered = list.Where(x => x.Name.ToLower().StartsWith(NameTxt.Text.ToLower()));
            Grid1.ItemsSource = filtered;
        }
    }
}
