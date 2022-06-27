using Employee_Manager.Classes;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
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
    public partial class ManPower_UC : UserControl
    {


        public ManPower_UC()
        {
            InitializeComponent();
            RefreshTable();

        }

        private void RefreshTable()
        {
            SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "EmployeeSMT", "aoi", "$Flex2016");

            string qry = string.Format(@"SELECT Name, WN, Shift, MainRole, RoleType, BackUpRole, BackUpRole2, NextRole, LastUpdate
                                         FROM SMT_ManPower
                                         order by RoleType asc");
            DataTable dt1 = sql.SelectDb(qry);
            MainWindow.TrimDataTable(dt1);

            qry = string.Format(@"SELECT distinct MainRole, RoleType, RoleCapacity, BackUpCapacity, NextCapacity
                                  FROM SMT_ManPower
                                  order by RoleType asc");
            DataTable dt2 = sql.SelectDb(qry);
            MainWindow.TrimDataTable(dt2);

            Grid1.DataContext = dt1.DefaultView;
            Grid2.DataContext = dt2.DefaultView;

            DataRow LineRow = dt1.AsEnumerable().First(r => r.Field<string>("RoleType") == "Lines");
            DataRow SetupRow = dt1.AsEnumerable().First(r => r.Field<string>("RoleType") == "Setup");
            DataRow ProcessRow = dt1.AsEnumerable().First(r => r.Field<string>("RoleType") == "Process");
            DataRow MaintenanceRow = dt1.AsEnumerable().First(r => r.Field<string>("RoleType") == "Maintenance");
            DataRow RuslanRow = dt1.AsEnumerable().First(r => r.Field<string>("RoleType") == "Ruslan-Team");
            DataRow ITRow = dt1.AsEnumerable().First(r => r.Field<string>("RoleType") == "SMT-IT");

            Lines_Update.Content = "Lines: " + LineRow.ItemArray[8].ToString();
            Setup_Update.Content = "Setup: " + SetupRow.ItemArray[8].ToString();
            Process_Update.Content = "Process: " + ProcessRow.ItemArray[8].ToString();
            Maintenance_Update.Content = "Maintenance: " + MaintenanceRow.ItemArray[8].ToString();
            Ruslan_Update.Content = "Ruslan: " + RuslanRow.ItemArray[8].ToString();
            IT_Update.Content = "SMT IT: " + ITRow.ItemArray[8].ToString();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string Path = MainWindow.LogedUser.WN == "7667" ? @"\\mignt002\public\smt1\ALLA\\" : @"\\mignt002\public\smt8\Maxim\\";
                OpenFileDialog theDialog = new OpenFileDialog();
                theDialog.Title = "Open Text File";
                theDialog.Filter = "All files (*.*)|*.*";
                theDialog.InitialDirectory = Path;
                if (theDialog.ShowDialog() == true)
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    FileInfo existingFile = new FileInfo(theDialog.FileName);
                    List<ManPower> list = new List<ManPower>();
                    using (ExcelPackage package = new ExcelPackage(existingFile))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.End.Row;
                        int colCount = worksheet.Dimension.End.Column;
                        SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "EmployeeSMT", "aoi", "$Flex2016");

                        for (int row = 2; row <= rowCount; row++)
                        {
                            if (worksheet.Cells[row, 1].Value?.ToString() == null) continue;
                            ManPower M = new ManPower();
                            M.Name = worksheet.Cells[row, 1].Value?.ToString().Trim().Replace("'", string.Empty) + " " + worksheet.Cells[row, 2].Value?.ToString().Trim().Replace("'", string.Empty); ;
                            M.WN = worksheet.Cells[row, 3].Value?.ToString().Trim();

                            M.Shift = worksheet.Cells[row, 4].Value?.ToString().Trim();
                            M.MainRole = worksheet.Cells[row, 5].Value?.ToString().Trim().Replace("'", string.Empty).Replace("\"", string.Empty);
                            M.RoleType = (M.MainRole == "AOI" || M.MainRole == "QC" || M.MainRole == "מפעיל") ? "Lines" :
                                         (M.MainRole == "X-RAY" || M.MainRole == "Process" || M.MainRole == "איש נפל" || M.MainRole == "אח משמרת" || M.MainRole == "מלחים/ה" || M.MainRole == "איש חומר" || M.MainRole == "מפעיל מסכות" || M.MainRole == "QC משמרת" || M.MainRole == "QC Training") ? "מעטפת" :
                                         (M.MainRole == "ראשצ" || M.MainRole == "סידור גלילים" || M.MainRole == "אופטימזציה" || M.MainRole == "הרכבה" || M.MainRole == "פירוק" || M.MainRole == "מדבקות" || M.MainRole == "ספירה x ray" || M.MainRole == "ורפיקציה" || M.MainRole == "שינוע" || M.MainRole == "full") ? "Setup" :
                                         theDialog.FileName.Contains("SMT_IT") ? "SMT-IT" :
                                         theDialog.FileName.Contains("Maintenance") ? "Maintenance" :
                                         theDialog.FileName.Contains("Ruslan") ? "Ruslan-Team" :
                                         theDialog.FileName.Contains("Process") ? "Process" : "";
                            M.BackUpRole = worksheet.Cells[row, 6].Value?.ToString().Trim();
                            M.BackUpRole2 = worksheet.Cells[row, 7].Value?.ToString().Trim();
                            M.NextRole = worksheet.Cells[row, 8].Value?.ToString().Trim();

                            if (worksheet.Cells[row, 9].Value?.ToString() == null|| worksheet.Cells[row, 9].Value?.ToString() =="")
                                M.StartDate = "";
                            else
                                M.StartDate = Convert.ToDateTime(worksheet.Cells[row, 9].Value?.ToString()).ToString("yyyy-MM-dd");

                            M.TotalYears = worksheet.Cells[row, 10].Value?.ToString().Trim();
                            M.TotalYears = M.TotalYears == null ? "" : M.TotalYears.Length > 5 ? M.TotalYears.Substring(0, 5) : M.TotalYears;
                            M.RoleCapacity = M.MainRole == "AOI" ? 44 :
                                             M.MainRole == "QC" ? 44 :
                                             M.MainRole == "QC Training" ? 1 :
                                             M.MainRole == "מפעיל" ? 8 :
                                             M.MainRole == "X-RAY" ? 2 :
                                             M.MainRole == "Process" ? 6 :
                                             M.MainRole == "איש נפל" ? 1 :
                                             M.MainRole == "אח משמרת" ? 3 :
                                             M.MainRole == "מלחים/ה" ? 2 :
                                             M.MainRole == "איש חומר" ? 2 :
                                             M.MainRole == "מפעיל מסכות" ? 2 :
                                             M.MainRole == "QC משמרת" ? 2 :
                                             M.MainRole == "אופטימזציה" ? 2 :
                                             M.MainRole == "הרכבה" ? 12 :
                                             M.MainRole == "ורפיקציה" ? 2 :
                                             M.MainRole == "מדבקות" ? 2 :
                                             M.MainRole == "משנע" ? 2 :
                                             M.MainRole == "סידור גלילים" ? 2 :
                                             M.MainRole == "ספירה x ray" ? 2 :
                                             M.MainRole == "פירוק" ? 6 :
                                             M.MainRole == "ראשצ" ? 2 :
                                             M.MainRole == "ראש צוות" && theDialog.FileName.Contains("SMT_IT") ? 1 :
                                             M.MainRole == "תכנת" && theDialog.FileName.Contains("SMT_IT") ? 5 :
                                             M.MainRole == "מנהל אחזקה" && theDialog.FileName.Contains("Maintenance") ? 1 :
                                             M.MainRole == "ראש צוות" && theDialog.FileName.Contains("Maintenance") ? 3 :
                                             M.MainRole.Contains("מחסן אחזקה") && theDialog.FileName.Contains("Maintenance") ? 1 :
                                             M.MainRole == "טכנאי" && theDialog.FileName.Contains("Maintenance") ? 8 :
                                             M.MainRole == "טכנאי כללי" && theDialog.FileName.Contains("Maintenance") ? 2 :
                                             M.MainRole == "מהנדס תהליך" && theDialog.FileName.Contains("Ruslan") ? 2 :
                                             M.MainRole.Contains("AOI") && theDialog.FileName.Contains("Ruslan") ? 9 :
                                             M.MainRole == "Process support" && theDialog.FileName.Contains("Process") ? 11 :
                                             M.MainRole == "Programer" && theDialog.FileName.Contains("Process") ? 2 :
                                             0;
                            M.BackUpCapacity = 0;
                            M.NextCapacity = 0;
                            list.Add(M);
                        }

                        string qry = "";

                        if (theDialog.FileName.Contains("SMT_IT"))
                            qry = string.Format(@"DELETE FROM SMT_ManPower WHERE RoleType='SMT-IT'");
                        else if (theDialog.FileName.Contains("Maintenance"))
                            qry = string.Format(@"DELETE FROM SMT_ManPower WHERE RoleType='Maintenance'");
                        else if (theDialog.FileName.Contains("סטאפ"))
                            qry = string.Format(@"DELETE FROM SMT_ManPower WHERE RoleType='Setup'");
                        else if (theDialog.FileName.Contains("Process"))
                            qry = string.Format(@"DELETE FROM SMT_ManPower WHERE RoleType='Process'");
                        else if (theDialog.FileName.Contains("Ruslan"))
                            qry = string.Format(@"DELETE FROM SMT_ManPower WHERE RoleType='Ruslan-Team'");
                        else
                            qry = string.Format(@"DELETE FROM SMT_ManPower WHERE RoleType in ('Lines', N'מעטפת')");

                        sql.Update(qry);

                        foreach (var row in list)
                        {
                            row.BackUpCapacity = list.Where(x => x.BackUpRole == row.MainRole || x.BackUpRole2 == row.MainRole).Count();
                            row.NextCapacity = list.Where(x => x.NextRole == row.MainRole).Count();

                            qry = string.Format(@"INSERT INTO SMT_ManPower (Name, WN, Shift, MainRole, RoleType, BackUpRole, BackUpRole2, NextRole, StartDate, TotalYears, RoleCapacity, BackUpCapacity, NextCapacity, LastUpdate)
                                                   VALUES(N'{0}','{1}',N'{2}',N'{3}',N'{4}',N'{5}',N'{6}',N'{7}','{8}','{9}','{10}','{11}','{12}','{13}')",
                                                     row.Name, row.WN, row.Shift, row.MainRole, row.RoleType, row.BackUpRole, row.BackUpRole2, row.NextRole, row.StartDate, row.TotalYears, row.RoleCapacity, row.BackUpCapacity, row.NextCapacity, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            sql.InsertNonQuery(qry);
                        }

                        RefreshTable();
                        MessageBox.Show("Update Successfully!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}

