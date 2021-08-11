using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MM_Mistu_PLC_Csharp
{
    public partial class AutoAreaSpeedSet : Form
    {
        private int _XAreaTotalCnt, _YAreaTotalCnt;
        public int XAreaTotalCnt
        {
            set { _XAreaTotalCnt = value; }
            get { return _XAreaTotalCnt; }
        }
        public int YAreaTotalCnt
        {
            set { _YAreaTotalCnt = value; }
            get { return _YAreaTotalCnt; }
        }
        private string[][] _MemoAutoSpeedArray;
        public string [][] MemoAutoSpeedArray
        {
            set { _MemoAutoSpeedArray = value; }
            get { return _MemoAutoSpeedArray; }
        }

        
        public string[][] _AutoSpeedArray;
        public string[][] _AutoMMArray;
        public AutoAreaSpeedSet()
        {
            InitializeComponent();
        }

        
        private void button1_Click(object sender, EventArgs e)
        {
            //Form1 MainForm1 = (Form1)this.Owner;
            //MainForm1.AutoReXArea = Convert.ToInt32(AutoReXAreaComboBox.Text) - 1;//陣列-1
            //MainForm1.AutoReYArea = Convert.ToInt32(AutoReYAreaComboBox.Text) - 1;
            //this.Close();


        }
        private void AutoAreaSpeedSet_Load(object sender, EventArgs e)
        {


            string[][] AutoSpeedArray = new string[_XAreaTotalCnt * _YAreaTotalCnt][];
            for (int i = 0; i < _XAreaTotalCnt * _YAreaTotalCnt; i++)
            {
                AutoSpeedArray[i] = new string[3];
            }


            int x = 0;
            for (int j = 0; j < _YAreaTotalCnt; j++)
            {
                for (int i = 0; i < _XAreaTotalCnt; i++)
                {
                    AutoSpeedArray[x][0] = "(" + (i+1) + "," + (j+1) + ")";
                    
                    x++;
                   
                }
            }

            //將二維數組的內容添加到泛型數組
            List<string[]> str = new List<string[]>(); //聲明泛型數組

            for (int k = 0; k < x; k++)
                str.Add(AutoSpeedArray[k]);
            //查詢

            var Query = from s in str
                        select new
                        {
                            區域 = s[0]
                        };

            this.bindingSource1.DataSource = Query;
            this.dataGridView1.DataSource = this.bindingSource1;

            //額外手動新增,這樣才能編輯 grid
            this.dataGridView1.Columns.Add("col","X軸速度");
            this.dataGridView1.Columns.Add("col", "Y軸速度");
            this.dataGridView1.Columns.Add("col", "Z軸速度");
            
            DataGridViewButtonColumn col3 = new DataGridViewButtonColumn();
            col3.UseColumnTextForButtonValue = true;
            col3.Text = "開啟 EZM 檔";
            this.dataGridView1.Columns.Add(col3);
            this.dataGridView1.Columns.Add("col", "EZM 檔路徑");
            this.dataGridView1.Columns[5].Width = 300;
            // DEFAULT 值
            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            {
                
                this.dataGridView1[1, i].Value =  3000;//X軸速度 default
                this.dataGridView1[2, i].Value = 3000;//Y軸速度  default
                this.dataGridView1[3, i].Value = 3000;//Z軸速度  default
            }
            // xy 區域編號比實際陣列+1避免user混淆
            x = 0;

            // 如果有舊資料時, show 舊資料
            if (_MemoAutoSpeedArray!=null && _MemoAutoSpeedArray.Length > 0)
            {
                for (int i = 0; i<this.dataGridView1.Rows.Count; i++)
                {
                    this.dataGridView1[0, i].Value = _MemoAutoSpeedArray[i][0];
                    this.dataGridView1[1, i].Value = _MemoAutoSpeedArray[i][1];
                    this.dataGridView1[2, i].Value = _MemoAutoSpeedArray[i][2];
                    this.dataGridView1[3, i].Value = _MemoAutoSpeedArray[i][3];
                    if (_MemoAutoSpeedArray[i].Length >3)
                            this.dataGridView1[5, i].Value = _MemoAutoSpeedArray[i][4];
                }
            }
           
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.ColumnIndex == 4)
            {
                 OpenFileDialog OpenFileDialog = new OpenFileDialog();
                OpenFileDialog.InitialDirectory = ".\\";
                OpenFileDialog.Filter = "EZM檔 (*.ezm)|*.ezm";
                OpenFileDialog.FilterIndex = 2;
                OpenFileDialog.RestoreDirectory = true;
                
                if (OpenFileDialog.ShowDialog() == DialogResult.OK)
                    this.dataGridView1[5, e.RowIndex].Value = OpenFileDialog.FileName;
                
            }
        }

        private void AutoAreaSpeedSaveBtn_Click(object sender, EventArgs e)
        {
            //把 grid 內容存入速度陣列
            _AutoSpeedArray = new string[this.dataGridView1.Rows.Count][];
            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            {
                _AutoSpeedArray[i] = new string[5];
                _AutoSpeedArray[i][0] = this.dataGridView1[0, i].Value.ToString();//區域編號
                _AutoSpeedArray[i][1] = this.dataGridView1[1, i].Value.ToString();//X軸速度
                _AutoSpeedArray[i][2] = this.dataGridView1[2, i].Value.ToString();//Y軸速度 
                _AutoSpeedArray[i][3] = this.dataGridView1[3, i].Value.ToString();//Z軸速度 
                _AutoSpeedArray[i][4] = this.dataGridView1[5, i].Value.ToString();//mm file path
            }
            //MessageBox.Show("各區xy軸速度/MM file x路徑已儲存");
            this.Close();
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AutoAreaSpeedSet_FormClosed(object sender, FormClosedEventArgs e)
        {
            //for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            //{
            //    if (this.dataGridView1[4, i].Value == null)
            //    {
            //        MessageBox.Show("EZM檔未選擇!!");
            //        return;
            //    }
            //}
        }

       
    }
}
