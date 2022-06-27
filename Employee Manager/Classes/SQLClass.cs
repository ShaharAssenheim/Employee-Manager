using Employee_Manager.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Employee_Manager
{
    class SQLClass
    {
        private SqlConnection conn;
        private readonly string connectionString = "";

        public SQLClass(string ServerName, string DataBaseName, string UserName, string Secret)
        {
            connectionString =
           "Data Source=" + ServerName + ";" +
           "Initial Catalog=" + DataBaseName + ";" +
           "User id=" + UserName + ";" +
           "Password=" + Secret + ";";
        }

        private bool OpenConnection()
        {
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                return true;
            }
            catch (SqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Cannot connect to server.  Contact administrator");
                        break;
                    case 53:
                        MessageBox.Show("Cannot connect to server. check your internet connection");
                        break;

                    case 1045:
                        MessageBox.Show("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        private bool CloseConnection()
        {
            try
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public int InsertScalar(string query)
        {
            int n = -1;
            if (this.OpenConnection() == true)
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    n = (int)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    this.CloseConnection();
                    MessageBox.Show(ex.Message);
                }
                this.CloseConnection();
            }
            return n;
        }

        public void Update(string query)
        {
            if (this.OpenConnection() == true)
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    this.CloseConnection();
                    MessageBox.Show(ex.Message);
                }
                this.CloseConnection();
            }
        }

        public void InsertNonQuery(string query)
        {
            if (this.OpenConnection() == true)
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    this.CloseConnection();
                    MessageBox.Show(ex.Message);
                }
                this.CloseConnection();
            }
        }

        internal DataTable SelectDb(string query)
        {
            var cmdResult = new DataTable();

            if (this.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, conn);

                if (cmd != null)
                {
                    try
                    {
                        var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        cmdResult.Load(reader);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        if (conn.State == ConnectionState.Open) conn.Close();
                    }
                }
            }
            return cmdResult;
        }

        public List<Worker> SelectWorkers(string query)
        {
            List<Worker> list = new List<Worker>();

            if (this.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Worker r = new Worker();
                            r.WN = reader.IsDBNull(0) ? "" : reader.GetString(0).Trim();
                            r.Name = reader.IsDBNull(1) ? "" : reader.GetString(1).Trim();
                            r.Shift = reader.IsDBNull(2) ? "" : reader.GetString(2).Trim();
                            r.Password = reader.IsDBNull(3) ? "" : reader.GetString(3).Trim();
                            r.Level = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                            r.StartDate = reader.IsDBNull(5) ? "" : reader.GetDateTime(5).ToString("dd-MM-yyyy HH:mm:ss").Substring(0, 10);
                            r.TotalYears = reader.IsDBNull(6) ? "" : reader.GetString(6);
                            list.Add(r);
                        }
                        reader.Close();
                    }
                }
                catch (SqlException ex)
                {
                    this.CloseConnection();
                    MessageBox.Show(ex.Message);
                }
                this.CloseConnection();
            }
            return list;
        }

        public List<InOut> SelectInOut(string query)
        {
            List<InOut> list = new List<InOut>();

            if (this.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InOut r = new InOut();
                            r.Date = reader.GetDateTime(0);
                            r.WN = reader.IsDBNull(1) ? "" : reader.GetString(1).Trim();
                            r.Name = reader.IsDBNull(2) ? "" : reader.GetString(2).Trim();
                            r.Department = reader.IsDBNull(3) ? "" : reader.GetString(3).Trim();
                            r.Department = r.Department.Contains("ייצור משלים") ? "הלחמת גל" : r.Department;
                            r.In_Work = reader.IsDBNull(4) ? "" : reader.GetString(4).Trim();
                            r.Out_Work = reader.IsDBNull(5) ? "" : reader.GetString(5).Trim();
                            r.Line = reader.IsDBNull(6) ? "" : reader.GetString(6).Trim();
                            r.In_Line = reader.IsDBNull(7) ? "" : reader.GetValue(7).ToString().Substring(0, 5);
                            r.Out_Line = reader.IsDBNull(8) ? "" : reader.GetValue(8).ToString().Substring(0, 5);

                            if (r.Line == "")
                                r.RowColor = "Salmon";

                            r.Out_Work = r.Out_Work == "00:0" ? "" : r.Out_Work;
                            if (MainWindow.ExceptionalList.Exists(x => x.WN == r.WN)) continue;
                            list.Add(r);
                        }
                        reader.Close();
                    }
                }
                catch (SqlException ex)
                {
                    this.CloseConnection();
                    MessageBox.Show(ex.Message);
                }
                this.CloseConnection();
            }
            return list;
        }

        public List<Break> SelectBreaks(string query)
        {
            List<Break> list = new List<Break>();

            if (this.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Break r = new Break();
                            r.Break_ID = reader.GetInt32(0);
                            r.BreakStart_Full = reader.GetDateTime(1);
                            r.BreakEnd_Full = reader.IsDBNull(2) ? new DateTime() : reader.GetDateTime(2);
                            r.Name = reader.IsDBNull(3) ? "" : reader.GetString(3).Trim();
                            r.WN = reader.IsDBNull(4) ? "" : reader.GetString(4).Trim();
                            r.Department = reader.IsDBNull(5) ? "" : reader.GetString(5).Trim();
                            r.BreakStart = r.BreakStart_Full.ToString("HH:mm");
                            r.BreakEnd = r.BreakEnd_Full.Year == 1 ? "" : r.BreakEnd_Full.ToString("HH:mm");
                            if (MainWindow.ExceptionalList.Exists(x => x.WN == r.WN)) continue;

                            TimeSpan ts;
                            if (r.BreakEnd_Full.Year == 1)
                                ts = DateTime.Now - r.BreakStart_Full;
                            else
                                ts = r.BreakEnd_Full - r.BreakStart_Full;
                            r.Duration = Math.Round(ts.TotalMinutes);

                            if (!r.Department.ToLower().Contains("wave"))//Not Wave Soldering
                            {
                                r.Allowed = r.BreakStart_Full.Hour >= 7 && r.BreakStart_Full.Hour < 11 ? 15 :
                                r.BreakStart_Full.Hour >= 11 && r.BreakStart_Full.Hour < 15 ? 35 :
                                r.BreakStart_Full.Hour >= 15 && r.BreakStart_Full.Hour < 16 ? 10 :
                                r.BreakStart_Full.Hour >= 16 && r.BreakStart_Full.Hour < 19 ? 15 :
                                r.BreakStart_Full.Hour >= 19 && r.BreakStart_Full.Hour < 23 ? 20 :
                                r.BreakStart_Full.Hour >= 23 || r.BreakStart_Full.Hour < 3 ? 40 :
                                r.BreakStart_Full.Hour >= 3 && r.BreakStart_Full.Hour < 4 ? 10 :
                                r.BreakStart_Full.Hour >= 4 && r.BreakStart_Full.Hour < 7 ? 20 : 0;
                            }
                            else//Wave Soldering
                            {
                                r.Allowed = r.BreakStart_Full.Hour >= 7 && r.BreakStart_Full.Hour < 11 ? 15 :
                                r.BreakStart_Full.Hour >= 11 && r.BreakStart_Full.Hour < 15 ? 30 :
                                r.BreakStart_Full.Hour >= 15 && r.BreakStart_Full.Hour < 16 ? 10 :
                                r.BreakStart_Full.Hour >= 16 && r.BreakStart_Full.Hour < 19 ? 15 :
                                r.BreakStart_Full.Hour >= 19 || r.BreakStart_Full.Hour < 7 ? 30 : 0;
                            }


                            r.TimeLeft = r.Allowed - r.Duration;
                            if (r.TimeLeft < 0 && r.TimeLeft >= -5)
                            {
                                r.RowColor = "LightGoldenrodYellow";
                                if (list.Any(x => x.WN == r.WN && (x.LateLessFive > 0 || x.LateMoreFive > 0)))
                                {
                                    if (list.Any(x => x.WN == r.WN && x.LateLessFive > 0))
                                        list.FirstOrDefault(x => x.WN == r.WN && x.LateLessFive > 0).LateLessFive++;
                                    else
                                        list.FirstOrDefault(x => x.WN == r.WN && x.LateMoreFive > 0).LateLessFive++;
                                }
                                else
                                    r.LateLessFive = 1;
                            }
                            else if (r.TimeLeft < -5)
                            {
                                r.RowColor = "Salmon";
                                if (list.Any(x => x.WN == r.WN && (x.LateLessFive > 0 || x.LateMoreFive > 0)))
                                {
                                    if (list.Any(x => x.WN == r.WN && x.LateMoreFive > 0))
                                        list.FirstOrDefault(x => x.WN == r.WN && x.LateMoreFive > 0).LateMoreFive++;
                                    else
                                        list.FirstOrDefault(x => x.WN == r.WN && x.LateLessFive > 0).LateMoreFive++;
                                }
                                else
                                    r.LateLessFive = 1;
                            }

                            list.Add(r);
                        }
                        reader.Close();
                    }
                }
                catch (SqlException ex)
                {
                    this.CloseConnection();
                    MessageBox.Show(ex.Message);
                }
                this.CloseConnection();
            }
            return list;
        }

        public List<Break> SelectDelBreaks(string query)
        {
            List<Break> list = new List<Break>();

            if (this.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Break r = new Break();
                            r.Break_ID = reader.GetInt32(0);
                            r.WN = reader.IsDBNull(1) ? "" : reader.GetString(1).Trim();
                            r.Name = reader.IsDBNull(2) ? "" : reader.GetString(2).Trim();
                            r.BreakStart_Full = reader.GetDateTime(3);
                            r.BreakEnd_Full = reader.GetDateTime(4);
                            r.Note = reader.IsDBNull(5) ? "" : reader.GetString(5).Trim();
                            r.DeletedBy = reader.IsDBNull(6) ? "" : reader.GetString(6).Trim();

                            TimeSpan ts;
                            if (r.BreakEnd_Full.Year == 1)
                                ts = DateTime.Now - r.BreakStart_Full;
                            else
                                ts = r.BreakEnd_Full - r.BreakStart_Full;
                            r.Duration = Math.Round(ts.TotalMinutes);

                            r.Allowed = r.BreakStart_Full.Hour >= 7 && r.BreakStart_Full.Hour < 11 ? 15 :
                                        r.BreakStart_Full.Hour >= 11 && r.BreakStart_Full.Hour < 15 ? 35 :
                                        r.BreakStart_Full.Hour >= 15 && r.BreakStart_Full.Hour < 16 ? 10 :
                                        r.BreakStart_Full.Hour >= 16 && r.BreakStart_Full.Hour < 19 ? 15 :
                                        r.BreakStart_Full.Hour >= 19 && r.BreakStart_Full.Hour < 23 ? 20 :
                                        r.BreakStart_Full.Hour >= 23 || r.BreakStart_Full.Hour < 3 ? 40 :
                                        r.BreakStart_Full.Hour >= 3 && r.BreakStart_Full.Hour < 4 ? 10 :
                                        r.BreakStart_Full.Hour >= 4 && r.BreakStart_Full.Hour < 7 ? 20 : 0;

                            r.TimeLeft = r.Allowed - r.Duration;

                            list.Add(r);
                        }
                        reader.Close();
                    }
                }
                catch (SqlException ex)
                {
                    this.CloseConnection();
                    MessageBox.Show(ex.Message);
                }
                this.CloseConnection();
            }
            return list;
        }

        public List<WaveShift> WaveShift(string query)
        {
            List<WaveShift> list = new List<WaveShift>();

            if (this.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            WaveShift r = new WaveShift();
                            r.Date = reader.GetDateTime(0);
                            r.WN = reader.IsDBNull(1) ? "" : reader.GetString(1).Trim();
                            r.Name = reader.IsDBNull(2) ? "" : reader.GetString(2).Trim();
                            r.Shift_Duration = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                            r.Break_Duration = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                            list.Add(r);
                        }
                        reader.Close();
                    }
                }
                catch (SqlException ex)
                {
                    this.CloseConnection();
                    MessageBox.Show(ex.Message);
                }
                this.CloseConnection();
            }
            return list;
        }

    }
}
