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
    public partial class AutoModeAreaSelect : Form
    {
        private int _XAreaTotalCnt, _YAreaTotalCnt;
        private int _PauseXAreaIdx, _PauseYAreaIdx;
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
        public int PauseXAreaIdx
        {
            set { _PauseXAreaIdx = value; }
            get { return _PauseXAreaIdx; }
        }
        public int PauseYAreaIdx
        {
            set { _PauseYAreaIdx = value; }
            get { return _PauseYAreaIdx; }
        }
        public AutoModeAreaSelect()
        {
            InitializeComponent();
        }

        private void ReStartAreaBtn_Click(object sender, EventArgs e)
        {
            Form1 MainForm1 = (Form1)this.Owner;
            MainForm1.AutoReXArea = Convert.ToInt32(AutoReXAreaComboBox.Text)-1;//陣列-1
            MainForm1.AutoReYArea = Convert.ToInt32(AutoReYAreaComboBox.Text)-1;
            MainForm1.PauseMode =1;
            this.Close();
        }

        private void ReStartCancelAreaBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AutoReXAreaComboBox_DropDown(object sender, EventArgs e)
        {
            AutoReXAreaComboBox.Items.Clear();
            for (int i = 0; i < _XAreaTotalCnt; i++)
            {
                AutoReXAreaComboBox.Items.Add((i+1).ToString());
            }
            
        }

        private void AutoReYAreaComboBox_DropDown(object sender, EventArgs e)
        {
            AutoReYAreaComboBox.Items.Clear();
            for (int j = 0; j < _YAreaTotalCnt; j++)
            {
                AutoReYAreaComboBox.Items.Add((j + 1).ToString());
            }
        }

        private void AutoModeAreaSelect_Load(object sender, EventArgs e)
        {
            AutoReXAreaComboBox.Items.Clear();
            AutoReXAreaComboBox.Items.Add(_PauseXAreaIdx);
            AutoReYAreaComboBox.Items.Clear();
            AutoReXAreaComboBox.Items.Add(_PauseYAreaIdx);
        }
    }
}
