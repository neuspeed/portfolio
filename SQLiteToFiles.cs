using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace SQLiteToFiles
{
    public partial class Form1 : Form
    {

        private String dbFileName;
        private SQLiteConnection m_dbConn;
               
        public Form1()
        {
            InitializeComponent();
        }

        private bool CreateFilesFromDB(string file)
        {
            bool ret = true;
            try
            {
                //string dir = Path.
            }
            catch (Exception ex)
            {
                Console.WriteLine("CreateFilesFromDB: " + ex.ToString());
                ret = false;
            }

            return ret;
        }

        private void btnOpenDB_Click(object sender, EventArgs e)
        {
            if (ofdDBOpen.ShowDialog() == DialogResult.OK)
            {
                dbFileName = ofdDBOpen.FileName;

                try
                {
                    m_dbConn = new SQLiteConnection("Data Source=" + dbFileName);
                    m_dbConn.Open();
                  
                    lblDBStatus.Text = "Установлено";
                    lblDBStatus.ForeColor = Color.Green;
                }
                catch (SQLiteException ex)
                {
                    lblDBStatus.Text += "Не установлено";
                    lblDBStatus.ForeColor = Color.Red;
                    MessageBox.Show("SQLite: " + ex.Message);
                }
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblDBStatus.Text = "Не установлено";
            lblDBStatus.ForeColor = Color.Red;
        }

        private void btnDBExport_Click(object sender, EventArgs e)
        {
            if (fbdTileFolder.ShowDialog() == DialogResult.OK)
            {
                if (!Directory.Exists(fbdTileFolder.SelectedPath + "\\tile"))
                {
                    Directory.CreateDirectory(fbdTileFolder.SelectedPath + "\\tile");
                }
                string tileDir = fbdTileFolder.SelectedPath + "\\tile";
                DataTable dTable = new DataTable();
                String sqlQuery;
                if (m_dbConn == null)
                {
                    MessageBox.Show("Установите соединение с базой данных");
                    return;
                }

                try
                {
                    sqlQuery = "SELECT X,Y,Zoom,Tile,Tiles.id FROM Tiles INNER JOIN TilesData on TilesData.id = Tiles.id";
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, m_dbConn);
                    adapter.Fill(dTable);
                    DataView dTableSortedByZoom = new DataView(dTable);
                    dTableSortedByZoom.Sort = "Zoom ASC, X ASC";

                    if (dTableSortedByZoom.Count > 1)
                    {
                        int dirZExist = -1;
                        int dirXExist = -1;
                        pbConvertData.Maximum = dTableSortedByZoom.Count;
                        for (int i = 0; i < dTableSortedByZoom.Count; i++)
                        {
                            int currentZDir = (int)(Int64)dTableSortedByZoom[i][2];
                            int currentXDir = (int)(Int64)dTableSortedByZoom[i][0];
                            int currentY = (int)(Int64)dTableSortedByZoom[i][1];

                            if (currentZDir != dirZExist)
                            {
                                dirZExist = currentZDir;
                                dirXExist = -1;
                                if (!Directory.CreateDirectory(tileDir + "\\" + dirZExist).Exists)
                                {
                                    MessageBox.Show("Папка " + tileDir + "\\" + dirZExist + "не создана");
                                }
                            }
                            if (currentXDir != dirXExist)
                            {
                                dirXExist = currentXDir;
                                if (!Directory.CreateDirectory(tileDir + "\\" + dirZExist + "\\" + dirXExist).Exists)
                                {
                                    MessageBox.Show("Папка " + tileDir + "\\" + dirZExist + "\\" + dirXExist + "не создана");
                                }
                            }

                            Byte[] tileIMG = (Byte[])dTableSortedByZoom[i][3];
                            //if (File.Exists(tileDir + "\\" + dirZExist + "\\" + currentXDir + "\\" + currentY + ".PNG"))
                            //{
                            //    MessageBox.Show("Файл уже существует :" + tileDir + "\\" + dirZExist + "\\" + currentXDir + "\\" + currentY + ".PNG");
                            //}
                            FileStream fs = File.Create(tileDir + "\\" + dirZExist + "\\" + dirXExist + "\\" + currentY + ".PNG");
                            if (fs != null)
                            {
                                fs.Write(tileIMG, 0, tileIMG.Length);
                                fs.Close();
                                pbConvertData.Value = i;
                                lblConvertData.Text = ((int)(((double)pbConvertData.Value / (double)dTableSortedByZoom.Count) * 100)).ToString() + " %";
                            }
                            else
                            {
                                MessageBox.Show("Ошибка создания файла");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("База данных пуста");
                    }
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show("SQLite: " + ex.Message);
                }
            }
           
        }
    }
}
