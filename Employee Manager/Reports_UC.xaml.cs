using Employee_Manager.Classes;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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
    public partial class Reports_UC : UserControl
    {
        public ExcelPackage excel;
        public List<string> ExportChooseFiles = new List<string>();

        public Reports_UC()
        {
            InitializeComponent();
            dp.SelectedDate = DateTime.Now;
            dp2.SelectedDate = DateTime.Now;

        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            ExportChooseFiles.Clear();
            if (BreakPerEmpCheck.IsChecked == false && AllBreakCheck.IsChecked == false && LinesScanEmpCheck.IsChecked == false && DelBreaksCheck.IsChecked == false)
            {
                MessageBox.Show("You Must Choose Files", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            if (dp2.SelectedDate.Value < dp.SelectedDate.Value)
            {
                MessageBox.Show("Invalid Date Range", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            else
            {
                if (BreakPerEmpCheck.IsChecked == true)
                    ExportChooseFiles.Add("BreakPerEmp");

                if (AllBreakCheck.IsChecked == true)
                    ExportChooseFiles.Add("AllBreak");

                if (LinesScanEmpCheck.IsChecked == true)
                    ExportChooseFiles.Add("LinesScanEmp");

                if (DelBreaksCheck.IsChecked == true)
                    ExportChooseFiles.Add("DelBreaks");
                Generate();
            }
        }

        private void Generate()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Execl files (*.xlsx)|*.xlsx",
                FilterIndex = 0,
                RestoreDirectory = true
            };

            saveFileDialog.FileName = "Employee Manager";
            saveFileDialog.Title = "Employee Manager";

            bool? result = saveFileDialog.ShowDialog();
            string path = saveFileDialog.FileName;

            if (result == true)
            {
                try
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    excel = new ExcelPackage();

                    for (int k = 0; k < ExportChooseFiles.Count(); k++)
                    {
                        ExcelWorksheet workSheet;
                        if (ExportChooseFiles[k] == "BreakPerEmp")
                            workSheet = BreakPerEmp();

                        if (ExportChooseFiles[k] == "AllBreak")
                            workSheet = AllBreak();

                        if (ExportChooseFiles[k] == "LinesScanEmp")
                            workSheet = LinesScanEmp();

                        if (ExportChooseFiles[k] == "DelBreaks")
                            workSheet = DelBreaks();
                    }
                    if (File.Exists(path))
                        File.Delete(path);

                    FileStream objFileStrm = File.Create(path);
                    objFileStrm.Close();
                    // Write content to excel file  
                    File.WriteAllBytes(path, excel.GetAsByteArray());
                    //Close Excel package 
                    excel.Dispose();
                    System.Diagnostics.Process.Start(path);
                }
                catch (Exception ex)
                {
                    if (ex.ToString().Contains("being used by another process"))
                    {
                        MessageBox.Show("The File Is Open, Plaese Close It And Try Again.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    else
                        MessageBox.Show(ex.ToString(), "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
            }
        }

        public ExcelWorksheet BreakPerEmp()
        {
            SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "HC_Visualisation", "aoi", "$Flex2016");
            string From = dp.SelectedDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
            string To = dp2.SelectedDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
            string PrintFrom = dp.SelectedDate.Value.ToString("dd.MM.yyyy");
            string PrintTo = dp2.SelectedDate.Value.ToString("dd.MM.yyyy");


            string qry = string.Format(@"SELECT A.ID, A.StartDate, A.EndDate, B.FirstName+' '+ B.LastName, B.HR, B.LineName
                                         FROM BreakRegister AS A
                                         LEFT Join PresenceRegister AS B ON A.PresenceID=B.ID
                                         WHERE A.StartDate>='{0}' AND A.StartDate<='{1}'
                                         ORDER BY A.StartDate ASC", From, To);
            List<Break> list = sql.SelectBreaks(qry);
            list.ForEach(x => { x.AllLates = x.LateLessFive + x.LateMoreFive; });

            var workSheet = excel.Workbook.Worksheets.Add("Top Lates - " + PrintFrom + " - " + PrintTo);
            workSheet.TabColor = System.Drawing.Color.Black;
            workSheet.DefaultRowHeight = 12;
            workSheet.Row(1).Height = 20;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Row(1).Style.Font.Size = 15;


            workSheet.Cells[1, 1].Value = "Name";
            workSheet.Cells[1, 2].Value = "WN";
            workSheet.Cells[1, 3].Value = "Department";
            workSheet.Cells[1, 4].Value = "Total Lates < 5";
            workSheet.Cells[1, 5].Value = "Total Lates > 5";
            workSheet.Cells[1, 6].Value = "Total Lates";

            int i = 2;
            foreach (var row in list.Where(x => x.LateLessFive > 0 || x.LateMoreFive > 0).OrderByDescending(y => y.AllLates))
            {
                workSheet.Row(i).Style.Font.Size = 13;
                workSheet.Cells[i, 1].Value = row.Name;
                workSheet.Cells[i, 2].Value = row.WN;
                workSheet.Cells[i, 3].Value = row.Department;
                workSheet.Cells[i, 4].Value = row.LateLessFive;
                workSheet.Cells[i, 5].Value = row.LateMoreFive;
                workSheet.Cells[i, 6].Value = row.AllLates;
                i++;
            }

            workSheet.Column(1).AutoFit();
            workSheet.Column(2).AutoFit();
            workSheet.Column(3).AutoFit();
            workSheet.Column(4).AutoFit();
            workSheet.Column(5).AutoFit();
            workSheet.Column(6).AutoFit();

            return workSheet;
        }

        public ExcelWorksheet AllBreak()
        {
            SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "HC_Visualisation", "aoi", "$Flex2016");
            string From = dp.SelectedDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
            string To = dp2.SelectedDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
            string PrintFrom = dp.SelectedDate.Value.ToString("dd.MM.yyyy");
            string PrintTo = dp2.SelectedDate.Value.ToString("dd.MM.yyyy");


            string qry = string.Format(@"SELECT A.ID, A.StartDate, A.EndDate, B.FirstName+' '+ B.LastName, B.HR, B.LineName
                                         FROM BreakRegister AS A
                                         LEFT Join PresenceRegister AS B ON A.PresenceID=B.ID
                                         WHERE A.StartDate>='{0}' AND A.StartDate<='{1}'
                                         ORDER BY A.StartDate ASC", From, To);
            List<Break> list = sql.SelectBreaks(qry);

            var workSheet = excel.Workbook.Worksheets.Add("All Breaks - " + PrintFrom + " - " + PrintTo);
            workSheet.TabColor = System.Drawing.Color.Black;
            workSheet.DefaultRowHeight = 12;
            workSheet.Row(1).Height = 20;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Row(1).Style.Font.Size = 15;


            workSheet.Cells[1, 1].Value = "Date";
            workSheet.Cells[1, 2].Value = "WN";
            workSheet.Cells[1, 3].Value = "Name";
            workSheet.Cells[1, 4].Value = "Start Break";
            workSheet.Cells[1, 5].Value = "End Break";
            workSheet.Cells[1, 6].Value = "Allowed";
            workSheet.Cells[1, 7].Value = "Duration";
            workSheet.Cells[1, 8].Value = "Time Left";

            int i = 2;
            foreach (var row in list)
            {
                workSheet.Row(i).Style.Font.Size = 13;
                workSheet.Cells[i, 1].Value = row.BreakEnd_Full.ToString("dd/MM/yyyy");
                workSheet.Cells[i, 2].Value = row.WN;
                workSheet.Cells[i, 3].Value = row.Name;
                workSheet.Cells[i, 4].Value = row.BreakStart;
                workSheet.Cells[i, 5].Value = row.BreakEnd;
                workSheet.Cells[i, 6].Value = row.Allowed;
                workSheet.Cells[i, 7].Value = row.Duration;
                workSheet.Cells[i, 8].Value = row.TimeLeft;
                i++;
            }

            workSheet.Column(1).AutoFit();
            workSheet.Column(2).AutoFit();
            workSheet.Column(3).AutoFit();
            workSheet.Column(4).AutoFit();
            workSheet.Column(5).AutoFit();
            workSheet.Column(6).AutoFit();
            workSheet.Column(7).AutoFit();
            workSheet.Column(8).AutoFit();

            return workSheet;
        }

        public ExcelWorksheet LinesScanEmp()
        {
            SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "HC_Visualisation", "aoi", "$Flex2016");
            string From = dp.SelectedDate.Value.ToString("yyyy-MM-dd");
            string To = dp2.SelectedDate.Value.ToString("yyyy-MM-dd");
            string PrintFrom = dp.SelectedDate.Value.ToString("dd.MM.yyyy");
            string PrintTo = dp2.SelectedDate.Value.ToString("dd.MM.yyyy");


            string qry = string.Format(@"SELECT A.Date, A.WN, A.Name,  A.Department, A.SMT_In, A.SMT_Out, B.LineName, CAST(B.StartDate as time), CAST(B.EndDate as time)
                                         FROM [EmployeeSMT].[dbo].[Employee_InOut] A
                                         LEFT JOIN [HC_Visualisation].[dbo].[PresenceRegister] B
                                         ON A.WN = B.HR AND CAST(A.Date as date)  = CAST(B.StartDate as date)
										 WHERE CAST(A.Date as date)>='{0}' AND CAST(A.Date as date)<='{1}' AND A.Deleted is null
                                         ORDER BY A.Date ASC", From, To);
            List<InOut> list = sql.SelectInOut(qry);

            var workSheet = excel.Workbook.Worksheets.Add("Line Scans - " + PrintFrom + " - " + PrintTo);
            workSheet.TabColor = System.Drawing.Color.Black;
            workSheet.DefaultRowHeight = 12;
            workSheet.Row(1).Height = 20;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Row(1).Style.Font.Size = 15;


            workSheet.Cells[1, 1].Value = "Date";
            workSheet.Cells[1, 2].Value = "WN";
            workSheet.Cells[1, 3].Value = "Name";
            workSheet.Cells[1, 4].Value = "In Work";
            workSheet.Cells[1, 5].Value = "Out Work";
            workSheet.Cells[1, 6].Value = "Line";
            workSheet.Cells[1, 7].Value = "In Line";
            workSheet.Cells[1, 8].Value = "Out Line";

            int i = 2;
            foreach (var row in list)
            {
                workSheet.Row(i).Style.Font.Size = 13;
                workSheet.Cells[i, 1].Value = row.Date.ToString("dd/MM/yyyy");
                workSheet.Cells[i, 2].Value = row.WN;
                workSheet.Cells[i, 3].Value = row.Name;
                workSheet.Cells[i, 4].Value = row.In_Work;
                workSheet.Cells[i, 5].Value = row.Out_Work;
                workSheet.Cells[i, 6].Value = row.Line;
                workSheet.Cells[i, 7].Value = row.In_Line;
                workSheet.Cells[i, 8].Value = row.Out_Line;
                i++;
            }

            workSheet.Column(1).AutoFit();
            workSheet.Column(2).AutoFit();
            workSheet.Column(3).AutoFit();
            workSheet.Column(4).AutoFit();
            workSheet.Column(5).AutoFit();
            workSheet.Column(6).AutoFit();
            workSheet.Column(7).AutoFit();
            workSheet.Column(8).AutoFit();

            return workSheet;
        }

        public ExcelWorksheet DelBreaks()
        {
            SQLClass sql = new SQLClass("MIGSQLCLU4\\SMT", "EmployeeSMT", "aoi", "$Flex2016");

            string qry = string.Format(@"SELECT * FROM Break_Deleted ORDER BY StartTime DESC");
            List<Break> list = sql.SelectDelBreaks(qry);

            var workSheet = excel.Workbook.Worksheets.Add("Deleted Breaks");
            workSheet.TabColor = System.Drawing.Color.Black;
            workSheet.DefaultRowHeight = 12;
            workSheet.Row(1).Height = 20;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Row(1).Style.Font.Size = 15;


            workSheet.Cells[1, 1].Value = "WN";
            workSheet.Cells[1, 2].Value = "Name";
            workSheet.Cells[1, 3].Value = "Break Start";
            workSheet.Cells[1, 4].Value = "Break End";
            workSheet.Cells[1, 5].Value = "Duration";
            workSheet.Cells[1, 6].Value = "Allowd";
            workSheet.Cells[1, 7].Value = "Time Over";
            workSheet.Cells[1, 8].Value = "Note";
            workSheet.Cells[1, 9].Value = "Delete By";

            int i = 2;
            foreach (var row in list)
            {
                workSheet.Row(i).Style.Font.Size = 13;
                workSheet.Cells[i, 1].Value = row.WN;
                workSheet.Cells[i, 2].Value = row.Name;
                workSheet.Cells[i, 3].Value = row.BreakStart_Full.ToString("dd/MM/yyyy HH:mm");
                workSheet.Cells[i, 4].Value = row.BreakEnd_Full.ToString("dd/MM/yyyy HH:mm");
                workSheet.Cells[i, 5].Value = row.Duration;
                workSheet.Cells[i, 6].Value = row.Allowed;
                workSheet.Cells[i, 7].Value = row.TimeLeft;
                workSheet.Cells[i, 8].Value = row.Note;
                workSheet.Cells[i, 9].Value = row.DeletedBy;
                i++;
            }

            workSheet.Column(1).AutoFit();
            workSheet.Column(2).AutoFit();
            workSheet.Column(3).AutoFit();
            workSheet.Column(4).AutoFit();
            workSheet.Column(5).AutoFit();
            workSheet.Column(6).AutoFit();
            workSheet.Column(7).AutoFit();
            workSheet.Column(8).AutoFit();
            workSheet.Column(9).AutoFit();

            return workSheet;
        }
    }
}
