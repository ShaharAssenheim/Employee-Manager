using Employee_Manager.Classes;
using System;
using System.Collections.Generic;
using System.Data;
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
    public partial class MainWindow : Window
    {
        public static List<Worker> ExceptionalList = new List<Worker>();
        public static List<string> Authorized = new List<string>() { "17342", "7373", "7667", "8466", "18483", "17941", "4226", "19034", "5940", "1135", "11496", "10674", "1011", "4179" };
        //                                                            Shahar,  Max,    Alla,   Elik,   Danit,    Ori,   Ruslan,  Maxim,   Mati,   Yuri,  Sergey,   Nir,   Miroslav  yaakov
        public static Worker LogedUser = new Worker();
        public static MainWindow mn;

        public MainWindow()
        {
            InitializeComponent();
            mn = this;
            Get_Exceptional();
            Login();
        }

        private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListViewMenu.SelectedItem == null) return;

            int index = ListViewMenu.SelectedIndex;
            MoveCursorMenu(index);

            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "EMp_List":
                    MainGrid.Children.Clear();
                    MainGrid.Children.Add(new EmpList_UC());
                    break;
                case "Line_Scan":
                    MainGrid.Children.Clear();
                    MainGrid.Children.Add(new EmpInOut());
                    break;
                case "Breaks":
                    MainGrid.Children.Clear();
                    MainGrid.Children.Add(new Breaks_UC());
                    break;
                case "MissBreaks":
                    MainGrid.Children.Clear();
                    MainGrid.Children.Add(new NotRported_UC());
                    break;
                case "Reports":
                    MainGrid.Children.Clear();
                    MainGrid.Children.Add(new Reports_UC());
                    break;
                case "ManPower":
                    MainGrid.Children.Clear();
                    MainGrid.Children.Add(new ManPower_UC());
                    break;
                default:
                    break;
            }
            ListViewMenu.SelectedItem = null;
        }

        private void Login()
        {
            Login log = new Login();
            log.ShowDialog();
        }

        private void MoveCursorMenu(int index)
        {
            TrainsitionigContentSlide.OnApplyTemplate();
            GridCursor.Margin = new Thickness(0, (25 + (60 * index)), 0, 0);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Get_Exceptional()
        {
            SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "EmployeeSMT", "aoi", "$Flex2016");
            string qry = string.Format(@"SELECT * FROM Exceptional_Workers");
            DataTable dt = sql.SelectDb(qry);
            foreach (DataRow row in dt.Rows)
            {
                Worker W = new Worker() { WN = row["WN"].ToString().Trim(), Name = row["Name"].ToString().Trim() };
                ExceptionalList.Add(W);
            }
        }

        public static void TrimDataTable(DataTable dt)
        {
            foreach (DataColumn dc in dt.Columns) // trim column names
            {
                dc.ColumnName = dc.ColumnName.Trim();
            }

            foreach (DataRow dr in dt.Rows) // trim string data
            {
                foreach (DataColumn dc in dt.Columns)
                {
                    if (dc.DataType == typeof(string))
                    {
                        object o = dr[dc];
                        if (!Convert.IsDBNull(o) && o != null)
                        {
                            dr[dc] = o.ToString().Trim();
                        }
                    }
                }
            }
        }
    }
}
