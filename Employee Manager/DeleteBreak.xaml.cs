using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Employee_Manager.Classes;

namespace Employee_Manager
{
    public partial class DeleteBreak : Window
    {
        Break B = new Break();

        public DeleteBreak(Break b)
        {
            InitializeComponent();
            B = b;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ReasonTxt.Text == "")
            {
                MessageBox.Show("You Must Fill A Reason.", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "EmployeeSMT", "aoi", "$Flex2016");
            string query = string.Format(@"INSERT INTO Break_Deleted (WN, Name, StartTime, EndTime, Note, DeleteBy) VALUES('{0}', N'{1}', '{2}', '{3}', N'{4}', N'{5}')",
                                            B.WN, B.Name, B.BreakStart_Full.ToString("yyyy-MM-dd HH:mm:ss"), B.BreakEnd_Full.ToString("yyyy-MM-dd HH:mm:ss"), ReasonTxt.Text, MainWindow.LogedUser.Name);
            sql.InsertNonQuery(query);
            sql = new SQLClass("MIGSQLCLU4\\SMT", "HC_Visualisation", "aoi", "$Flex2016");
            query = string.Format("DELETE FROM BreakRegister WHERE ID ='{0}'", B.Break_ID);
            sql.Update(query);
            Close();
        }
    }
}
