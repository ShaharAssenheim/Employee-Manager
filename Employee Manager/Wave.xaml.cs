using Employee_Manager.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Employee_Manager
{
    public partial class Wave : Window
    {
        DateTime CurrentDT;
        List<WaveShift> list = new List<WaveShift>();
        public Wave(DateTime dt)
        {
            InitializeComponent();
            Point position = MainWindow.mn.PointToScreen(new Point(0d, 0d));
            Left = position.X + 210;
            Top = position.Y + 160;
            CurrentDT = dt;
            GetData();
        }

        private void GetData()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Clear();
            SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "Wave_Soldering", "aoi", "$Flex2016");

            string qry = string.Format(@"SELECT distinct Shift_Date, A.WN, B.Name, Shift_Duration, Break_Duration
                                      FROM [Wave_Soldering].[dbo].[Shift_Times] A
                                      LEFT JOIN [EmployeeSMT].[dbo].[Employee_InOut] B on A.WN = B.WN
                                      WHERE Shift_Date='{0}'", CurrentDT.ToString("yyyy-MM-dd"));
            list = sql.WaveShift(qry);
            WaveGrid.ItemsSource = list.OrderBy(x => x.WN);
            Mouse.OverrideCursor = null;
        }

        private void Clear()
        {
            NameTxt.Text = "";
            WNTxt.Text = "";
            ShiftTxt.Text = "";
            BreakTxt.Text = "";
            ProfileElipse.Visibility = Visibility.Hidden;
        }

        private void Grid_SelectionChanged(object sender, SelectedCellsChangedEventArgs e)//when the user press on specific row
        {
            WaveShift W = (WaveShift)WaveGrid.SelectedItem;
            if (W != null)
            {
                WNTxt.Text = W.WN;
                NameTxt.Text = W.Name;
                ShiftTxt.Text = W.Shift_Duration.ToString();
                BreakTxt.Text = W.Break_Duration.ToString();
                GetPhoto(W.WN);
            }
        }


        private void WNTxtBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var filtered = list.Where(x => x.WN.StartsWith(WNTxt.Text));
            WaveGrid.ItemsSource = filtered;
        }

        private void NameTxtBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var filtered = list.Where(x => x.Name.ToLower().StartsWith(NameTxt.Text.ToLower()));
            WaveGrid.ItemsSource = filtered;
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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (WNTxt.Text != "")
            {
                string qry = string.Format(@"UPDATE Shift_Times 
                                           SET Shift_Duration='{0}', Break_Duration='{1}'
                                           WHERE Shift_Date='{2}' AND WN='{3}'",
                                           ShiftTxt.Text, BreakTxt.Text, CurrentDT.ToString("yyyy-MM-dd"), WNTxt.Text);

                SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "Wave_Soldering", "aoi", "$Flex2016");
                sql.Update(qry);
            }
        }

        private void ShiftTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void BreakTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (WNTxt.Text != "")
            {
                var result = MessageBox.Show("Are you sure you want to delete?", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk);
                if (result == MessageBoxResult.Yes)
                {
                    SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "Wave_Soldering", "aoi", "$Flex2016");
                    string query = string.Format("DELETE FROM Shift_Times WHERE WN ='{0}' AND Shift_Date = '{1}'", WNTxt.Text, CurrentDT.ToString("yyyy-MM-dd"));
                    sql.Update(query);
                    MessageBox.Show("Delete Successfully!", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            GetData();
        }
    }
}
