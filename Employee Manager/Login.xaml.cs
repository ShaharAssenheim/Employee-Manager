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
using System.Windows.Shapes;
using Employee_Manager.Classes;

namespace Employee_Manager
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            FocusManager.SetFocusedElement(this, UserNameTxt);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "AOI", "aoi", "$Flex2016");
            string qry = string.Format(@"SELECT A.work_number, A.name, A.shift, A.password, A.lvl, B.StartDate, B.TotalYears
                                         FROM [AOI].[dbo].[employers] A
                                         LEFT JOIN [EmployeeSMT].[dbo].[SMT_ManPower] B ON RTRIM(LTRIM(A.work_number)) = RTRIM(LTRIM(B.WN)) 
                                         WHERE A.work_number='{0}' AND A.password='{1}'", UserNameTxt.Text, PasswordTxt.Password);
            List<Worker> list = sql.SelectWorkers(qry);

            if (list.Count > 0)
            {
                if (MainWindow.Authorized.Exists(x => x == list[0].WN))
                {
                    MainWindow.LogedUser = list[0];
                    SetPhoto(list[0].WN);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Hello " + list[0].Name + "\nCurrently You Not Allowed To Logged In This Application.", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    UserNameTxt.Text = "";
                    PasswordTxt.Password = "";
                    return;
                } 
            }
            else
            {
                MessageBox.Show("Invalid Username or Password!", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                UserNameTxt.Text = "";
                PasswordTxt.Password = "";
                return;
            }
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "AOI", "aoi", "$Flex2016");
                string qry = string.Format(@"SELECT A.work_number, A.name, A.shift, A.password, A.lvl, B.StartDate, B.TotalYears
                                         FROM [AOI].[dbo].[employers] A
                                         LEFT JOIN [EmployeeSMT].[dbo].[SMT_ManPower] B ON RTRIM(LTRIM(A.work_number)) = RTRIM(LTRIM(B.WN)) 
                                         WHERE A.work_number='{0}' AND A.password='{1}'", UserNameTxt.Text, PasswordTxt.Password);
                List<Worker> list = sql.SelectWorkers(qry);

                if (list.Count > 0)
                {
                    if (MainWindow.Authorized.Exists(x => x == list[0].WN))
                    {
                        MainWindow.LogedUser = list[0];
                        SetPhoto(list[0].WN);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Hello " + list[0].Name + "\nCurrently You Not Allowed To Logged In This Application.", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        UserNameTxt.Text = "";
                        PasswordTxt.Password = "";
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Username or Password!", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    UserNameTxt.Text = "";
                    PasswordTxt.Password = "";
                    return;
                }
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SetPhoto(string p)//get the photo of the operator
        {
            try
            {
                string path = @"\\mignt002\Private\falcon\" + p + ".jpg";
                BitmapImage bitmap = new BitmapImage();
                if (File.Exists(path))
                {
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path);
                    bitmap.EndInit();
                    MainWindow.mn.ProfileImage.ImageSource = bitmap;
                    MainWindow.mn.ProfileElipse.ToolTip = MainWindow.LogedUser.Name;
                }
                else
                {
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(@"pack://application:,,,/Resources/profile.jpg", UriKind.Absolute);
                    bitmap.EndInit();
                    MainWindow.mn.ProfileImage.ImageSource = bitmap;
                    MainWindow.mn.ProfileElipse.ToolTip = "";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

        }
    }
}
