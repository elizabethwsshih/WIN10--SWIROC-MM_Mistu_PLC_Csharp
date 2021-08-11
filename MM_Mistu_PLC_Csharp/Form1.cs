using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace MM_Mistu_PLC_Csharp
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32")]
        private static extern uint GetPrivateProfileString(string section, string key, string defaultvalue, byte[] buf, uint size, string FilePath);


        // new CLASS
        PLCAction PLCAction = new PLCAction();
        DrawItems DrawItems = new DrawItems();
        FileIO FileIO = new FileIO();
        VerifyPW VP = new VerifyPW();

        private OpenFileDialog OpenFileDialog;
        public int ManualMode = -1;//手動模式全域變數
        double XCurPos, YCurPos, ZCurPos;
        int XAreaCnt, YAreaCnt;

        int IniAreaCnt; //新版的區域總數
        List<double> IniXPointList; //新版的所有區域座標點集合
        List<double> IniYPointList; //新版的所有區域座標點集合
        List<double> IniZPointList; //新版的所有區域座標點集合
        List<double> IniSpeedRateList; //新版的所有區域示意圖集合
        List<string> IniMMFileList; //新版的所有區域mm file集合
        List<string> IniAreaNumList = new List<string>(); //新版的所有區示意圖域移動順序集合(temp str)
        List<int> IniAreaNumXList = new List<int>(); //新版的所有區域示意圖X移動順序集合
        List<int> IniAreaNumYList = new List<int>(); //新版的所有區域示意圖Y移動順序集合
        List<string> IniPicFileList; //新版的所有區域示意圖檔案路徑集合

        //短解寫法-----------------------------------------------------------
        List<int> IniXdirList = new List<int>();//新版決定X軸向所乘倍率(1or-1)
        List<int> IniYdirList = new List<int>();//新版決定Y軸向所乘倍率(1or-1)
        List<int> IniZdirList = new List<int>();//新版決定Z軸向所乘倍率(1or-1)
        List<int> IniXoriList = new List<int>();//新版決定X軸原點
        List<int> IniYoriList = new List<int>();//新版決定Y軸原點
        List<int> IniZoriList = new List<int>();//新版決定Z軸原點
        List<int> IniXUpBoundList = new List<int>();//新版決定X軸上限
        List<int> IniXDownBoundList = new List<int>();//新版決定X軸下限
        List<int> IniYUpBoundList = new List<int>();//新版決定Y軸上限
        List<int> IniYDownBoundList = new List<int>();//新版決定Y軸下限
        List<int> IniZUpBoundList = new List<int>();//新版決定Z軸上限
        List<int> IniZDownBoundList = new List<int>();//新版決定Z軸下限
        //短解寫法-----------------------------------------------------------

        double w, h, OffsetW, OffsetH;//新版的動畫區域長寬分配數量單位


        string ProdMgrFilePath;//新版生管單路徑名稱
        ProdMgrFile MgrFile = new ProdMgrFile();//新版生管單物件class
        string ProgramStatus = null; // 新版當前程式執行項目 
        string ProdAlmMsg = null; // 新版寫入生管單之 alm 字串 
        string UserAccount = null; //新版工號欄位
        DateTime ProgStart, ProgEnd;//新版生管單開啟程式時間與關閉程式時間
        DateTime MaunalRedLightStart;//新版記錄手動紅光打開時間
        DateTime dt1;//新版所有生管單紀錄動作起始時間
        string RecipeName; //新版製程檔名 


        int MaunalAreaIdx = -1;//新版maual模式現在專注區塊的所在編號


        int XCatchStatus = 0, XMoveStatus = -1, XManualCatchStatus = -1, XGoHomeStatus = -1, XGoHomeMoveStatus = -1;
        int YCatchStatus = 0, YMoveStatus = -1, YManualCatchStatus = -1, YGoHomeStatus = -1, YGoHomeMoveStatus = -1;
        int ZCatchStatus = 0, ZMoveStatus = -1, ZManualCatchStatus = -1, ZGoHomeStatus = -1, ZGoHomeMoveStatus = -1;
        double x_width, y_height;
        double IniXPt, IniYPt, IniZPt, IniZStage1Pt, IniZStage2Pt;


        string[][] AutoSpeedArray, AutoMMArray;//所有區域的xy軸速度陣列
        string[][] SetAutoSpeedArray, SetAutoMMArray;//設定區域的xy軸速度陣列

        //string[][] SetAutoSpeedArray, SetAutoMMArray;//設定所有區域的xy軸速度陣列
        string MMFilePath;
        int AutoMarkMode; // 新版雷射=1,2/紅光=0
        int ActionMode = -1;// 新版自動 = 0, 補焊=1
        int LaserReset; //雷射重置狀態
        int AutoLaserStage = 0; //自動雷刻模式, 1:stage1, 2:stage2
        int ManualLaserStage = 0; //手動雷刻模式, 1:stage1, 2:stage2


        //關閉FLAG狀態
        int MarkMateFlag = 1, AutoMarkThreadFlag = 0, AutoMarkStage2ThreadFlag = 0, ManualMarkThreadFlag = 0, GoHomeThreadFlag = 0, Timer4Flag = 0, Timer5Flag = 0;
        int M1209 = -1;

        //雷射MARKING中保護
        int CloseDoor = -1; //判但門是否開啟,0為關閉,1為開啟
        // for 判斷是否雷射出光中, -1:移動中或紅光狀態可直接 abort;0:準備出光,須等待出光完(=1)才可以交給IntrTimerabort; 
        //                          1: 出光完成,交給IntrTimer abort 2:紅光
        int FinishLaserMarking = -1;
        int StopLaserMarking = 0;//暫停紐發訊號給IntrTimer表示需要暫停了,IntrTimer會等到 FinishLaserMarking = 1 才abort
        int ForDoorProcessing = 0;  //for 判斷是否急停使用,1:進行銲接中,0:一般狀態, 當Processing==1 && CloseDoor ==1 才能急停

        //初始重置 Btn 狀態
        int ManLaserReqBtnFlag = 0;

        //新版 Thread 是否執行中,for錯誤訊息判是不是一開機plc移動指令就卡住
        int ThreadAlive = -1;

        System.Threading.Timer timer2; //回傳活著訊號
        System.Threading.Timer timer3; //相關get plc 訊號
        System.Threading.Timer timer4; //手動雷射源專用 timer
        System.Threading.Timer timer5; //手動調整位置保護機制專用 timer
        System.Threading.Timer IntrTimer; //暫停專用 timer

        Bitmap DemoBitmap, _Bitmap;
        Thread AutoMarkThread, ManualMarkThread, AutoMarkStage2Thread, GoHomeThread, ManualLaserStartThread;
        Thread DeleteFileThread; //新版:砍過期檔案
        int GoHomePause = 0; // 歸home被暫停 =1,無暫停=0
        int AutoModeStatus;//紀錄自動模式: 0完全停止(下次會從頭開始)1暫停(下次從指定位置開始)2啟動
        int MarkMoveMode = -1; //1為自動,0為手動
        int CurXAreaIdx = 0, CurYAreaIdx = 0;//記錄自動模式走到哪一區域

        //20210113 蕭兄測試
        int LaserReqTestFlag = -1;
        int RedLightReqTestFlag = -1;


        //功能加密----------------------------
        private string PWStr;
        public string _PWStr
        {
            set { PWStr = value; }
        }

        //代參數
        private string _args;
        public Form1()
        {
            InitializeComponent();
        }

        public Form1(string value)
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(value))
            {
                _args = value;  //1為ipc,2為雷捷
            }
        }

        //----------子視窗傳值給父視窗----------
        public int AutoReXArea = 0, AutoReYArea = 0, PauseMode = 0;//紀錄所有暫停相關參數

        #region
        // ================ 測試連接 PLC 模組,成功則回傳一地址================
        private void TestOpenBtn_Click(object sender, EventArgs e)
        {

            int iReturnCode;				//Return code
            if (LogicStationNumtextBox.Text.Length > 0)
            {
                iReturnCode = PLCAction.PLC_Connect(Convert.ToInt32(LogicStationNumtextBox.Text));

                ReturnCodetextBox.Text = String.Format("0x{0:x8} [HEX]", iReturnCode);
            }
            else MessageBox.Show("LogicStationNumber is null!");
        }

        // ================ 測試連接 PLC 模組,成功則回傳一地址================


        //============== 讀取 PLC 暫存器值/==============
        private void TestReadBtn_Click(object sender, EventArgs e)
        {
            int iReturnCode;				//Return code
            String szDeviceName = "";		//List data for 'DeviceName'
            int iNumberOfData = 0;			//Data for 'iNumberOfData'
            short[] arrDeviceValue;		    //Data for 'DeviceValue'
            int iNumber;					//Loop counter
            System.String[] arrData;	    //Array for 'Data'
            int ReturnINT32Val;
            double ReturnVal;


            if (DataSizetextBox.Text.Length > 0 && DeviceNametextBox.Lines.Length > 0)
            {
                //----------------1. 抓取 device 值-------------------------
                szDeviceName = String.Join("\n", DeviceNametextBox.Lines);


                iNumberOfData = Convert.ToInt32(DataSizetextBox.Text);
                try
                {
                    arrDeviceValue = new short[iNumberOfData];
                    iReturnCode = axActUtlType1.ReadDeviceBlock2(szDeviceName,
                                                                 iNumberOfData,
                                                                 out arrDeviceValue[0]);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, Name,
                                     MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;

                }
                //The return code of the method is displayed by the hexadecimal.
                ReturnCodetextBox.Text = String.Format("0x{0:x8}", iReturnCode);
                // Console.WriteLine("iReturnCode=" + iReturnCode);



                //When the ReadDeviceBlock2 method is succeeded, display the read data.
                if (iReturnCode == 0)
                {
                    //Assign array for the read data.
                    arrData = new System.String[iNumberOfData];

                    //Copy the read data to the 'arrData'.
                    for (iNumber = 0; iNumber < iNumberOfData; iNumber++)
                    {
                        arrData[iNumber] = arrDeviceValue[iNumber].ToString();
                    }

                    //Set the read data to the 'Data', and display it.
                    ReturnDatatextBox.Lines = arrData;
                }
                //-------------------2. 將讀取到的 short 值轉換至 int32-------------------------
                // short(2Bytes,16bit)=> 2個 short 組合成 4bytes=> 1個 int32(4 Bytes,32 bits)
                for (iNumber = 0; iNumber < iNumberOfData - 1; iNumber++)
                {
                    ReturnINT32Val = PLCAction.Short2Int32(arrDeviceValue[iNumber], arrDeviceValue[iNumber + 1]);
                    ReturnVal = Convert.ToDouble(ReturnINT32Val) / Convert.ToDouble(10000);
                    ConvertDatatextBox.Text = Convert.ToString(ReturnVal);
                }
            }
            else MessageBox.Show(this, "DataSize or Device name is null!");

        }


        private void TestWriteBtn_Click(object sender, EventArgs e)
        {
            int iReturnCode;				//Return code
            String szDeviceName = "";		//List data for 'DeviceName'
            int iNumberOfData = 0;			//Data for 'DeviceSize'
            short[] arrDeviceValue;		    //Data for 'DeviceValue'
            int iNumber;					//Loop counter
            int iSizeOfIntArray;		    //
            short[] ReturnVal16 = new short[2];
            //-------------------1. 將讀取到的 INT32 值轉換至 short-------------------------
            ReturnVal16 = PLCAction.Int32Short(Convert.ToDouble(TestWriteDataTxtBox.Text), 10000);


            //Get size for 'DeviceValue'
            iSizeOfIntArray = TestWriteDataTxtBox.Lines.Length * 2;
            //Assign the array for 'DeviceValue'.
            iNumberOfData = Convert.ToInt32(DataSizetextBox.Text);
            arrDeviceValue = new short[iNumberOfData];
            szDeviceName = DeviceNametextBox.Text;
            //Convert the 'DeviceValue'.
            for (iNumber = 0; iNumber < iSizeOfIntArray; iNumber++)
            {
                try
                {
                    arrDeviceValue[iNumber]
                        = ReturnVal16[iNumber];// Convert.ToInt16(TestWriteDataTxtBox.Lines[iNumber]);
                }

                //Exception processing
                catch (Exception exExcepion)
                {
                    MessageBox.Show(exExcepion.Message,
                                      Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            try
            {
                //The WriteDeviceRandom2 method is executed.
                iReturnCode = axActUtlType1.WriteDeviceBlock2(szDeviceName,
                                                                iNumberOfData,
                                                                ref arrDeviceValue[0]);
            }

           //Exception processing			
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, Name,
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //The return code of the method is displayed by the hexadecimal.
            ReturnCodetextBox.Text = String.Format("0x{0:x8} [HEX]", iReturnCode);


        }
        #endregion
        //============== 讀取 PLC 暫存器值/==============
        //=============form 初始化與顯示狀態=============
        private void Form1_Load(object sender, EventArgs e)
        {

            if (_args == "1")//IPC
            {
                this.Text = "(IPC)" + this.Text;
            }
            else if (_args == "2")//SWIROC
            {
                this.Text = "(SWIROC)" + this.Text;


            }




            //201903 MARK
            // this.Hide();
            ProgramStatus = "開啟自動化銲接人機";

            //------------------------
            //if (FileIO.CycleCnt() > 3500)
            //{
            //    //MessageBox.Show(this, "程式執行次數已超過上限，請聯絡雷射中心工研院工程師！！", "警告");
            //    this.Close();
            //}

            OpenFileDialog = new OpenFileDialog();
            //initializing the MMLIBXXXX.ocx
            int MMIniResult, MMStandbyResult;
            MMIniResult = axMMMark1.Initial();
            ////standby the system for marking
            MMStandbyResult = axMMMark1.MarkStandBy();
            //可圈選要打的局部區域
            axMMMark1.SetCurEditFun(2);


            if (MMIniResult == 0 && MMStandbyResult == 0)
                MarkMateFlag = 1;

            PLCAction.PLC_Connect(0);
            //20180920 蕭兄建議開啟後通通初始化
            PLCAction.PLC_AllInitial();


            //ManualMarkBtn.Enabled = false;
            ManualStepDistComboBox.Enabled = false;
            XSelectAreaComboBox.Enabled = true;
            YSelectAreaComboBox.Enabled = true;

            this.tabPage2.Parent = null;
            this.tabPage6.Parent = null;
            this.tabPage7.Parent = null;
            this.tabPage3.Parent = null;
            this.tabPage4.Parent = null;
            this.tabPage8.Parent = this.tabControl1;
            //this.tabPage1.Parent = null;

            TimerCallback callback = new TimerCallback(_do);
            timer2 = new System.Threading.Timer(callback, null, 0, 500);//500豪秒起來一次

            // 初始重置訊 
            //20210226 SWIROC 要等太久會導致被初始化先拿掉
            //LaserReset = 1;
            //TimerCallback callback2 = new TimerCallback(PLC_HandShakingSet);
            //timer3 = new System.Threading.Timer(callback2, null, 0, 500);

            // 啟動聆聽暫停訊號
            TimerCallback callback6 = new TimerCallback(_IntrLaserMarking);
            IntrTimer = new System.Threading.Timer(callback6, null, 0, 50);

            //指定一次就好校正檔避免吃到default造成放大/縮小
            string LensFileStr = FileIO.GetLensFile();
            axMMLensCal1.ChangeLens(LensFileStr);

            //xyz軸向倍率(短解寫法)
            List<string> SecList2 = new List<string>();
            SecList2.Add("AXIS_DIR"); //短解先HARDCODE,因為SETTING.INI格式有問題(段落誤判)
            //SecList2 = FileIO.IniDoubleReadSec(@"D://MM_PLC_Setting//Setting.ini");
            IniXdirList = FileIO.IniIntReadKey(SecList2, "X_DIR", @"D://MM_PLC_Setting//Setting.ini");
            IniYdirList = FileIO.IniIntReadKey(SecList2, "Y_DIR", @"D://MM_PLC_Setting//Setting.ini");
            IniZdirList = FileIO.IniIntReadKey(SecList2, "Z_DIR", @"D://MM_PLC_Setting//Setting.ini");

            //原點判讀(短解寫法)
            List<string> SecList3 = new List<string>();
            SecList3.Add("ORIGINAL"); //短解先HARDCODE,因為SETTING.INI格式有問題(段落誤判)
            //SecList2 = FileIO.IniDoubleReadSec(@"D://MM_PLC_Setting//Setting.ini");
            IniXoriList = FileIO.IniIntReadKey(SecList3, "X_ORI", @"D://MM_PLC_Setting//Setting.ini");
            IniYoriList = FileIO.IniIntReadKey(SecList3, "Y_ORI", @"D://MM_PLC_Setting//Setting.ini");
            IniZoriList = FileIO.IniIntReadKey(SecList3, "Z_ORI", @"D://MM_PLC_Setting//Setting.ini");


            //原點判讀(短解寫法)
            List<string> SecList4 = new List<string>();
            SecList4.Add("BOUND"); //短解先HARDCODE,因為SETTING.INI格式有問題(段落誤判)
            //SecList2 = FileIO.IniDoubleReadSec(@"D://MM_PLC_Setting//Setting.ini");
            IniXUpBoundList = FileIO.IniIntReadKey(SecList4, "X_UPBOUND", @"D://MM_PLC_Setting//Setting.ini");
            IniXDownBoundList = FileIO.IniIntReadKey(SecList4, "X_DOWNBOUND", @"D://MM_PLC_Setting//Setting.ini");
            IniYUpBoundList = FileIO.IniIntReadKey(SecList4, "Y_UPBOUND", @"D://MM_PLC_Setting//Setting.ini");
            IniYDownBoundList = FileIO.IniIntReadKey(SecList4, "Y_DOWNBOUND", @"D://MM_PLC_Setting//Setting.ini");
            IniZUpBoundList = FileIO.IniIntReadKey(SecList4, "Z_UPBOUND", @"D://MM_PLC_Setting//Setting.ini");
            IniZDownBoundList = FileIO.IniIntReadKey(SecList4, "Z_DOWNBOUND", @"D://MM_PLC_Setting//Setting.ini");


            //初始重置 Btn 狀態
            ManLaserReqBtnFlag = 0;

            //----------------Laser 只開一次---------------------
            ////2020226 不管是IPC OR SWIROC 只開 M1205一次
            PLCAction.axActUtlType1.SetDevice("M1205", 1);
            FileIO.LogMotion("雷射準備M1205=1");
            int LaserTrigStatus = 0;
            while (true)
            {
                PLCAction.axActUtlType1.GetDevice("M1205", out LaserTrigStatus);
                if (LaserTrigStatus == 1)
                    break;
            }
            System.Threading.Thread thh;
            thh = new Thread(new ThreadStart(delegate()
            {
                FrmLaserWait f = new FrmLaserWait();
                f.ShowDialog();
            }));
            thh.Start();
            this.Enabled = false;

            if (_args == "2") //SWIROC 要等比較久
                Thread.Sleep(3000);
            else if (_args == "1")//ipc 不用等這麼久
                Thread.Sleep(1000);

            thh.Abort();



            this.Enabled = true;

            //----------------Laser Start---------------------


            this.Hide();
        }
        private void _do(object state)
        {

            this.BeginInvoke(new setlabel2(setlabel3));//委派

        }
        private void _IntrLaserMarking(object state)
        {

          //  this.BeginInvoke(new _IntrLaser(IntrLaser));//委派

        }
        delegate void _IntrLaser();
        private void IntrLaser()
        {
            if (CloseDoor == 1 && ForDoorProcessing == 1)//機台工作中打開才需要急停
            {
                AutoMarkStopBtn_Click(null, null);
                ForDoorProcessing = 0;
            }

            if (StopLaserMarking == 1 && FinishLaserMarking == 1)
            //StopLaserMarking =1 喊暫停,但要等出光完成(FinishLaserMarking == 1)
            //FinishLaserMarking 代表MM 出光已完成
            {
                AutoMarkStage2Thread.Abort();
                AutoMarkStage2Thread.Join();
                StopLaserMarking = 0;
                FinishLaserMarking = -1;
                //一進來就清空示意圖框
                Bitmap bmp = new Bitmap(PicShowPicBox.Width, PicShowPicBox.Height);
                PicShowPicBox.Image = bmp;
                bmp = DrawEmpthRect(bmp);
            }
        }

        private void PLC_HandShakingSet(object state)
        {
            //-===================================================
            //** 雷射重置機制,回傳值給PLC表示我還活著
            //** M1200 重置, M1209 傳給 PLC 訊號
            //** M1200 在程式起來時只需要做一次, by LaserReset flag 判段只須做一次
            //** PLC 將 M1209 = 0, PC端將 M1209 =1, 不斷循環

            if (LaserReset == 0)
            {
                PLCAction.axActUtlType1.SetDevice("M1200", 0);
                // FileIO.LogMotion("交握M1200=0");
            }
            else if (LaserReset == 1)
            {
                PLCAction.axActUtlType1.SetDevice("M1200", 1);
                // FileIO.LogMotion("交握M1200=1");
                LaserReset = 0;
            }
            if (M1209 == 0)//axActUtlType1.GetDevice("M1209", out LaserProtect) == 0)
            {
                PLCAction.axActUtlType1.SetDevice("M1209", 1);
                //  FileIO.LogMotion("交握重置M1209=1");
            }

        }

        delegate void setlabel2();
        private void setlabel3()
        {

            //每秒更新一次
            CurTimeLbl.Text = DateTime.Now.ToString("HH:mm:ss");


            // ReadPLCDataRandom 一次呼叫完畢,節省時間

            ////-----------------
            double[] _ReadVal = PLCAction.ReadPLCDataRandom("D1000\nD1001\nD1010\nD1011\nR1000\nR1001\nR1010\nR1011\nD1006\nD1007" //1-2-3-4-5
                                                         + "\nD1016\nD1017\nR1002\nR1003\nR1012\nR1013\nM1107\nM1117\nM1102\nM1105" //6-7-8--9-10-11-12
                                                         + "\nM1112\nM1115\nM1209\nM1600\nM1601\nM1602\nM1603\nM1604\nM1605\nM1606" //13-14-15-16-17-18-19-20-21-22
                                                         + "\nM1607\nM1608\nM1609\nM1610\nM1611\nM1602\nM1002\nM1012\nD1020\nD1021"// 23-24-25-26-27-28-29-30--31
                                                         + "\nR1020\nR1021\nD1026\nD1027\nR1022\nR1023\nM1122\nM1125"//32-33-34--35-36
                                                         + "\nM1613\nM1614\nM1615\nM1107\nM1117\nM1127\nM1616", 55);//37-38-39-40-41-42-43

            //// hardcode 上面順序比較節省尋找時間
            XCurPos = Math.Round(_ReadVal[0], 3); //D1000
            YCurPos = Math.Round(_ReadVal[1], 3); //D1010
            ZCurPos = Math.Round(_ReadVal[30], 3); //D1020

            //顯示需乘上倍率
            XCurPosTxtBox.Text = (XCurPos * Convert.ToDouble(IniXdirList[0])).ToString();
            YCurPosTxtBox.Text = (YCurPos * Convert.ToDouble(IniYdirList[0])).ToString();
            ZCurPosTxtBox.Text = (ZCurPos * Convert.ToDouble(IniZdirList[0])).ToString();

            AutoXTgtTxtBox.Text = Math.Round(_ReadVal[2], 3).ToString();  //R1000
            AutoYTgtTxtBox.Text = Math.Round(_ReadVal[3], 3).ToString();  //R1010
            AutoZTgtTxtBox.Text = Math.Round(_ReadVal[31], 3).ToString();  //R1020


            AutoXSpeedTxtBox.Text = (Math.Round(_ReadVal[4], 3) * 100).ToString(); //D1006
            AutoYSpeedTxtBox.Text = (Math.Round(_ReadVal[5], 3) * 100).ToString(); //D1016
            AutoZSpeedTxtBox.Text = (Math.Round(_ReadVal[32], 3) * 100).ToString(); //D1026


            SetXSpeedTxtBox.Text = (Math.Round(_ReadVal[6], 3) * 100).ToString(); //R1002
            SetYSpeedTxtBox.Text = (Math.Round(_ReadVal[7], 3) * 100).ToString(); //R1012
            SetZSpeedTxtBox.Text = (Math.Round(_ReadVal[33], 3) * 100).ToString(); //R1022

            //201903 沒有門
            //門是否異常被打開
            //CloseDoor = Convert.ToInt32(_ReadVal[42]); //M1616
            CloseDoor = 0;


            XGoHomeStatus = Convert.ToInt32(_ReadVal[39]); //M1107
            YGoHomeStatus = Convert.ToInt32(_ReadVal[40]); //M1117
            ZGoHomeStatus = Convert.ToInt32(_ReadVal[41]); //M1127


            XCatchStatus = Convert.ToInt32(_ReadVal[10]); //M1102
            XMoveStatus = Convert.ToInt32(_ReadVal[11]);  //M1105
            YCatchStatus = Convert.ToInt32(_ReadVal[12]); //M1112
            YMoveStatus = Convert.ToInt32(_ReadVal[13]);  //M1115
            ZCatchStatus = Convert.ToInt32(_ReadVal[34]); //M1122
            ZMoveStatus = Convert.ToInt32(_ReadVal[35]);  //M1125


            if (XMoveStatus == 1) XMoveStatusTxtBox.Text = "移動中";
            else XMoveStatusTxtBox.Text = "靜止";
            if (YMoveStatus == 1) YMoveStatusTxtBox.Text = "移動中";
            else YMoveStatusTxtBox.Text = "靜止";
            if (ZMoveStatus == 1) ZMoveStatusTxtBox.Text = "移動中";
            else ZMoveStatusTxtBox.Text = "靜止";

            //系統交握訊號讀回

            // M1209 = Convert.ToInt32(_ReadVal[14]); //M1209
            //讀回系統M1209交握值
            M1209 = PLCAction.PLC_HandShakingRead();

            //判斷自動流程狀態
            //0完全停止(下次會從頭開始)1暫停(下次從指定位置開始)2啟動
            if (AutoModeStatus == 0)
            {
                AutoModeStatusLbl.Text = "停止";
                AutoModeStatusLbl2.Text = "停止";
            }
            else if (AutoModeStatus == 1)
            {
                AutoModeStatusLbl.Text = "暫停";
                AutoModeStatusLbl2.Text = "暫停";
            }
            else if (AutoModeStatus == 2)
            {
                AutoModeStatusLbl.Text = "自動模式執行中";
                AutoModeStatusLbl2.Text = "自動模式執行中";
            }

            XSpeedLbl.Text = Convert.ToString(AutoReXArea);
            YSpeedLbl.Text = Convert.ToString(AutoReYArea);

            AlmMsgTxtBox.Clear();
            string[] almstr = new string[20];
            if (_ReadVal[15] == 1)
                //  AlmMsgTxtBox.Text += "X軸下極限(M1600),";
                almstr[0] = "X軸下極限(M1600)_";
            if (_ReadVal[16] == 1)
                // AlmMsgTxtBox.Text += "X軸上極限(M1601),";
                almstr[1] = "X軸上極限(M1601)_";
            if (_ReadVal[17] == 1)
                // AlmMsgTxtBox.Text += "Y軸下極限(M1602),";
                almstr[2] = "Y軸下極限(M1602)_";
            if (_ReadVal[18] == 1)
                // AlmMsgTxtBox.Text += "Y軸上極限(M1603),";
                almstr[3] = "Y軸上極限(M1603)_";
            if (_ReadVal[19] == 1)
                //AlmMsgTxtBox.Text += "Z軸下極限(M1604),";
                almstr[4] = "Z軸下極限(M1604)_";
            if (_ReadVal[20] == 1)
                //AlmMsgTxtBox.Text += "Z軸上極限(M1605),";
                almstr[5] = "Z軸上極限(M1605)_";
            if (_ReadVal[21] == 1)
                //AlmMsgTxtBox.Text += "平台馬達異常(M1606),";
                almstr[6] = "平台馬達異常(M1606)_";
            if (_ReadVal[22] == 1)
                //AlmMsgTxtBox.Text += "監控系統急停(M1607),";
                almstr[7] = "監控系統急停(M1607)_";
            if (_ReadVal[23] == 1)
                //AlmMsgTxtBox.Text += "雷射源急停(M1608),";
                almstr[8] = "雷射源急停(M1608)_";
            if (_ReadVal[24] == 1)
                //AlmMsgTxtBox.Text += "X軸需復歸(M1610),";
                almstr[9] = "X軸需復歸(M1610)_";
            if (_ReadVal[25] == 1)
                //AlmMsgTxtBox.Text += "Y軸需復歸(M1611),";
                almstr[10] = "Y軸需復歸(M1611)_";
            if ((ThreadAlive == 0 || ThreadAlive == -1) && XCatchStatus == 1)
                // AlmMsgTxtBox.Text += "X軸一速移動卡住(M1002),";
                almstr[11] = "X軸一速移動卡住(M1002)_";
            if ((ThreadAlive == 0 || ThreadAlive == -1) && YCatchStatus == 1)
                // AlmMsgTxtBox.Text += "Y軸一速移動卡住(M1012),";
                almstr[12] = "Y軸一速移動卡住(M1012)_";
            if (_ReadVal[26] == 1)
                //AlmMsgTxtBox.Text += "Z軸需復歸(M1612),";
                almstr[13] = "Z軸需復歸(M1612)_";
            if (_ReadVal[36] == 1)
                //AlmMsgTxtBox.Text += "X軸ERROR,";
                almstr[14] = "X軸ERROR_";
            if (_ReadVal[37] == 1)
                // AlmMsgTxtBox.Text += "Y軸ERROR,";
                almstr[15] = "Y軸ERROR_";
            if (_ReadVal[38] == 1)
                //AlmMsgTxtBox.Text += "Z軸ERROR,";
                almstr[16] = "Z軸ERROR_";
            if (GoHomePause == 1)
                // AlmMsgTxtBox.Text += "歸Home動作已暫停,必須繼續完成歸Home動作才能繼續其他動作";
                almstr[17] = "歸Home動作已暫停,必須繼續完成歸Home動作才能繼續其他動作_";

            if (XGoHomeStatus == 0 || YGoHomeStatus == 0 ||
               (XGoHomeStatus == 0 && YGoHomeStatus == 0 && ZCurPos <= 0.5 && ZCurPos >= -0.5)) //Z軸 PLC M1127無反應,只好以數字限制
            {
                //AlmMsgTxtBox.Text += "需要歸Home才能繼續其他動作_";
                almstr[18] = "需要歸Home才能繼續其他動作_";
            }
            ProdAlmMsg = "";
            // AlmMsgTxtBox.Text = "";
            for (int i = 0; i < almstr.Length; i++)
                AlmMsgTxtBox.Text += almstr[i];



            //收集所有 alm 訊息
            ProdAlmMsg = ProdAlmMsg + AlmMsgTxtBox.Text;
        }
        private void DeleteFile()
        {
            string DelFilePath;
            //1.生管單
            DelFilePath = @"D:\\MDataTemp";
            FileIO.DeleteFile(DelFilePath, "*.csv", 14);//天數

            //2.平台 log
            DelFilePath = @"D:\\MM_PLC_Setting\\LogFiles";
            FileIO.DeleteFile(DelFilePath, "*.txt", 14);

            //3.按鈕log
            DelFilePath = @"D:\\MM_PLC_Setting\\LogFiles\\MotionLog";
            FileIO.DeleteFile(DelFilePath, "*.txt", 14);

        }
        //=============form 初始化與顯示狀態===============
        private void 開啟舊檔ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VerifyPW VerifyPW = new VerifyPW();
            VerifyPW.Owner = this;
            VerifyPW.ShowDialog();

            int Result;
            Result = AccountVerify("PE", PWStr);
            if (PWStr == null || PWStr == "CLOSE") return;
            else if (Result > 0)
            {
                MessageBox.Show(this, "輸入密碼錯誤!");
                return;
            }

            int tmp = 0;
            String OpenFilePath = "";

            OpenFileDialog.InitialDirectory = ".\\";
            OpenFileDialog.Filter = "pmoptz files (*.pmoptz)|*.h|All files (*.*)|*.*";
            OpenFileDialog.FilterIndex = 2;
            OpenFileDialog.RestoreDirectory = true;
            DialogResult result = OpenFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                OpenFilePath = OpenFileDialog.FileName;
                MMfileNameLbl.Text = OpenFilePath.Substring(OpenFilePath.LastIndexOf("\\") + 1, (OpenFilePath.LastIndexOf(".") - OpenFilePath.LastIndexOf("\\")) - 1);

                if (string.IsNullOrEmpty(OpenFilePath))
                {
                    MessageBox.Show(this, "Error= No file !!!");
                }
                else
                {
                    tmp = axMMMark1.LoadFile(OpenFilePath);
                    // IsLoadEZM = 1;

                }
                double POWER = axMMMark1.GetPower("root"); //對全圖的所有物件作設定
                SetMMPowerTxtBox.Text = Convert.ToString(POWER);

                double speed = axMMMark1.GetSpeed("root");
                SetMMSpeedTxtBox.Text = Convert.ToString(speed);

                double Freq = axMMMark1.GetFrequency("root");
                SetMMFreqTxtBox.Text = Convert.ToString(Freq);

                double MarkDelay = axMMMark1.GetMarkDelay("root");
                MarkDelay = MarkDelay / 1000;
                SetMMMarkDelayTxtBox.Text = Convert.ToString(MarkDelay);


                double LaserOnDelay = axMMMark1.GetLaserOnDelay("root");
                LaserOnDelay = LaserOnDelay / 1000;
                SetMMLaserOnDelayTxtBox.Text = Convert.ToString(LaserOnDelay);

                double PolyDelay = axMMMark1.GetPolyDelay("root");
                PolyDelay = PolyDelay / 1000;
                SetMMPolyDelayTxtBox.Text = Convert.ToString(PolyDelay);

                double LaserOffDelay = axMMMark1.GetLaserOffDelay("root");
                LaserOffDelay = LaserOffDelay / 1000;
                SetMMLaserOffDelayTxtBox.Text = Convert.ToString(LaserOffDelay);

                double JumpSpeed = axMMMark1.GetJumpSpeed("root");
                SetMMJumpSpeedTxtBox.Text = Convert.ToString(JumpSpeed);

                double JumpDelay = axMMMark1.GetJumpDelay("root");
                JumpDelay = JumpDelay / 1000;
                SetMMJumpDelayTxtBox.Text = Convert.ToString(JumpDelay);





            }
        }

        private void eXITToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //shutdown the system
            axMMMark1.MarkShutdown();

            //un-initializing the MMLIBXXXX.ocx
            axMMMark1.Finish();
            this.Close();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            PLCAction.axActUtlType1.SetDevice("M1206", 0);
            FileIO.LogMotion("關閉紅光M1206=0");
            //20210226不管是 swiroc 或 ipc 都改成 M1205 只開一次
            PLCAction.axActUtlType1.SetDevice("M1205", 0);
            FileIO.LogMotion("雷射關閉M1205=0");


            if (MarkMateFlag == 1)
            {
                //shutdown the system
                axMMMark1.Finish();
                axMMMark1.MarkShutdown();
                //un-initializing the MMLIBXXXX.ocx
                // axMMMark1.Finish();
            }


            if (AutoMarkThreadFlag == 1) // 有產生過 thread 才要關閉
                AutoMarkThread.Abort();
            if (AutoMarkStage2ThreadFlag == 1) // 有產生過 thread 才要關閉
                AutoMarkStage2Thread.Abort();
            if (GoHomeThreadFlag == 1)
                GoHomeThread.Abort();

            //timer1.Dispose();
            //timer2.Dispose();
            //timer3.Dispose();



            PLCAction.axActUtlType1.Close();

            //----------------產生結束生管單-----------------
            //生管單基本資料
            //有登入成功才要寫生管單
            if (VP.VP_result == 1)
            {
                ProgramStatus = "關閉自動化銲接人機";
                ProgEnd = DateTime.Now;
                TimeSpan ts = ProgEnd - ProgStart;
                double TimeDiff = Convert.ToDouble(ts.TotalSeconds.ToString());
                string StrTimeDiff = TimeDiff.ToString("#0.00");

                PordMgrFileEnd(ProgramStatus, ProgEnd.ToString(),
                                StrTimeDiff, ProdMgrFilePath, ProdAlmMsg);

            }

        }

        private void MMExcuteBtn_Click(object sender, EventArgs e)
        {
            axMMMark1.StartMarking(3);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            axMMMark1.StopMarking();
        }



        private void label29_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ActionMode = 0;
            this.tabPage2.Parent = null;
            this.tabPage6.Parent = null;
            this.tabPage7.Parent = null;
            this.tabPage3.Parent = null;
            //this.tabPage5.Parent = null;
            this.tabPage4.Parent = null;
            // this.tabPage1.Parent = this.tabControl1;
            if (Timer4Flag == 1)
                timer4.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {


        }

        private void button3_Click(object sender, EventArgs e)
        {
            FileIO.LogAction(button3.Text);
            VerifyPW VerifyPW = new VerifyPW();
            VerifyPW.Owner = this;
            VerifyPW.ShowDialog();
            MessageBox.Show(this, PWStr);
            int Result = AccountVerify("Admin", PWStr);
            if (Result > 0) return;


            this.tabPage2.Parent = null;
            //this.tabPage1.Parent = null;
            this.tabPage6.Parent = null;
            this.tabPage8.Parent = null;
            this.tabPage4.Parent = null;
            this.tabPage7.Parent = null;
            this.tabPage3.Parent = this.tabControl1;
            ManualMode = 1;
            if (Timer4Flag == 1)
                timer4.Dispose();


        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            FileIO.LogAction(button4.Text);
            this.BeginInvoke((MethodInvoker)delegate { this.Close(); });
            // this.Close();

        }


        private void AutoModeBtn_Click(object sender, EventArgs e)
        {
            FileIO.LogAction(AutoModeBtn.Text);
            string FilePath;
            SaveFileDialog SaveDlg = new SaveFileDialog();
            SaveDlg.Title = "儲存參數檔";
            SaveDlg.Filter = "文字檔 (*.txt)|*.txt";

            if (SaveDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK && SaveDlg.FileName != "")
            {
                FilePath = SaveDlg.FileName;
                FileStream fs = new FileStream(FilePath, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write("XAreaCnt=" + (XAreaCntTxtBox.Text) + "\r\n");
                sw.Write("YAreaCnt=" + (YAreaCntTxtBox.Text) + "\r\n");

                sw.Write("XAreaWidth=" + (XAreaWidthTxtBox.Text) + "\r\n");
                sw.Write("YAreaHeight=" + (YAreaHeightTxtBox.Text) + "\r\n");

                sw.Write("XIniPoint=" + (IniXStartTxtBox.Text) + "\r\n");
                sw.Write("YIniPoint=" + (IniYStartTxtBox.Text) + "\r\n");
                sw.Write("ZIniPoint=" + (IniZStartTxtBox.Text) + "\r\n");
                sw.Write("ZStage2IniPoint=" + (IniZStartStagw2TxtBox.Text) + "\r\n");

                sw.Write("SpeedRate=" + (SpeedRateLbl.Text) + "\r\n");

                sw.Write("[MMFile]" + (AutoSetMMFileTxtBox.Text) + "\r\n");
                //解析各區xy軸速度設定
                for (int i = 0; i < SetAutoSpeedArray.Length; i++)
                {
                    if (SetAutoSpeedArray[i][0].Length > 0 && SetAutoSpeedArray[i][1].Length > 0 && SetAutoSpeedArray[i][2].Length > 0)
                        sw.Write("AreaSpeed " + SetAutoSpeedArray[i][0] + SetAutoSpeedArray[i][1] + "," + SetAutoSpeedArray[i][2] + "," + SetAutoSpeedArray[i][3] + "\r\n");
                    if (SetAutoSpeedArray[i][0].Length == 0) MessageBox.Show(this, "未指定各區MM檔案路徑!!");
                    else sw.Write("AreaMM " + SetAutoSpeedArray[i][0] + SetAutoSpeedArray[i][4] + "\r\n");

                }
                sw.Flush();
                sw.Close();
                fs.Close();
            }

        }


        private void ManualNoStopRadioBox_Click(object sender, EventArgs e)
        {
            ManualMode = 1;

            DisableOtherManualBtn2();
            ManualNoStopRadioBtn.Checked = true;
            ManualZNegBtn.Enabled = true;
            ManualYPosBtn.Enabled = true;
            ManualXNegBtn.Enabled = true;
            ManualXPosBtn.Enabled = true;
            ManualYNegBtn.Enabled = true;
            ManualZPosBtn.Enabled = true;

            //---
            ManualMarkBtn.Enabled = true;
            ManualMarkBtn.BackColor = Color.LightSkyBlue;
            ManualRedLightBtn.Enabled = true;
            ManualRedLightBtn.BackColor = Color.Brown;

        }



        private void XSelectAreaComboBox_Click(object sender, EventArgs e)
        {

        }

        private void ManualStepRadioBox_Click(object sender, EventArgs e)
        {


            FileIO.LogAction(ManualStepRadioBox.Text);

            ManualMode = 2;
            axActUtlType1.SetDevice("M1201", 1);
            FileIO.LogMotion("手動模式M1201=1");

            ClearPicShow();

            DisableOtherManualBtn2();
            ManualStepRadioBox.Checked = true;
            ManualZNegBtn.Enabled = true;
            ManualYPosBtn.Enabled = true;
            ManualXNegBtn.Enabled = true;
            ManualXPosBtn.Enabled = true;
            ManualYNegBtn.Enabled = true;
            ManualZPosBtn.Enabled = true;
            ManualStepDistComboBox.Enabled = true;
            //---
            ManualMarkBtn.Enabled = true;
            ManualMarkBtn.BackColor = Color.LightSkyBlue;
            ManualRedLightBtn.Enabled = true;
            ManualRedLightBtn.BackColor = Color.Brown;


        }

        private void ManualGoToAreaRadioBox_Click(object sender, EventArgs e)
        {
            FileIO.LogAction(ManualGoToAreaRadioBox.Text);
            XSelectAreaComboBox.Items.Clear();
            YSelectAreaComboBox.Items.Clear();

            //初始化
            DisableOtherManualBtn2();
            ManualGoToAreaRadioBox.Checked = true;
            ManualAreaAsnMoveBtn.Enabled = true;
            ManualAreaAsnMoveBtn.BackColor = Color.SteelBlue;

        }

        private void button8_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 2;
        }



        private void ManualXPosBtn_MouseDown(object sender, MouseEventArgs e)
        {
            //要記得先連通PLC才能執行
            if (ManualMode == 1)
            {
                //axActUtlType1.SetDevice("M1201", 0);
                //axActUtlType1.SetDevice("M1001", 1); 
                ManualSet();
                PLCAction.ManualContinous("XPos");

            }
        }

        private void ManualXPosBtn_MouseUp(object sender, MouseEventArgs e)
        {
            if (ManualMode == 1)
            {
                //axActUtlType1.SetDevice("M1201", 0);
                //axActUtlType1.SetDevice("M1001", 0);
                ManualSet();
                PLCAction.ManualContinousPause();
            }

        }



        private void ManualXNegBtn_MouseDown(object sender, MouseEventArgs e)
        {
            //要記得先連通PLC才能執行
            if (ManualMode == 1)
            {
                //axActUtlType1.SetDevice("M1201", 0);
                //axActUtlType1.SetDevice("M1000", 1);
                //ManualMarkBtn.Enabled = true;
                ManualSet();
                PLCAction.ManualContinous("XNag");
            }

        }

        private void ManualXNegBtn_MouseUp(object sender, MouseEventArgs e)
        {
            if (ManualMode == 1)
            {
                //axActUtlType1.SetDevice("M1201", 0);
                //axActUtlType1.SetDevice("M1000", 0);
                //ManualMarkBtn.Enabled = true;
                ManualSet();
                PLCAction.ManualContinousPause();

            }
        }

        private void ManualYPosBtn_MouseDown(object sender, MouseEventArgs e)
        {
            //要記得先連通PLC才能執行
            if (ManualMode == 1)
            {
                //axActUtlType1.SetDevice("M1201", 0);
                //axActUtlType1.SetDevice("M1010", 1);
                ManualSet();
                PLCAction.ManualContinous("YPos");


            }
        }

        private void ManualYPosBtn_MouseUp(object sender, MouseEventArgs e)
        {

            if (ManualMode == 1)
            {
                //axActUtlType1.SetDevice("M1201",0);
                //axActUtlType1.SetDevice("M1010", 0);

                //ManualMarkBtn.Enabled = true;
                ManualSet();
                PLCAction.ManualContinousPause();
            }
        }

        private void ManualYNegBtn_MouseDown(object sender, MouseEventArgs e)
        {
            //要記得先連通PLC才能執行
            if (ManualMode == 1)
            {
                //axActUtlType1.SetDevice("M1201", 0);
                //axActUtlType1.SetDevice("M1011", 1);
                ManualSet();
                PLCAction.ManualContinous("YNag");

            }
        }

        private void ManualYNegBtn_MouseUp(object sender, MouseEventArgs e)
        {
            if (ManualMode == 1)
            {
                //axActUtlType1.SetDevice("M1201", 0);
                //axActUtlType1.SetDevice("M1011", 0);
                ManualSet();
                PLCAction.ManualContinousPause();

            }
        }

        private void ManualModeBtn_Click(object sender, EventArgs e)
        {
            ManualMsgTxtBox.Clear();
            if (//!ManualNoStopRadioBox.Checked && 
                !ManualStepRadioBox.Checked && !ManualGoToAreaRadioBox.Checked)
            {
                MessageBox.Show(this, "請選擇手動模式!"); return;
            }
            //---------------------1.儲存所有平台移動速度相關參數------------------------------
            //1.吋動距離
            //2.xy軸速度
            //3.速度比率
            //4.xy目標位置(根據 manual mode 計算)

            short[] StepDist = new short[2];
            short[] XSpeed = new short[2];
            short[] YSpeed = new short[2];
            short[] SpeedRate = new short[2];
            short[] XTarget = new short[2];
            short[] YTarget = new short[2];

            double StepDistVal;
            double XSpeedVal;
            double YSpeedVal;
            double SpeedRateVal;

            double XTgtPos1;
            double YTgtPos1;
            double XTgtPos2;
            double YTgtPos2;

            if (ManualStepDistComboBox.Text.Length > 0)
                StepDist = PLCAction.Int32Short(Convert.ToDouble(ManualStepDistComboBox.Text), 10000);
            if (ManualXSpeedTxtBox.Text.Length > 0)
                //XSpeed = PLCAction.Int32Short(Convert.ToDouble(ManualXSpeedTxtBox.Text), 100);
                XSpeed = PLCAction.Int32ShortSpeed(Convert.ToDouble(ManualYSpeedTxtBox.Text));
            else
            {
                MessageBox.Show(this, "請填入X軸速度!"); return;
            }
            if (ManualYSpeedTxtBox.Text.Length > 0)
                YSpeed = PLCAction.Int32ShortSpeed(Convert.ToDouble(ManualYSpeedTxtBox.Text));
            else
            {
                MessageBox.Show(this, "請填入Y軸速度!"); return;
            }
            SpeedRate = PLCAction.Int32Short(Convert.ToDouble(SpeedRateLbl.Text), 10);

            //寫入吋動距離
            try
            { PLCAction.axActUtlType1.WriteDeviceBlock2("R1202", 2, ref StepDist[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            //驗證寫入吋動距離數字
            ManualMsgTxtBox.Text += "寫入步階距離(R1202)=";
            StepDistVal = PLCAction.ReadPLCData("R1202");
            ManualMsgTxtBox.Text += Convert.ToString(StepDistVal) + "\r\n";


            //寫入X軸速度
            try
            { PLCAction.axActUtlType1.WriteDeviceBlock2("R1002", 2, ref XSpeed[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            //驗證寫入X軸速度數字
            ManualMsgTxtBox.Text += "寫入X軸速度(R1002)=";
            XSpeedVal = PLCAction.ReadPLCData("R1002");
            ManualMsgTxtBox.Text += Convert.ToString(XSpeedVal) + "\r\n";


            //寫入Y軸速度
            try
            { PLCAction.axActUtlType1.WriteDeviceBlock2("R1012", 2, ref YSpeed[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            ManualMsgTxtBox.Text += "寫入Y軸速度(R1012)=";
            //驗證寫入Y軸速度數字
            YSpeedVal = PLCAction.ReadPLCData("R1012");
            ManualMsgTxtBox.Text += Convert.ToString(YSpeedVal) + "\r\n";

            //寫入速度比例
            try
            { PLCAction.axActUtlType1.WriteDeviceBlock2("R1200", 2, ref SpeedRate[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            ManualMsgTxtBox.Text += "寫入速度比例(R1200)=";
            //驗證寫入速度比例
            SpeedRateVal = PLCAction.ReadPLCData("R1200");
            ManualMsgTxtBox.Text += Convert.ToString(SpeedRateVal) + "\r\n";

            //---------------------1.儲存所有平台移動速度相關參數 end------------------------------

            ////------------------------------2.手動模式判斷------------------------------------------
            //if (ManualNoStopRadioBox.Checked)
            //{
            //    ManualMode = 1;
            //    ManualMsgTxtBox.Text += "--------手動連續移動模式--------"+"\r\n";
            //    axActUtlType1.SetDevice("M1201", 0);//吋動(連續)模式
            //    ManualMsgTxtBox.Text += "M1201=0"+"\r\n";
            //    ManualXPosBtn.Enabled = true;
            //    ManualYPosBtn.Enabled = true;
            //    ManualXNegBtn.Enabled = true;
            //    ManualYNegBtn.Enabled = true;
            //}
            //else
            if (ManualStepRadioBox.Checked)
            {
                if (ManualStepDistComboBox.Text.Length > 0)
                {
                    ManualMode = 2;
                    //ManualMsgTxtBox.Text += "--------手動步階模式--------" + "\r\n";
                    FileIO.LogMotion("--------手動步階模式--------");
                    axActUtlType1.SetDevice("M1201", 1);//步階模式
                    FileIO.LogMotion("手動模式M1201=1");
                    //ManualMsgTxtBox.Text += "M1201=1" + "\r\n";
                    ManualXPosBtn.Enabled = true;
                    ManualYPosBtn.Enabled = true;
                    ManualXNegBtn.Enabled = true;
                    ManualYNegBtn.Enabled = true;
                }
                else
                {
                    MessageBox.Show(this, "您未輸入步進吋動距離!");
                    return;
                }
            }
            else if (ManualGoToAreaRadioBox.Checked)
            {
                if (XSelectAreaComboBox.Text.ToString().Length > 0 && YSelectAreaComboBox.Text.Length.ToString().Length > 0)
                {
                    ManualMsgTxtBox.Text += "--------手動移動至指定區域模式--------" + "\r\n";
                    ManualMode = 3;

                    //按鈕在此模式不可使用,反白
                    ManualXPosBtn.Enabled = false;
                    ManualYPosBtn.Enabled = false;
                    ManualXNegBtn.Enabled = false;
                    ManualYNegBtn.Enabled = false;
                    ManualMarkBtn.Enabled = false;



                    //---------讀取&儲存座標資料------------
                    ManualMsgTxtBox.Text += "X實際位置(D1000)=";
                    ManualMsgTxtBox.Text += Convert.ToString(XCurPos) + "\r\n";

                    //中心點
                    XTgtPos1 = Convert.ToDouble(XAreaWidthTxtBox.Text) / 2 + (Convert.ToDouble(XSelectAreaComboBox.Text) - 1) * (Convert.ToDouble(XAreaWidthTxtBox.Text));
                    ManualMsgTxtBox.Text += "寫入X目標位置(R1000)=";

                    //寫入X 軸目標位置
                    XTarget = PLCAction.Int32Short(XTgtPos1, 10000);
                    try
                    { axActUtlType1.WriteDeviceBlock2("R1000", 2, ref XTarget[0]); }
                    catch (Exception exception)
                    { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                    XTgtPos2 = PLCAction.ReadPLCData("R1000");
                    ManualMsgTxtBox.Text += Convert.ToString(XTgtPos2) + "\r\n";


                    ManualMsgTxtBox.Text += "Y實際位置(D1010)=";
                    ManualMsgTxtBox.Text += Convert.ToString(YCurPos) + "\r\n";
                    //中心點
                    YTgtPos1 = Convert.ToDouble(YAreaHeightTxtBox.Text) / 2 + (Convert.ToDouble(YSelectAreaComboBox.Text) - 1) * Convert.ToDouble(YAreaHeightTxtBox.Text);
                    ManualMsgTxtBox.Text += "寫入Y目標位置(R1010)=";
                    //寫入Y 軸目標位置
                    YTarget = PLCAction.Int32Short(YTgtPos1, 10000);
                    try
                    { axActUtlType1.WriteDeviceBlock2("R1010", 2, ref YTarget[0]); }
                    catch (Exception exception)
                    { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                    YTgtPos2 = PLCAction.ReadPLCData("R1010");
                    ManualMsgTxtBox.Text += Convert.ToString(YTgtPos2) + "\r\n";
                    //------------------------------


                    //-----------移動至目標位置-----------------
                    axActUtlType1.SetDevice("M1002", 1);//自動模式要用一速移動
                    FileIO.LogMotion("X一速模式M1002=1");
                    //ManualMsgTxtBox.Text += "--\r\n";
                    //ManualMsgTxtBox.Text += "X一速移動開始 M1002=1\r\n";
                    Thread.Sleep(200);

                    while (true)
                    {
                        axActUtlType1.GetDevice("M1102", out XCatchStatus);
                        //ManualMsgTxtBox.Text += "X一速接收命令狀態 M1102=" + Convert.ToString(XCatchStatus) + "\r\n";
                        FileIO.LogMotion("X一速接收命令狀態M1102" + XCatchStatus.ToString());

                        axActUtlType1.GetDevice("M1105", out XMoveStatus);
                        //ManualMsgTxtBox.Text += "X移動狀態 M1105=" + Convert.ToString(XMoveStatus) + "\r\n";
                        FileIO.LogMotion("X移動狀態M1105" + XMoveStatus.ToString());

                        XCurPos = Convert.ToDouble(PLCAction.ReadPLCData("D1000"));
                        //ManualMsgTxtBox.Text += "X實際位置(D1000)=";
                        //ManualMsgTxtBox.Text += Convert.ToString(XCurPos) + ",X 目標位置=" + Convert.ToString(XTgtPos2) + "\r\n";
                        FileIO.LogMotion("X實際位置(D1000)=" + XCurPos.ToString());

                        Thread.Sleep(200);
                        if (XCatchStatus == 1 && XMoveStatus == 0 && XCurPos == XTgtPos2)  //-----------!!!要改!!
                            break;

                    }
                    axActUtlType1.SetDevice("M1002", 0);//自動模式要用一速移動
                    FileIO.LogMotion("X一速模式M1002=0");
                    //ManualMsgTxtBox.Text += "--\r\n";
                    //ManualMsgTxtBox.Text += "X一速移動結束 M1002=0\r\n";
                    //------------------------------------------
                    Thread.Sleep(100);
                    //-------------------------------------------
                    axActUtlType1.SetDevice("M1012", 1);//自動模式要用一速移動
                    //ManualMsgTxtBox.Text += "--\r\n";
                    //ManualMsgTxtBox.Text += "Y一速移動開始 M1012=1\r\n";
                    FileIO.LogMotion("Y一速模式M1012=0");
                    Thread.Sleep(100);
                    while (true)
                    {
                        axActUtlType1.GetDevice("M1112", out YCatchStatus);
                        //ManualMsgTxtBox.Text += "Y 一速接收命令狀態 M1112=" + Convert.ToString(YCatchStatus) + "\r\n";
                        FileIO.LogMotion("Y 一速接收命令狀態 M1112=" + YCatchStatus.ToString());
                        axActUtlType1.GetDevice("M1115", out YMoveStatus);
                        //ManualMsgTxtBox.Text += "Y移動狀態 M1115=" + Convert.ToString(YMoveStatus) + "\r\n";
                        FileIO.LogMotion("Y移動狀態 M1115=" + YMoveStatus.ToString());

                        YCurPos = Convert.ToDouble(PLCAction.ReadPLCData("D1010"));
                        //ManualMsgTxtBox.Text += "Y實際位置(D1010)=";
                        //ManualMsgTxtBox.Text += Convert.ToString(YCurPos) + ",Y 目標位置=" + Convert.ToString(YTgtPos2) + "\r\n";
                        FileIO.LogMotion("Y實際位置(D1010)=" + YCurPos.ToString());
                        Thread.Sleep(100);
                        if (YCatchStatus == 1 && YMoveStatus == 0 && YCurPos == YTgtPos2) //-----------!!!要改!!
                        {
                            ManualMarkBtn.Enabled = true;
                            //ManualMarkBtn.BackColor = Color.Crimson;
                            break;
                        }

                    }
                    axActUtlType1.SetDevice("M1012", 0);//自動模式要用一速移動
                    //ManualMsgTxtBox.Text += "--\r\n";
                    //ManualMsgTxtBox.Text += "X一速移動結束 M1012=0\r\n";
                    FileIO.LogMotion("Y一速M1012=0");

                }
                //------------------------------2.手動模式判斷 end------------------------------------------
                else MessageBox.Show(this, "您未選擇補焊區塊!");

            }

        }

        private void ManualNoStopRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            ManualXPosBtn.Enabled = false;
            ManualYPosBtn.Enabled = false;
            ManualXNegBtn.Enabled = false;
            ManualYNegBtn.Enabled = false;
            //ManualMarkBtn.Enabled = false;
            //ManualMarkBtn.BackColor = Color.Gray;
            ManualMsgTxtBox.Clear();
        }

        private void ManualStepRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            ManualXPosBtn.Enabled = false;
            ManualYPosBtn.Enabled = false;
            ManualXNegBtn.Enabled = false;
            ManualYNegBtn.Enabled = false;
            //ManualMarkBtn.Enabled = false;
            //ManualMarkBtn.BackColor = Color.Gray;
            ManualMsgTxtBox.Clear();
        }

        private void ManualGoToAreaRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            ManualXPosBtn.Enabled = false;
            ManualYPosBtn.Enabled = false;
            ManualXNegBtn.Enabled = false;
            ManualYNegBtn.Enabled = false;
            //ManualMarkBtn.Enabled = false;
            //ManualMarkBtn.BackColor = Color.Gray;
            ManualMsgTxtBox.Clear();
            XSelectAreaComboBox.Enabled = true;
            YSelectAreaComboBox.Enabled = true;
        }

        private void ManualXNegBtn_Click(object sender, EventArgs e)
        {
            FileIO.LogAction("X左");
            ManualSet();// for 吋動需要修改步階距離
            if (ManualMode == 2)
            {
                PLCAction.ManualStep("XNag");



            }
        }
        private void ManualYPosBtn_Click(object sender, EventArgs e)
        {
            FileIO.LogAction("Y前");

            ManualSet();// for 吋動需要修改步階距離
            if (ManualMode == 2)
            {
                PLCAction.ManualStep("YPos");

            }
        }

        private void BoundProtect(object state)//(double X_CurBound, double Y_CurBound)
        {
            Timer5Flag = 1;
            double[] _ReadVal = PLCAction.ReadPLCDataRandom("M1606", 1);
            if (_ReadVal[0] == 1)
            {
                PLCAction.axActUtlType1.SetDevice("M1200", 1);
                FileIO.LogMotion("1.極限保護M1200=1");
                PLCAction.axActUtlType1.SetDevice("M1200", 0);
                FileIO.LogMotion("2.極限保護M1200=0");

            }
            if (XCurPos > 340)
                // for 20210423 雷捷這台先拿掉極限保護
                if (XCurPos > 1000)
                {

                    PLCAction.axActUtlType1.SetDevice("M1202", 1);
                    FileIO.LogMotion("3.極限保護M1202=1");
                    PLCAction.axActUtlType1.SetDevice("M1202", 0);
                    FileIO.LogMotion("4.極限保護M1202=0");
                    PLCAction.axActUtlType1.SetDevice("M1200", 1);
                    FileIO.LogMotion("5.極限保護M1200=1");
                    PLCAction.axActUtlType1.SetDevice("M1200", 0);
                    FileIO.LogMotion("6.極限保護M1200=0");
                    ManualXNegBtn_MouseDown(null, null);
                    ManualXNegBtn_MouseUp(null, null);
                }
            //if (XCurPos < -230)
            // for 20210423 雷捷這台先拿掉極限保護
            if (XCurPos < -1000)
            {

                PLCAction.axActUtlType1.SetDevice("M1202", 1);
                FileIO.LogMotion("7.極限保護M1202=1");
                PLCAction.axActUtlType1.SetDevice("M1202", 0);
                FileIO.LogMotion("8.極限保護M1202=0");
                PLCAction.axActUtlType1.SetDevice("M1200", 1);
                FileIO.LogMotion("9.極限保護M1200=1");
                PLCAction.axActUtlType1.SetDevice("M1200", 0);
                FileIO.LogMotion("10.極限保護M1200=0");
                ManualXPosBtn_MouseDown(null, null);
                ManualXPosBtn_MouseUp(null, null);
            }
            //if (YCurPos > 340)
            if (YCurPos > 1000)
            {

                PLCAction.axActUtlType1.SetDevice("M1202", 1);
                FileIO.LogMotion("11.極限保護M1202=1");
                PLCAction.axActUtlType1.SetDevice("M1202", 0);
                FileIO.LogMotion("12.極限保護M1202=0");
                PLCAction.axActUtlType1.SetDevice("M1200", 1);
                FileIO.LogMotion("13.極限保護M1200=1");
                PLCAction.axActUtlType1.SetDevice("M1200", 0);
                FileIO.LogMotion("14.極限保護M1200=0");
                ManualYNegBtn_MouseDown(null, null);
                ManualYNegBtn_MouseUp(null, null);
            }
            // if (YCurPos < -90)
            if (YCurPos < -1000)
            {

                PLCAction.axActUtlType1.SetDevice("M1202", 1);
                FileIO.LogMotion("14.極限保護M1202=0");
                PLCAction.axActUtlType1.SetDevice("M1202", 0);
                FileIO.LogMotion("15.極限保護M1202=0");
                PLCAction.axActUtlType1.SetDevice("M1200", 1);
                FileIO.LogMotion("16.極限保護M1200=0");
                PLCAction.axActUtlType1.SetDevice("M1200", 0);
                FileIO.LogMotion("17.極限保護M1200=0");
                ManualYPosBtn_MouseDown(null, null);
                ManualYPosBtn_MouseUp(null, null);
            }

            // if (ZCurPos > 349)
            if (ZCurPos > 1000)
            {
                PLCAction.axActUtlType1.SetDevice("M1606", 0);
                FileIO.LogMotion("18.極限保護M1606=0");
                PLCAction.axActUtlType1.SetDevice("M1202", 1);
                FileIO.LogMotion("19.極限保護M1202=1");
                PLCAction.axActUtlType1.SetDevice("M1202", 0);
                FileIO.LogMotion("20.極限保護M1202=0");
                PLCAction.axActUtlType1.SetDevice("M1200", 1);
                FileIO.LogMotion("21.極限保護M1200=1");
                PLCAction.axActUtlType1.SetDevice("M1200", 0);
                FileIO.LogMotion("22.極限保護M1200=0");
                Thread.Sleep(100);
                ManualZPosBtn_MouseDown(null, null);
                Thread.Sleep(100);
                ManualZPosBtn_MouseUp(null, null);
            }
            //if (ZCurPos < -160)
            if (ZCurPos < -1000)
            {

                PLCAction.axActUtlType1.SetDevice("M1202", 1);
                FileIO.LogMotion("23.極限保護M1202=1");
                PLCAction.axActUtlType1.SetDevice("M1202", 0);
                FileIO.LogMotion("24.極限保護M1202=0");
                PLCAction.axActUtlType1.SetDevice("M1200", 1);
                FileIO.LogMotion("25.極限保護M1200=1");
                PLCAction.axActUtlType1.SetDevice("M1200", 0);
                FileIO.LogMotion("26.極限保護M1200=0");
                Thread.Sleep(100);
                ManualZNegBtn_MouseDown(null, null);
                Thread.Sleep(100);
                ManualZNegBtn_MouseUp(null, null);
            }

        }

        private void ManualSet()
        {
            double _StepDistVal = Convert.ToDouble(ManualStepDistComboBox.Text);
            double _XSpeedVal = Convert.ToDouble(ManualXSpeedTxtBox.Text);
            double _YSpeedVal = Convert.ToDouble(ManualYSpeedTxtBox.Text);
            double _ZSpeedVal = Convert.ToDouble(ManualZSpeedTxtBox.Text);
            double _SpeedRateVal = Convert.ToDouble(ManualSpeedRateLbl.Text);

            PLCAction.ManualSet(_StepDistVal, _XSpeedVal, _YSpeedVal, _ZSpeedVal, _SpeedRateVal);// for 吋動需要修改步階距離

            RedLightOff(); // 紅光關閉防呆

        }
        private void ManualXPosBtn_Click(object sender, EventArgs e)
        {
            FileIO.LogAction("X右");
            ManualSet();
            if (ManualMode == 2)
            {
                PLCAction.ManualStep("XPos");

            }
        }
        private void ManualYNegBtn_Click(object sender, EventArgs e)
        {
            FileIO.LogAction("Y後");
            ManualSet();// for 吋動需要修改步階距離
            if (ManualMode == 2)
            {
                PLCAction.ManualStep("YNag");

            }
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            double SpeedRate;
            SpeedRate = Convert.ToDouble(hScrollBar1.Value);//*Convert.ToDouble(hScrollBar1.Width) / Convert.ToDouble(100.0);
            SpeedRateLbl.Text = SpeedRate.ToString();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //D值集中在前面, M值集中在後面,方便解析
            double[] _ReadVal = PLCAction.ReadPLCDataRandom("D1000\nD1001\nD1010\nD1011\nM1102\nM1105\nM1112\nM1115", 8);
        }


        private void label53_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
        }


        private void AutoMarkPauseBtn_Click(object sender, EventArgs e)
        {
            //AutoMsgTxtBox.Text += "---------暫停自動銲接----------\r\n";
            FileIO.LogMotion("---------暫停自動銲接----------");
            AutoMarkThread.Abort();
            //TwinkleAreaThread.Abort();

            AutoModeStatus = 1;
            axActUtlType1.SetDevice("M1002", 0);//自動模式要用一速移動
            FileIO.LogMotion("一速移動STOP M1002=0");
            axActUtlType1.SetDevice("M1012", 0);//自動模式要用一速移動
            FileIO.LogMotion("一速移動STOP M1012=0");

            AutoMarkBtn.Enabled = true;
        }

        private void AutoMarkStopBtn_Click(object sender, EventArgs e)
        {
            ProgramStatus = "StopReset";
            // 20180920 修改
            FileIO.LogAction(AutoMarkStopBtn.Text);
            //AutoMsgTxtBox.Text += "---------停止自動銲接----------\r\n";
            FileIO.LogMotion("---------停止自動銲接----------");
            AlmMsgTxtBox.Text = ProdAlmMsg + "_已暫停並準備回到初始加工點_";
            //停止紅光預覽
            PLCAction.axActUtlType1.SetDevice("M1206", 0);
            FileIO.LogMotion("停止紅光預覽M1206=0");
            //stop laser
            PLCAction.axActUtlType1.SetDevice("M1205", 0);
            FileIO.LogMotion("STOP Laser M1205=0");



            //reset & stop
            PLCAction.axActUtlType1.SetDevice("M1202", 1);
            PLCAction.axActUtlType1.SetDevice("M1200", 1);
            while (true)
            {

                double[] _ReadVal = PLCAction.ReadPLCDataRandom("M1210", 1);
                if (_ReadVal[0] == 1)
                {
                    PLCAction.axActUtlType1.SetDevice("M1202", 0);
                    PLCAction.axActUtlType1.SetDevice("M1200", 0);
                    break;
                }
            }
            // initial PLC 所有指令
            PLCAction.PLC_AllInitial();
            RedLightOff(); // 紅光關閉防呆
            axMMMark1.StopMarking();


            //一進來就清空示意圖框
            Bitmap bmp = new Bitmap(PicShowPicBox.Width, PicShowPicBox.Height);
            PicShowPicBox.Image = bmp;
            bmp = DrawEmpthRect(bmp);


            if (AutoMarkThreadFlag == 1 && AutoMarkThread.IsAlive && AutoMarkThread != null) // 有產生 thread 才要關閉
            {
                if (FinishLaserMarking == -1)//如果只是一般的移動中或是紅光
                {
                    AutoMarkStage2Thread.Abort();
                    AutoMarkStage2Thread.Join();
                }
                else //需要等待雷射出光後才關
                {
                    StopLaserMarking = 1;
                }

            }
            if (AutoMarkStage2ThreadFlag == 1 && AutoMarkStage2Thread != null && AutoMarkStage2Thread.IsAlive)
            {

                if (FinishLaserMarking == -1)//如果只是一般的移動中或是紅光
                {
                    AutoMarkStage2Thread.Abort();
                    AutoMarkStage2Thread.Join();
                }
                else
                {
                    StopLaserMarking = 1;
                }
            }




            if (GoHomeThreadFlag == 1 && GoHomeThread.IsAlive)
            {
                //暫停gohome程序,須等待重新完成 gohome 才能繼續別的步驟
                GoHomePause = 1;
                GoHomeThread.Abort();
                GoHomeThread.Join();

                //AutoGoHomeBtn.Enabled = false;
                //AutoGoHomeBtn.BackColor = Color.Gray;

                AutoMarkBtn.Enabled = false;
                AutoMarkBtn.BackColor = Color.Gray;

                AutoLaserMarkBtn.Enabled = false;
                AutoLaserMarkBtn.BackColor = Color.Gray;

                AutoRedLightStage2Btn.Enabled = false;
                AutoRedLightStage2Btn.BackColor = Color.Gray;

                AutoLaserStage2MarkBtn.Enabled = false;
                AutoLaserStage2MarkBtn.BackColor = Color.Gray;

                AutoPageBtn.Enabled = false;
                AutoPage2Btn.Enabled = false;
                ManualPageBtn.Enabled = false;
            }

            AutoModeStatus = 0;//結束狀態
            //最後退回初始加工點且不做雷射
            //PLCAction.AutoStageMove(IniXPointList[0], IniYPointList[0], IniZPointList[0], IniSpeedRateList[0]);

            //還原其他按鈕
            EnableOtherBtn();

            //暫停生管單
            //生管單基本資料
            String ProgramStatus2 = "暫停" + ProgramStatus;
            PordMgrFileEnd(ProgramStatus2, DateTime.Now.ToString(),
                           "", ProdMgrFilePath, ProdAlmMsg);
            ProdAlmMsg = null;//寫完log清空alm字串
            //---------------------------------------------------

            //20210226 因為 SWIROC和 IPC都只改成開一次雷射,所以在這邊急停的話就先關再偷偷打開囉
            PLCAction.axActUtlType1.SetDevice("M1205", 1);
            FileIO.LogMotion("雷射準備M1205=1");


        }
        private void AutoStageMove(int i, int j)
        {
            double XTgtPos1, XTgtPos2, YTgtPos1 = 0, YTgtPos2;
            short[] XTarget = new short[2];
            short[] YTarget = new short[2];
            short[] XSpeed = new short[2];
            short[] YSpeed = new short[2];

            // Console.WriteLine("****執行(" + i + ',' + j + ")區塊****");
            FileIO.LogMotion("****執行(" + i + ',' + j + ")區塊****");
            AutoAreaNumLbl.Text = "(" + (i + 1).ToString() + "," + (j + 1).ToString() + ")";
            AutoAreaNumLbl2.Text = "(" + (i + 1).ToString() + "," + (j + 1).ToString() + ")";
            //-------讀取 & 儲存座標資料---------
            CurXAreaIdx = i;//記錄當下走到哪格至全域變數
            CurYAreaIdx = j;
            // 影響 performance , mark掉
            //TwinkleAreaThread = new Thread(new ThreadStart(TwinkleArea));
            //TwinkleAreaThread.Start();//閃爍當下正在run的區域


            XTgtPos1 = IniXPt + Convert.ToDouble(x_width) * (i);

            //寫入X 軸目標位置
            XTarget = PLCAction.Int32Short(XTgtPos1, 10000);
            try
            { PLCAction.axActUtlType1.WriteDeviceBlock2("R1000", 2, ref XTarget[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            XTgtPos2 = PLCAction.ReadPLCData("R1000");

            YTgtPos1 = IniYPt + Convert.ToDouble(y_height) * (j);

            //寫入Y 軸目標位置
            YTarget = PLCAction.Int32Short(YTgtPos1, 10000);
            try
            { PLCAction.axActUtlType1.WriteDeviceBlock2("R1010", 2, ref YTarget[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            YTgtPos2 = PLCAction.ReadPLCData("R1010");


            //寫入X軸速度-----------------------------------
            for (int k = 0; k < AutoSpeedArray.Length; k++)
            {
                if (AutoSpeedArray[k][0] == "(" + (i + 1) + ',' + (j + 1) + ")") //user介面比實際陣列編號多1
                {
                    XSpeed = PLCAction.Int32Short(Convert.ToDouble(AutoSpeedArray[k][1]), 100);
                    YSpeed = PLCAction.Int32Short(Convert.ToDouble(AutoSpeedArray[k][2]), 100);
                    break;
                }
            }

            try
            { PLCAction.axActUtlType1.WriteDeviceBlock2("R1002", 2, ref XSpeed[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            //寫入Y軸速度
            try
            { PLCAction.axActUtlType1.WriteDeviceBlock2("R1012", 2, ref YSpeed[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            //--------------X移動動作--------------
            PLCAction.axActUtlType1.SetDevice("M1002", 0);//自動模式要用一速移,先設為0避免卡住
            FileIO.LogMotion("X一速M1002=0");
            Thread.Sleep(10);
            PLCAction.axActUtlType1.SetDevice("M1002", 1);//自動模式要用一速移動

            Thread.Sleep(10);
            //Console.WriteLine("X一速移動開始 M1002=1");
            FileIO.LogMotion("X一速移動開始 M1002=1");
            //-------------------------------------

            //-----------------判斷是否移動至目標位置完成--------------------
            while (true)
            {
                //Console.WriteLine("X一速接收命令狀態 M1100=" + XCatchStatus);
                //Console.WriteLine("X移動狀態 M1105=" + XMoveStatus);
                //Console.WriteLine("**X實際位置(D1000)=" + XCurPos);
                FileIO.LogMotion("X一速接收命令狀態 M1100=" + XCatchStatus);
                FileIO.LogMotion("X移動狀態 M1105=" + XMoveStatus);
                FileIO.LogMotion("**X實際位置(D1000)=" + XCurPos);

                if (XCatchStatus == 1 && XMoveStatus == 0 && XCurPos == XTgtPos2)  //-----------!!!要改!!
                    break;

            }
            Thread.Sleep(10);
            PLCAction.axActUtlType1.SetDevice("M1002", 0);//自動模式要用一速移動
            //Console.WriteLine("X結束一速接收命令狀態 M1002=0");
            FileIO.LogMotion("X結束一速接收命令狀態 M1002=0");
            //-----------------------------------------------------
            Thread.Sleep(10);
            PLCAction.axActUtlType1.SetDevice("M1012", 0);//自動模式要用一速移,先設為0避免卡住
            FileIO.LogMotion("Y一速移動歸零 M1012=0");
            Thread.Sleep(10);
            PLCAction.axActUtlType1.SetDevice("M1012", 1);//自動模式要用一速移動
            //Console.WriteLine("Y一速移動開始 M1012=1");
            FileIO.LogMotion("Y一速移動開始 M1012=1");
            while (true)
            {
                //Console.WriteLine("**Y 一速接收命令狀態 M1112=" + YCatchStatus);
                //Console.WriteLine("**Y 移動狀態 M1115=" + YMoveStatus);
                //Console.WriteLine("**Y實際位置(D1010)=" + YCurPos);
                FileIO.LogMotion("**Y 一速接收命令狀態 M1112=" + YCatchStatus);
                FileIO.LogMotion("**Y 移動狀態 M1115=" + YMoveStatus);
                FileIO.LogMotion("**Y實際位置(D1010)=" + YCurPos);
                if (YCatchStatus == 1 && YMoveStatus == 0 && YCurPos == YTgtPos2) //-----------!!!要改!!
                {
                    break;
                }

            }
            Thread.Sleep(10);
            PLCAction.axActUtlType1.SetDevice("M1012", 0);//自動模式要用一速移動
            //Console.WriteLine("Y結束一速接收命令狀態 M1100=0");
            FileIO.LogMotion("Y結束一速接收命令狀態 M1100=0");
            Thread.Sleep(10);
            //Console.WriteLine("AutoMarkMode =" + AutoMarkMode);
            FileIO.LogMotion("AutoMarkMode =" + AutoMarkMode);

        }

        private void AutoMarkProcess(int i, int j, string Type) // TYPE區分為打雷射<空>OR純粹移動<MOVE>
        {

            //PLCAction.AutoStageMove(i, j, x_width, y_height, IniXPt, IniYPt, IniZPt, AutoSpeedArray);

            ////-----------------如果是雷射模式就要進行雕刻--------------------
            //if (AutoMarkMode == 1 && Type != "MOVE")
            //{
            //    // Console.WriteLine("---------開始銲接------------");
            //    FileIO.LogMotion("---------開始銲接------------");
            //    StartLaserMark();
            //    //  Console.WriteLine("---------結束銲接------------");
            //    FileIO.LogMotion("---------結束銲接------------");
            //}

            ////FillRectsBySteps(XAreaCnt, YAreaCnt, i, j, 1, "AUTO");//閃爍完後直接著色
            //Bitmap _Bitmap = new Bitmap(pictureBox2.Image);
            //_Bitmap = DrawItems.FillRectsBySteps(_Bitmap, XAreaCnt, YAreaCnt, i, j, 1, "AUTO");
            //pictureBox2.Image = _Bitmap;

            //Thread.Sleep(10);//****重要, 休息時間使得資源來得及release,才不會搶著讀取同一張 bitmap
            //// RestartCnt++;//>0代表已經走過pause啟始執行位置

        }
        private void AutoMark_RedLight(int i)
        {
            PLCAction.axActUtlType1.SetDevice("M1206", 1);
            FileIO.LogMotion("自動化流程 紅光開啟1206=1");
            //開啟紅光
            axMMMark1.StartMarking(3);
            //平台走位, 輸入讀檔的xyz點位
            PLCAction.AutoStageMove(IniXPointList[i], IniYPointList[i], IniZPointList[i], IniSpeedRateList[i]);

            //0316 拿掉
            //PLCAction.axActUtlType1.SetDevice("M1206", 0);
            //FileIO.LogMotion("自動化流程 紅光關閉1206=0");
            axMMMark1.StopMarking();
        }
        private void AutoMark_LaserMark(int i)
        {
            //平台走位, 輸入讀檔的xyz點位
            PLCAction.AutoStageMove(IniXPointList[i], IniYPointList[i], IniZPointList[i], IniSpeedRateList[i]);
            //-----------------如果是雷射模式就要進行雕刻--------------------
            FileIO.LogMotion("---------開始銲接------------");
            StartLaserMark();//雷刻
            FileIO.LogMotion("---------結束銲接------------");
        }
        private void AutoMark()
        {
            ThreadAlive = 1; // thread 執行
            Thread.Sleep(100);
            AutoModeStatus = 2;//啟動

            Bitmap bmp = new Bitmap(PicShowPicBox.Width + 100, PicShowPicBox.Height + 100);

            //====================開始進行自動銲接程序========================
            FileIO.LogMotion("開始進行自動銲接程序");



            //if (AutoMarkMode != 0) //非紅光
            //{
            //    //20210122 for swiroc 只開關雷射一次並等待5秒
            //    PLCAction.axActUtlType1.SetDevice("M1205", 1);
            //    FileIO.LogMotion("雷射準備M1205=1");
            //    System.Threading.Thread thh;
            //    thh = new Thread(new ThreadStart(delegate()
            //        {
            //            FrmLaserWait f = new FrmLaserWait();
            //            f.ShowDialog();
            //        }));
            //    thh.Start();
            //    this.Enabled = false;
            //    Thread.Sleep(8000);
            //    thh.Abort();
            //    this.Enabled = true;
            //}


            for (int i = 0; i < IniAreaCnt; i++)
            {
                //0316 拿掉
                //RedLightOff(); // 紅光關閉防呆
                //1. 切換 MM file
                axMMMark1.LoadFile(IniMMFileList[i]);
                MMfileNameLbl.Text = IniMMFileList[i];
                //顯示加工中區域編碼
                AutoAreaNumLbl2.Text = "(" + IniAreaNumList[i] + ")";

                //2.平台走位與銲接(紅光)
                if (AutoMarkMode == 0) //紅光
                {
                    AutoMark_RedLight(i);
                }
                else //AutoMarkMode == 1 OR 2 為原本版本的自動化銲接第一階段與第二階段
                {
                    AutoMark_LaserMark(i);
                }

                //3.畫動畫
                bmp = DrawItems.DrawBlock(bmp,
                      w * (IniAreaNumXList[i] - 1) + OffsetW, h * (IniAreaNumYList[i] - 1) + OffsetH,
                      w * IniAreaNumXList[i] + OffsetW, h * (IniAreaNumYList[i] - 1) + OffsetH,
                      w * (IniAreaNumXList[i] - 1) + OffsetW, h * IniAreaNumYList[i] + OffsetH,
                      IniPicFileList[i]);
                //補畫方框
                bmp = DrawEmpthRect(bmp);

                PicShowPicBox.Image = bmp;
                PicShowPicBox.Refresh();




            }


            AutoModeStatus = 0;//結束狀態

            //20210122 for swiroc 最後才關雷射
            //PLCAction.axActUtlType1.SetDevice("M1205", 0);
            //FileIO.LogMotion("雷射關閉M1205=0");

            //最後退回初始加工點且不做雷射
            PLCAction.AutoStageMove(IniXPointList[0], IniYPointList[0], IniZPointList[0], IniSpeedRateList[0]);
            MessageBox.Show(this, "已完成本次流程!");
            FileIO.LogMotion("已完成本次流程!");

            EnableOtherBtn();

            AutoMarkMode = 0; //結束完回歸紅光模式避免誤觸
            ThreadAlive = 0; // thread 關閉



        }

        private void DisableOtherBtn()
        {

            AutoGoHomeBtn.Enabled = false;
            AutoGoHomeBtn.BackColor = Color.Gray;

            AutoMarkBtn.Enabled = false;
            AutoMarkBtn.BackColor = Color.Gray;

            AutoLaserMarkBtn.Enabled = false;
            AutoLaserMarkBtn.BackColor = Color.Gray;

            AutoRedLightStage2Btn.Enabled = false;
            AutoRedLightStage2Btn.BackColor = Color.Gray;

            AutoLaserStage2MarkBtn.Enabled = false;
            AutoLaserStage2MarkBtn.BackColor = Color.Gray;

            AutoPageBtn.Enabled = false;
            AutoPage2Btn.Enabled = false;
            ManualPageBtn.Enabled = false;
            RcpMaintainBtn.Enabled = false;
        }
        private void EnableOtherBtn()
        {

            AutoGoHomeBtn.Enabled = true;
            AutoGoHomeBtn.BackColor = Color.Green;

            AutoMarkBtn.Enabled = true;
            AutoMarkBtn.BackColor = Color.Brown;

            AutoLaserMarkBtn.Enabled = true;
            AutoLaserMarkBtn.BackColor = Color.PowderBlue;

            AutoRedLightStage2Btn.Enabled = true;
            AutoRedLightStage2Btn.BackColor = Color.DarkRed;

            AutoLaserStage2MarkBtn.Enabled = true;
            AutoLaserStage2MarkBtn.BackColor = Color.SteelBlue;

            AutoMarkStopBtn.Enabled = true;
            AutoMarkStopBtn.BackColor = Color.Pink;
            AutoMarkStopBtn.ForeColor = Color.Black;

            AutoPageBtn.Enabled = true;
            AutoPage2Btn.Enabled = true;
            ManualPageBtn.Enabled = true;
            RcpMaintainBtn.Enabled = true;

        }
        private void AutoMarkBtn_Click(object sender, EventArgs e)
        {
            FileIO.LogAction(AutoMarkBtn.Text);
            //-------------
            AutoMarkMode = 0; //紅光


            if (IniAreaCnt > 0)
            {
                DisableOtherBtn();
                AutoMsgTxtBox.Text += "------自動模式執行------\r\n";
                InitialMap();
                Thread.Sleep(100);
                //繪圖 + 平台移動 + 雷刻執行 
                //使用 thread 才能按鈕中止
                AutoMarkThreadFlag = 1; // Auto 程序 Thred 啟動
                AutoMarkThread = new Thread(new ThreadStart(AutoMark));
                Form.CheckForIllegalCrossThreadCalls = false; // 存取 UI 時需要用,較不安全的寫法,改用委派較佳(EX: UPDATE TXTBOX)

                AutoMarkThread.Start();
            }
            else
                MessageBox.Show(this, "請選擇工單!!");

        }

        private void button18_Click(object sender, EventArgs e)
        {
            VerifyPW VerifyPW = new VerifyPW();
            VerifyPW.Owner = this;
            VerifyPW.ShowDialog();
            MessageBox.Show(this, PWStr);
            int Result = AccountVerify("PE", PWStr);
            if (Result > 0) return;


            this.tabPage2.Parent = null;
            // this.tabPage1.Parent = null;
            this.tabPage3.Parent = null;
            this.tabPage6.Parent = null;
            this.tabPage4.Parent = null;
            this.tabPage7.Parent = null;
            //this.tabPage5.Parent = this.tabControl1;


        }


        private void button11_Click(object sender, EventArgs e)
        {
            //axActUtlType1.SetDevice("M1003", 1);
            //ManualMsgTxtBox.Text += "X原點復歸開始 M1003=1\r\n";
            //axActUtlType1.SetDevice("M1013", 1);
            //ManualMsgTxtBox.Text += "X原點復歸開始 M1003=1\r\n";
        }

        private void FindMsx()
        {

        }
        private void XSelectAreaComboBox_DropDown(object sender, EventArgs e)
        {

            ClearPicShow();

            XSelectAreaComboBox.Items.Clear();

            int XMaxAreaNum = PLCAction.FindMaxNum(IniAreaNumXList);

            for (int i = 0; i < XMaxAreaNum; i++)
                XSelectAreaComboBox.Items.Add(i + 1);//UI端從1開始

            if (IniAreaCnt == 0)
            {
                MessageBox.Show(this, "未選擇工單!");
                return;
            }

        }

        private void YSelectAreaComboBox_DropDown(object sender, EventArgs e)
        {
            ClearPicShow();

            YSelectAreaComboBox.Items.Clear();

            int YMaxAreaNum = PLCAction.FindMaxNum(IniAreaNumYList);
            for (int i = 0; i < YMaxAreaNum; i++)
                YSelectAreaComboBox.Items.Add(i + 1);//UI端從1開始

            if (IniAreaCnt == 0)
            {
                MessageBox.Show(this, "未選擇工單!");
                return;
            }
        }

        private void AutoSpeedSetBtn_Click(object sender, EventArgs e)
        {
            FileIO.LogAction(AutoSpeedSetBtn.Text);
            AutoAreaSpeedSet AutoAreaSpeedSetFrm = new AutoAreaSpeedSet();
            XAreaCnt = Convert.ToInt32(XAreaCntTxtBox.Text);
            YAreaCnt = Convert.ToInt32(YAreaCntTxtBox.Text);

            AutoAreaSpeedSetFrm.Owner = this;
            AutoAreaSpeedSetFrm.XAreaTotalCnt = XAreaCnt;
            AutoAreaSpeedSetFrm.YAreaTotalCnt = YAreaCnt;
            AutoAreaSpeedSetFrm.MemoAutoSpeedArray = SetAutoSpeedArray;
            AutoAreaSpeedSetFrm.ShowDialog();
            //初始化先設各區速度陣列
            SetAutoSpeedArray = new string[XAreaCnt * YAreaCnt][];
            SetAutoMMArray = new string[XAreaCnt * YAreaCnt][];
            for (int i = 0; i < XAreaCnt * YAreaCnt; i++)
            {
                SetAutoSpeedArray[i] = new string[4];
                SetAutoMMArray[i] = new string[3];
            }

            SetAutoSpeedArray = AutoAreaSpeedSetFrm._AutoSpeedArray;

        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            FileIO.LogAction(button8.Text);
            this.tabPage2.Parent = null;
            //this.tabPage1.Parent = null;
            this.tabPage3.Parent = null;
            this.tabPage8.Parent = null;
            this.tabPage4.Parent = null;
            this.tabPage7.Parent = null;
            this.tabPage6.Parent = this.tabControl1;
            if (Timer4Flag == 1)
                timer4.Dispose();
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void 開啟製程參數檔ToolStripMenuItem_Click(object sender, EventArgs e)
        {



        }
        private void ReadMMFilePara()
        {

        }

        private void AutoRecipeOpenBtn_Click(object sender, EventArgs e)
        {



        }

        private void AutoSpeedSetBtn_Click_1(object sender, EventArgs e)
        {

        }

        private void XSelectAreaComboBox_DropDownClosed(object sender, EventArgs e)
        {
        }
        private void ManualSelectArea()
        {
            short[] XTarget = new short[2];
            short[] YTarget = new short[2];

            double XTgtPos1;
            double YTgtPos1;
            double XTgtPos2;
            double YTgtPos2;
            if (XSelectAreaComboBox.Text.ToString().Length > 0 && YSelectAreaComboBox.Text.Length.ToString().Length > 0)
            {
                ManualMsgTxtBox.Text += "--------手動移動至指定區域模式--------" + "\r\n";
                ManualMode = 3;


                //---------讀取&儲存座標資料------------
                ManualMsgTxtBox.Text += "X實際位置(D1000)=";
                ManualMsgTxtBox.Text += Convert.ToString(XCurPos) + "\r\n";

                //中心點
                //XTgtPos1 = Convert.ToDouble(x_width) / 2 + (Convert.ToDouble(x_width) ) * ((Convert.ToDouble(XSelectAreaComboBox.Text))-1);
                XTgtPos1 = IniXPt + Convert.ToDouble(x_width) * ((Convert.ToDouble(XSelectAreaComboBox.Text)) - 1);
                ManualMsgTxtBox.Text += "寫入X目標位置(R1000)=";

                //寫入X 軸目標位置
                XTarget = PLCAction.Int32Short(XTgtPos1, 10000);
                try
                { axActUtlType1.WriteDeviceBlock2("R1000", 2, ref XTarget[0]); }
                catch (Exception exception)
                { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                XTgtPos2 = PLCAction.ReadPLCData("R1000");
                ManualMsgTxtBox.Text += Convert.ToString(XTgtPos2) + "\r\n";


                ManualMsgTxtBox.Text += "Y實際位置(D1010)=";
                ManualMsgTxtBox.Text += Convert.ToString(YCurPos) + "\r\n";
                //中心點
                //YTgtPos1 = Convert.ToDouble(y_height) / 2 + (Convert.ToDouble(y_height) ) * (Convert.ToDouble(YSelectAreaComboBox.Text)-1);
                YTgtPos1 = IniYPt + Convert.ToDouble(y_height) * ((Convert.ToDouble(YSelectAreaComboBox.Text)) - 1);
                ManualMsgTxtBox.Text += "寫入Y目標位置(R1010)=";
                //寫入Y 軸目標位置
                YTarget = PLCAction.Int32Short(YTgtPos1, 10000);
                try
                { axActUtlType1.WriteDeviceBlock2("R1010", 2, ref YTarget[0]); }
                catch (Exception exception)
                { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                YTgtPos2 = PLCAction.ReadPLCData("R1010");
                ManualMsgTxtBox.Text += Convert.ToString(YTgtPos2) + "\r\n";
                //------------------------------
                //-----------移動至目標位置-----------------
                axActUtlType1.SetDevice("M1002", 0);//自動模式要用一速移,先設為0避免卡住
                FileIO.LogMotion("x一速M1002=0");
                axActUtlType1.SetDevice("M1002", 1);//自動模式要用一速移動
                FileIO.LogMotion("x一速M1002=1");
                //ManualMsgTxtBox.Text += "--\r\n";
                //ManualMsgTxtBox.Text += "X一速移動開始 M1002=1\r\n";
                Thread.Sleep(100);

                while (true)
                {
                    axActUtlType1.GetDevice("M1102", out XCatchStatus);
                    //ManualMsgTxtBox.Text += "X一速接收命令狀態 M1102=" + Convert.ToString(XCatchStatus) + "\r\n";
                    FileIO.LogMotion("X一速接收命令狀態 M1102=" + Convert.ToString(XCatchStatus));

                    axActUtlType1.GetDevice("M1105", out XMoveStatus);
                    //ManualMsgTxtBox.Text += "X移動狀態 M1105=" + Convert.ToString(XMoveStatus) + "\r\n";
                    FileIO.LogMotion("X一速接收命令狀態 M1102=" + Convert.ToString(XCatchStatus));

                    XCurPos = Convert.ToDouble(PLCAction.ReadPLCData("D1000"));
                    //ManualMsgTxtBox.Text += "X實際位置(D1000)=";
                    //ManualMsgTxtBox.Text += Convert.ToString(XCurPos) + ",X 目標位置=" + Convert.ToString(XTgtPos2) + "\r\n";
                    FileIO.LogMotion("X實際位置(D1000)=" + Convert.ToString(XCurPos) + ",X 目標位置=" + Convert.ToString(XTgtPos2));
                    Thread.Sleep(100);
                    if (XCatchStatus == 1 && XMoveStatus == 0 && XCurPos == XTgtPos2)  //-----------!!!要改!!
                        break;

                }
                axActUtlType1.SetDevice("M1002", 0);//自動模式要用一速移動
                //ManualMsgTxtBox.Text += "--\r\n";
                //ManualMsgTxtBox.Text += "X一速移動結束 M1002=0\r\n";
                FileIO.LogMotion("X一速移動結束 M1002=0");
                //------------------------------------------
                Thread.Sleep(100);
                //-------------------------------------------
                axActUtlType1.SetDevice("M1012", 0);//自動模式要用一速移,先設為0避免卡住
                FileIO.LogMotion("Y一速移動結束 M1012=0");
                axActUtlType1.SetDevice("M1012", 1);//自動模式要用一速移動
                FileIO.LogMotion("Y一速移動開始 M1012=1");
                //ManualMsgTxtBox.Text += "--\r\n";
                //ManualMsgTxtBox.Text += "Y一速移動開始 M1012=1\r\n";
                Thread.Sleep(100);
                while (true)
                {
                    axActUtlType1.GetDevice("M1112", out YCatchStatus);
                    //ManualMsgTxtBox.Text += "Y 一速接收命令狀態 M1112=" + Convert.ToString(YCatchStatus) + "\r\n";
                    FileIO.LogMotion("Y 一速接收命令狀態 M1112=" + Convert.ToString(YCatchStatus));
                    axActUtlType1.GetDevice("M1115", out YMoveStatus);
                    FileIO.LogMotion("Y移動狀態 M1115=" + Convert.ToString(YMoveStatus));
                    //ManualMsgTxtBox.Text += "Y移動狀態 M1115=" + Convert.ToString(YMoveStatus) + "\r\n";

                    YCurPos = Convert.ToDouble(PLCAction.ReadPLCData("D1010"));
                    FileIO.LogMotion("Y實際位置(D1010)=" + Convert.ToString(YCurPos) + ",Y 目標位置=" + Convert.ToString(YTgtPos2));
                    //ManualMsgTxtBox.Text += "Y實際位置(D1010)=";
                    //ManualMsgTxtBox.Text += Convert.ToString(YCurPos) + ",Y 目標位置=" + Convert.ToString(YTgtPos2) + "\r\n";

                    Thread.Sleep(100);
                    if (YCatchStatus == 1 && YMoveStatus == 0 && YCurPos == YTgtPos2) //-----------!!!要改!!
                    {
                        ManualMarkBtn.Enabled = true;
                        //ManualMarkBtn.BackColor = Color.Crimson;
                        break;
                    }

                }
                axActUtlType1.SetDevice("M1012", 0);//自動模式要用一速移動
                FileIO.LogMotion("X一速移動結束M1012=0");
                //ManualMsgTxtBox.Text += "--\r\n";
                //ManualMsgTxtBox.Text += "X一速移動結束 M1012=0\r\n";



            }




        }
        private void XSelectAreaComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            axMMMark1.StopMarking();
            if (IniAreaCnt > 0)
            {


            }
            // 繪製 M*N示意圖
            if (x_width != 0 && y_height != 0)
            {


                //   InitialMap();
                _Bitmap = DrawItems.FillRectsBySteps(_Bitmap, XAreaCnt, YAreaCnt, Convert.ToInt32(XSelectAreaComboBox.Text) - 1, Convert.ToInt32(YSelectAreaComboBox.Text) - 1, 2, "MANUAL");
                PicShowPicBox.Image = _Bitmap;
                ManualSet();


                //stage2則自動代出對應的 ezm 檔
                if (ManualLaserStage == 2) //自動流程第二階
                {
                    // 找要切換的 EZM檔
                    for (int k = 0; k < AutoMMArray.Length; k++)
                    {
                        if (AutoMMArray[k][0] == "(" + XSelectAreaComboBox.Text + ',' + YSelectAreaComboBox.Text + ")") //user介面比實際陣列編號多1
                        {
                            axMMMark1.LoadFile(AutoMMArray[k][1]);
                            MMfileNameLbl.Text = AutoMMArray[k][1];
                            break;
                        }
                    }

                }
            }
            //可以看到但要先鎖住
            DisableOtherManualBtn();
        }

        private void YSelectAreaComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            axMMMark1.StopMarking();
            // 繪製 M*N示意圖
            if (x_width != 0 && y_height != 0)
            {

                InitialMap();
                _Bitmap = DrawItems.FillRectsBySteps(_Bitmap, XAreaCnt, YAreaCnt, Convert.ToInt32(XSelectAreaComboBox.Text) - 1, Convert.ToInt32(YSelectAreaComboBox.Text) - 1, 2, "MANUAL");
                PicShowPicBox.Image = _Bitmap;



                //stage2則自動代出對應的 ezm 檔
                if (ManualLaserStage == 2) //自動流程第二階
                {
                    // 找要切換的 EZM檔
                    for (int k = 0; k < AutoMMArray.Length; k++)
                    {
                        if (AutoMMArray[k][0] == "(" + XSelectAreaComboBox.Text + ',' + YSelectAreaComboBox.Text + ")") //user介面比實際陣列編號多1
                        {
                            axMMMark1.LoadFile(AutoMMArray[k][1]);
                            MMfileNameLbl.Text = AutoMMArray[k][1];
                            break;
                        }
                    }

                }
                //ManualSet();
                //AutoMarkProcess((Convert.ToInt32(XSelectAreaComboBox.Text) - 1), (Convert.ToInt32(YSelectAreaComboBox.Text) - 1));
            }
            //可以看到但要先鎖住
            DisableOtherManualBtn();
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox9_Enter(object sender, EventArgs e)
        {

        }

        private void ManualScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            double SpeedRate;
            SpeedRate = Convert.ToDouble(ManualScrollBar2.Value);//*Convert.ToDouble(hScrollBar1.Width) / Convert.ToDouble(100.0);
            ManualSpeedRateLbl.Text = SpeedRate.ToString();
            ManualSet();
        }

        private void ManualStepDistComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ManualSet();
        }

        private void ManualXSpeedTxtBox_TextChanged(object sender, EventArgs e)
        {
            ManualSet();
        }

        private void ManualYSpeedTxtBox_TextChanged(object sender, EventArgs e)
        {
            ManualSet();
        }

        private void button16_Click(object sender, EventArgs e)
        {
        }

        private void ManualMarkBtn_Click(object sender, EventArgs e)
        {
            if (CloseDoor == 0)
            {
                ProgramStatus = "Manual_Wielding";
                FileIO.LogAction(ManualMarkBtn.Text);
                DialogResult AutoLaserChkResult;
                //   if (ProdCodeTxtBox.Text == "Default")//條碼=default
                if (ProdCodeTxtBox.Text.Length == 0)
                {
                    AutoLaserChkResult =
                        MessageBox.Show(this, "條碼尚未掃描成功,確定要繼續嗎?",
                                        "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (AutoLaserChkResult == DialogResult.No)
                        return;
                }


                ManualLaserStartThread = new Thread(new ThreadStart(ManualLaserStart));
                Form.CheckForIllegalCrossThreadCalls = false; // 存取 UI 時需要用,較不安全的寫法,改用委派較佳(EX: UPDATE TXTBOX)
                ManualLaserStartThread.Start();
            }
            else
            {
                MessageBox.Show(this, "機台門未關閉!請重新確認!!");
            }
        }
        private void ManualLaserStart()
        {
            //----------------產生起始生管單----------------
            //生管單基本資料
            dt1 = DateTime.Now;
            string PMRemark = "Area=(" + XSelectAreaComboBox.Text + "_" + YSelectAreaComboBox.Text + ")";
            PordMgrFileStart(ProgramStatus, dt1.ToString(), ProdMgrFilePath, PMRemark + "_" + ProdAlmMsg);//補銲reamark註記
            //---------------------------------------------------

            int LaserTrigStatus = 0;
            if (_args == "1")//20210120 ipc才要問1215
            {
                PLCAction.axActUtlType1.GetDevice("M1215", out LaserTrigStatus);
                FileIO.LogMotion(" LaserTrigStatus M1215=" + LaserTrigStatus);
            }
            else if (_args == "2")//20210120 swiroc 跳過問1215
            {

                LaserTrigStatus = 0;
            }

            if (LaserTrigStatus == 0)//當下狀態為雷射關閉時,要開啟
            {
                DialogResult ManualLaserChkResult = MessageBox.Show(this, "確定要激發雷射嗎?", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (ManualLaserChkResult == DialogResult.No)
                    return;
                //if (_args == "2")//20210120 swiroc 跳過問1215
                //{
                //    //20210122 for swiroc 只開關雷射一次並等待5秒
                //    PLCAction.axActUtlType1.SetDevice("M1205", 1);
                //    FileIO.LogMotion("雷射準備M1205=1");
                //    System.Threading.Thread thh;
                //    thh = new Thread(new ThreadStart(delegate()
                //    {
                //        FrmLaserWait f = new FrmLaserWait();
                //        f.ShowDialog();
                //    }));
                //    thh.Start();
                //    this.Enabled = false;
                //    Thread.Sleep(8000);
                //    thh.Abort();
                //    this.Enabled = true;
                //}
                StartLaserMark();

                //紅光按鈕復原
                ManualRedLightBtn.Text = "紅光開啟";
                ManualRedLightBtn.BackColor = Color.Brown;
                ManualRedLightBtn.ForeColor = Color.Black;

            }//end if

            //----------------產生結束生管單-----------------
            //生管單基本資料

            TimeSpan ts = DateTime.Now - dt1;
            double TimeDiff = Convert.ToDouble(ts.TotalSeconds.ToString());
            string StrTimeDiff = TimeDiff.ToString("#0.00");
            // string PMRemark = "Area=(" + XSelectAreaComboBox.Text + "_" + YSelectAreaComboBox.Text + ")";
            PordMgrFileEnd(ProgramStatus, DateTime.Now.ToString(),
                            StrTimeDiff, ProdMgrFilePath, PMRemark + "_" + ProdAlmMsg);

            //---------------------------------------------------
            MessageBox.Show(this, "已完本次手動補銲!");

        }
        private void RedLightOff()
        {
            //防呆停止紅光預覽
            PLCAction.axActUtlType1.SetDevice("M1206", 0);
            FileIO.LogMotion("防呆停止紅光預覽M1206=0");
            axMMMark1.StopMarking();
            ManualRedLightBtn.Text = "紅光開啟";
            ManualRedLightBtn.BackColor = Color.Brown;
            ManualRedLightBtn.ForeColor = Color.Black;

        }
        private void StartLaserMark()
        {
            //防呆停止紅光預覽
            //PLCAction.axActUtlType1.SetDevice("M1206", 0);
            RedLightOff();
            axMMMark1.StopMarking();
            int LaserTrigStatus = 0;

            //20210302 不管是 swiroc 或是 ipc 都只能開一次雷射
            //原本是關著的才要打開 原本是開的就不用再開一次
            PLCAction.axActUtlType1.GetDevice("M1205", out LaserTrigStatus);
            if (LaserTrigStatus == 0)
            {
                PLCAction.axActUtlType1.SetDevice("M1205", 1);
                FileIO.LogMotion("雷射出光原本off，雷射準備M1205=1");
            }
            //20210122 for swiroc 只開關雷射一次

            while (true)
            {
                if (_args == "1")//20210120 ipc才要問1215
                {
                    PLCAction.axActUtlType1.GetDevice("M1215", out LaserTrigStatus);
                    FileIO.LogMotion("雷射準備狀態M1215" + LaserTrigStatus);
                }
                else if (_args == "2")//20210120 swiroc 跳過問1215
                {
                    PLCAction.axActUtlType1.GetDevice("M1205", out LaserTrigStatus);
                    FileIO.LogMotion("SWIROC雷射準備狀態M1205" + LaserTrigStatus);

                }
                if (LaserTrigStatus == 1)// 雷射開啟狀態ok
                {
                    if (ActionMode == 1)//補焊才要挑選區域
                        axMMMark1.SetMarkSelect(1);
                    else if (ActionMode == 0)
                        axMMMark1.SetMarkSelect(0);

                    //-----------------------
                    FinishLaserMarking = 0; //表示準備出光,此時不可abort thread
                    //-----------------------

                    axMMMark1.StartMarking(1);

                    //20210122 swiroc 必須維持雷射開著否則有問題
                    //PLCAction.axActUtlType1.SetDevice("M1205", 0);
                    //FileIO.LogMotion("雷射關閉M1205=0" + LaserTrigStatus);
                    //---------------------
                    FinishLaserMarking = 1;//表示出光完成,此時可透過 IntrTimer abort thread
                    //---------------------
                    break;
                }
            }//end while

        }

        private void button17_Click(object sender, EventArgs e)
        {
            axMMMark1.AutoCal(1, 0);
        }

        private void button21_Click(object sender, EventArgs e)
        {
            axMMMark1.AutoCal(1, 1);
        }

        private void button22_Click(object sender, EventArgs e)
        {
            // axMMIO1.SetOutput(3, 1);
        }

        private void TestStepRadioBtn_Click(object sender, EventArgs e)
        {
            TestNoStopRadioBtn.Checked = false;
            ManualMode = 2;
        }

        private void TestNoStopRadioBtn_Click(object sender, EventArgs e)
        {
            TestStepRadioBtn.Checked = false;
            ManualMode = 1;
        }

        private void TestPosXBtn_Click(object sender, EventArgs e)
        {
            if (ManualMode == 2)
            {
                axActUtlType1.SetDevice("M1001", 1);
                //ManualMsgTxtBox.Text += "X負向移動開始 M1000=1\r\n";
                Thread.Sleep(100);

                while (true)
                {
                    axActUtlType1.GetDevice("M1101", out XManualCatchStatus);
                    axActUtlType1.GetDevice("M1105", out XMoveStatus);
                    ManualMsgTxtBox.Text += "X接收命令狀態 M1101=" + Convert.ToString(XManualCatchStatus) + "\r\n";
                    ManualMsgTxtBox.Text += "X移動狀態 M1105=" + Convert.ToString(XMoveStatus) + "\r\n";
                    Thread.Sleep(100);
                    if (XManualCatchStatus == 1 && XMoveStatus == 0)// 0 移動中  =>//-----------!!!要改!!
                    {
                        ManualMarkBtn.Enabled = true;
                        //ManualMarkBtn.BackColor = Color.Crimson;
                        break;
                    }

                }

                axActUtlType1.SetDevice("M1001", 0);
                ManualMsgTxtBox.Text += "X負向移動結束 M1000=0\r\n";
            }
        }

        private void TestPosXBtn_MouseDown(object sender, MouseEventArgs e)
        {
            //要記得先連通PLC才能執行
            if (ManualMode == 1)
            {
                axActUtlType1.SetDevice("M1001", 1);
                ManualMsgTxtBox.Text += "X負向移動開始 M1000=1\r\n";

            }
        }

        private void TestPosXBtn_MouseUp(object sender, MouseEventArgs e)
        {
            if (ManualMode == 1)
            {
                axActUtlType1.SetDevice("M1001", 0);
                ManualMsgTxtBox.Text += "X負向移動結束 M1000=0\r\n";

            }
        }

        private void TestXNegBtn_MouseDown(object sender, MouseEventArgs e)
        {
            //要記得先連通PLC才能執行
            if (ManualMode == 1)
            {
                axActUtlType1.SetDevice("M1000", 1);
                ManualMsgTxtBox.Text += "X正向移動開始 M1000=1\r\n";

            }
        }

        private void TestXNegBtn_Move(object sender, EventArgs e)
        {

        }

        private void TestXNegBtn_MouseUp(object sender, MouseEventArgs e)
        {
            //要記得先連通PLC才能執行
            if (ManualMode == 1)
            {
                axActUtlType1.SetDevice("M1000", 0);
                ManualMsgTxtBox.Text += "X正向移動結束 M1000=0\r\n";

            }
        }

        private void TestYPosBtn_MouseDown(object sender, MouseEventArgs e)
        {
            //要記得先連通PLC才能執行
            if (ManualMode == 1)
            {
                axActUtlType1.SetDevice("M1010", 1);
                ManualMsgTxtBox.Text += "Y正向移動開始 M1000=1\r\n";

            }
        }

        private void TestYPosBtn_MouseUp(object sender, MouseEventArgs e)
        {
            //要記得先連通PLC才能執行
            if (ManualMode == 1)
            {
                axActUtlType1.SetDevice("M1010", 0);
                ManualMsgTxtBox.Text += "Y正向移動結束 M1000=0\r\n";

            }
        }

        private void TestYNegBtn_MouseDown(object sender, MouseEventArgs e)
        {
            //要記得先連通PLC才能執行
            if (ManualMode == 1)
            {
                axActUtlType1.SetDevice("M1011", 1);
                ManualMsgTxtBox.Text += "Y負向移動開始 M1011=1\r\n";

            }
        }

        private void TestYNegBtn_MouseUp(object sender, MouseEventArgs e)
        {
            //要記得先連通PLC才能執行
            if (ManualMode == 1)
            {
                axActUtlType1.SetDevice("M1011", 0);
                ManualMsgTxtBox.Text += "Y負向移動結束 M1011=0\r\n";

            }
        }

        private void TestXNegBtn_Click(object sender, EventArgs e)
        {
            if (ManualMode == 2)
            {
                axActUtlType1.SetDevice("M1000", 1);
                ManualMsgTxtBox.Text += "X正向移動開始 M1000=1\r\n";
                Thread.Sleep(100);

                while (true)
                {
                    axActUtlType1.GetDevice("M1100", out XManualCatchStatus);
                    axActUtlType1.GetDevice("M1105", out XMoveStatus);
                    ManualMsgTxtBox.Text += "X接收命令狀態 M1100=" + Convert.ToString(XManualCatchStatus) + "\r\n";
                    ManualMsgTxtBox.Text += "X移動狀態 M1105=" + Convert.ToString(XMoveStatus) + "\r\n";
                    Thread.Sleep(100);
                    if (XManualCatchStatus == 1 && XMoveStatus == 0)// 0靜止
                    {
                        ManualMarkBtn.Enabled = true;
                        //ManualMarkBtn.BackColor = Color.Crimson;
                        break;
                    }

                }

                axActUtlType1.SetDevice("M1000", 0);
                ManualMsgTxtBox.Text += "X正向移動結束 M1000=0\r\n";
            }
        }

        private void TestYPosBtn_Click(object sender, EventArgs e)
        {
            if (ManualMode == 2)
            {
                axActUtlType1.SetDevice("M1010", 1);
                ManualMsgTxtBox.Text += "Y負向移動開始 M1010=1\r\n";
                Thread.Sleep(100);

                while (true)
                {
                    axActUtlType1.GetDevice("M1110", out YManualCatchStatus);
                    axActUtlType1.GetDevice("M1115", out YMoveStatus);
                    ManualMsgTxtBox.Text += "Y接收命令狀態 M1110=" + Convert.ToString(YManualCatchStatus) + "\r\n";
                    ManualMsgTxtBox.Text += "Y移動狀態 M1115=" + Convert.ToString(YMoveStatus) + "\r\n";
                    Thread.Sleep(100);
                    if (YManualCatchStatus == 1 && YMoveStatus == 0)
                    {
                        ManualMarkBtn.Enabled = true;
                        break;
                    }

                }

                axActUtlType1.SetDevice("M1010", 0);
                ManualMsgTxtBox.Text += "Y負向移動結束 M1010=0\r\n";
            }
        }

        private void TestYNegBtn_Click(object sender, EventArgs e)
        {
            if (ManualMode == 2)
            {

                axActUtlType1.SetDevice("M1011", 1);
                ManualMsgTxtBox.Text += "Y負向移動開始 M1011=1\r\n";
                Thread.Sleep(100);

                while (true)
                {
                    axActUtlType1.GetDevice("M1111", out YManualCatchStatus);
                    axActUtlType1.GetDevice("M1115", out YMoveStatus);
                    ManualMsgTxtBox.Text += "Y接收命令狀態 M1111=" + Convert.ToString(YManualCatchStatus) + "\r\n";
                    ManualMsgTxtBox.Text += "Y移動狀態 M1115=" + Convert.ToString(YMoveStatus) + "\r\n";
                    Thread.Sleep(100);
                    if (YManualCatchStatus == 1 && YMoveStatus == 0)
                    {
                        ManualMarkBtn.Enabled = true;
                        break;
                    }

                }

                axActUtlType1.SetDevice("M1011", 0);
                ManualMsgTxtBox.Text += "Y負向移動結束 M1011=0\r\n";


            }
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void AutoGoHomeBtn_Click(object sender, EventArgs e)
        {
            ProgramStatus = "GoHome";
            FileIO.LogAction(AutoGoHomeBtn.Text);
            DisableOtherBtn();
            // 20180920 蕭兄建議 gohome 要能取消
            //歸 Home 按鈕也要disable 取消，因為不能中離
            //AutoMarkStopBtn.Enabled = false;
            //AutoMarkStopBtn.BackColor = Color.Gray;
            GoHomeThreadFlag = 1;
            GoHomeThread = new Thread(new ThreadStart(GoHome));
            Form.CheckForIllegalCrossThreadCalls = false; // 存取 UI 時需要用,較不安全的寫法,改用委派較佳(EX: UPDATE TXTBOX)

            GoHomeThread.Start();
            AutoGoHomeBtn.Enabled = false;
            GoHomePause = 0;
        }
        private void GoHome()
        {
            //----------------產生起始生管單----------------
            //生管單基本資料
            //FileIO.IniProdMgrFile2();
            //String ProgramStatus2 = "開始" + ProgramStatus;
            //FileIO.SetProdMgrFile("MODULE", ProdCodeTxtBox.Text);
            //FileIO.SetProdMgrFile("STEP", ProgramStatus2);
            DateTime dt1 = DateTime.Now;
            //FileIO.SetProdMgrFile("START_TIME", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            //FileIO.LogProdMgrFile(ProgramStatus2, ProdMgrFilePath);
            // string PMRemark = "Area=(" + XSelectAreaComboBox.Text + "_" + YSelectAreaComboBox.Text + ")";
            PordMgrFileStart(ProgramStatus, dt1.ToString(), ProdMgrFilePath, ProdAlmMsg);//自動reamk為空
            //---------------------------------------------------
            PLCAction.GoHome();
            EnableOtherBtn();
            //----------------產生結束生管單-----------------
            //生管單基本資料

            TimeSpan ts = DateTime.Now - dt1;
            double TimeDiff = Convert.ToDouble(ts.TotalSeconds.ToString());
            string StrTimeDiff = TimeDiff.ToString("#0.00");

            PordMgrFileEnd(ProgramStatus, DateTime.Now.ToString(),
                            StrTimeDiff, ProdMgrFilePath, ProdAlmMsg);
            //---------------------------------------------------
        }

        private void button12_Click(object sender, EventArgs e)
        {

        }

        private void button13_Click(object sender, EventArgs e)
        {
            axActUtlType1.SetDevice("M1206", 0);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            // this.tabPage1.Parent = null;
            this.tabPage2.Parent = null;
            this.tabPage6.Parent = null;
            this.tabPage3.Parent = null;
            this.tabPage8.Parent = null;
            this.tabPage4.Parent = null;
            this.tabPage7.Parent = this.tabControl1;

            //雷射源手動頁面專用 timer
            TimerCallback callback3 = new TimerCallback(_do3);
            //每一秒起來一次
            timer4 = new System.Threading.Timer(callback3, null, 0, 500);

        }
        private void _do3(object state)
        {

            this.BeginInvoke(new ManualLaserTag1(ManualLaserTag2));//委派

        }

        delegate void ManualLaserTag1();
        private void ManualLaserTag2()
        {
            Timer4Flag = 1;
            double[] _ReadVal = PLCAction.ReadPLCDataRandom("X30\nX31\nX32\nX33\nX34\nX35\nX36\nX37\nX38\nX39\nX3A\nX3B\nX3C" +
                                                   "\nY70\nY71\nY72\nY73\nY74\nY75\nY76\nY77", 21);//\nX3D",14);//\nX3E", 15);  //\nX3F", 16); //後面這三個抓不到
            //// hardcode 上面順序比較節省尋找時間


            if (_ReadVal[0] == 1)
                X30Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[0] == 0)
                X30Lbl.BackColor = Color.Bisque;

            if (_ReadVal[1] == 1)
                X31Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[1] == 0)
                X31Lbl.BackColor = Color.Bisque;

            if (_ReadVal[2] == 1)
                X32Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[2] == 0)
                X32Lbl.BackColor = Color.Bisque;

            if (_ReadVal[3] == 1)
                X33Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[3] == 0)
                X33Lbl.BackColor = Color.Bisque;

            if (_ReadVal[4] == 1)
                X34Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[4] == 0)
                X34Lbl.BackColor = Color.Bisque;

            if (_ReadVal[5] == 1)
                X35Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[5] == 0)
                X35Lbl.BackColor = Color.Bisque;

            if (_ReadVal[6] == 1)
                X36Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[6] == 0)
                X36Lbl.BackColor = Color.Bisque;

            if (_ReadVal[7] == 1)
                X37Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[7] == 0)
                X37Lbl.BackColor = Color.Bisque;

            if (_ReadVal[8] == 1)
                X38Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[8] == 0)
                X38Lbl.BackColor = Color.Bisque;

            if (_ReadVal[9] == 1)
                X39Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[9] == 0)
                X39Lbl.BackColor = Color.Bisque;

            if (_ReadVal[10] == 1)
                X3ALbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[10] == 0)
                X3ALbl.BackColor = Color.Bisque;

            if (_ReadVal[11] == 1)
                X3BLbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[11] == 0)
                X3BLbl.BackColor = Color.Bisque;

            if (_ReadVal[12] == 1)
                X3CLbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[12] == 0)
                X3CLbl.BackColor = Color.Bisque;

            if (_ReadVal[13] == 1)
                Y70Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[13] == 0)
                Y70Lbl.BackColor = Color.Tan;

            if (_ReadVal[14] == 1)
                Y71Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[14] == 0)
                Y71Lbl.BackColor = Color.Tan;

            if (_ReadVal[15] == 1)
                Y72Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[15] == 0)
                Y72Lbl.BackColor = Color.Tan;

            if (_ReadVal[16] == 1)
                Y73Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[16] == 0)
                Y73Lbl.BackColor = Color.Tan;

            if (_ReadVal[17] == 1)
                Y74Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[17] == 0)
                Y74Lbl.BackColor = Color.Tan;

            if (_ReadVal[18] == 1)
                Y75Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[18] == 0)
                Y75Lbl.BackColor = Color.Tan;

            if (_ReadVal[19] == 1)
                Y76Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[19] == 0)
                Y76Lbl.BackColor = Color.Tan;

            if (_ReadVal[20] == 1)
                Y77Lbl.BackColor = Color.LimeGreen;
            else if (_ReadVal[20] == 0)
                Y77Lbl.BackColor = Color.Tan;

            X3ELbl.BackColor = Color.LimeGreen;
            X3FLbl.BackColor = Color.LimeGreen;

        }

        private void ManualAsnPosRadioBtn_Click(object sender, EventArgs e)
        {
            FileIO.LogAction(ManualAsnPosRadioBtn.Text);

            ClearPicShow();

            //初始化
            DisableOtherManualBtn2();
            ManualAsnPosRadioBtn.Checked = true;
            ManualXPosTxtBox.Enabled = true;
            ManualYPosTxtBox.Enabled = true;
            ManualZPosTxtBox.Enabled = true;
            ManualAsnPosMoveBtn.Enabled = true;
            ManualAsnPosMoveBtn.BackColor = Color.SteelBlue;

        }

        private void button13_Click_1(object sender, EventArgs e)
        {


        }

        private void ManualNoStopRadioBtn_Click(object sender, EventArgs e)
        {
            FileIO.LogAction(ManualNoStopRadioBtn.Text);
            ManualStepDistComboBox.Enabled = true;
            XSelectAreaComboBox.Enabled = false;
            YSelectAreaComboBox.Enabled = false;
            ManualGoToAreaRadioBox.Checked = false;
            ManualAsnPosRadioBtn.Checked = false;
            ManualAreaAsnMoveBtn.Enabled = false;
            ManualAreaAsnMoveBtn.BackColor = Color.Gray;
            ManualAsnPosMoveBtn.Enabled = false;
            ManualAsnPosMoveBtn.BackColor = Color.Gray;
            ManualSet();
            ManualXPosBtn.Enabled = true;
            ManualYPosBtn.Enabled = true;
            ManualXNegBtn.Enabled = true;
            ManualYNegBtn.Enabled = true;
            ManualZPosBtn.Enabled = true;
            ManualZNegBtn.Enabled = true;
            ManualMode = 1;
            axActUtlType1.SetDevice("M1201", 0);
            FileIO.LogMotion("ManualNoStop M1201=0");

            ClearPicShow();
            //EnableOtherManualBtn();
        }

        private void AutoLaserMarkBtn_Click(object sender, EventArgs e)
        {
            FileIO.LogAction(AutoLaserMarkBtn.Text);

            DialogResult AutoLaserChkResult = MessageBox.Show(this, "確定要激發雷射嗎?", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (AutoLaserChkResult == DialogResult.No)
                return;


            AutoMarkMode = 1; //雷射
            AutoLaserStage = 1; //自動流程第一階

            //if (IsLoadEZM == 0)
            //{
            //    MessageBox.Show("請開啟EZM檔!");
            //    return;
            //}
            if (x_width > 0 && y_height > 0)
            {
                DisableOtherBtn();
                AutoLaserMarkBtn.Enabled = false;
                AutoLaserMarkBtn.BackColor = Color.Gray;
                AutoMsgTxtBox.Text += "------自動雷射銲接模式執行------\r\n";
                AutoReXArea = 0;
                AutoReYArea = 0;

                if (AutoModeStatus == 1)//暫停跳出暫停對話視窗讓user選擇重新由何處開始
                {
                    AutoModeAreaSelect AutoModeAreaSelect = new AutoModeAreaSelect();
                    AutoModeAreaSelect.Owner = this;
                    AutoModeAreaSelect.XAreaTotalCnt = XAreaCnt;
                    AutoModeAreaSelect.YAreaTotalCnt = YAreaCnt;
                    AutoModeAreaSelect.XAreaTotalCnt = XAreaCnt;
                    AutoModeAreaSelect.YAreaTotalCnt = YAreaCnt;
                    AutoModeAreaSelect.ShowDialog();


                }
                // 繪製 M*N示意圖
                InitialMap();
                Thread.Sleep(100);
                //繪圖 + 平台移動 + 雷刻執行 (蛇行移動)
                //使用 thread 才能按鈕中止

                AutoMarkThreadFlag = 1; // Auto 程序 Thred 啟動
                AutoMarkThread = new Thread(new ThreadStart(AutoMark));
                Form.CheckForIllegalCrossThreadCalls = false; // 存取 UI 時需要用,較不安全的寫法,改用委派較佳(EX: UPDATE TXTBOX)

                AutoMarkThread.Start();
            }
            else
                MessageBox.Show(this, "請選擇工單!!");

            AutoLaserStage = 0; //自動流程stage初始化
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void ManualLaserStopBtn_Click(object sender, EventArgs e)
        {
            axActUtlType1.SetDevice("M1205", 0);
            FileIO.LogMotion("ManualLaserStop M1205=0");
            axActUtlType1.SetDevice("M1215", 0);
            FileIO.LogMotion("ManualLaserStop M1215=0");

            if (ManualMarkThreadFlag == 1)
                ManualMarkThread.Abort();
        }

        private void button28_Click(object sender, EventArgs e)
        {
            if (axActUtlType1.GetDevice("M1200", out LaserReset) == 0)
            {
                axActUtlType1.SetDevice("M1200", 1);
            }
        }

        private void button29_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {

            //---------login---------
            //201903 坤坐建議把 LOGIN MARK 掉
            //this.Hide();
            //if ((VP.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            //    &&
            //    (VP.UserAccount != "ERROR" && VP.UserAccount != "")
            //    && (VP.UserPW != "ERROR") && VP.UserPW != "")
            //{

            //    VP.Dispose();
            //----------------讀取生管單路徑---------------------
            List<string> SecList = new List<string>();
            SecList = FileIO.IniDoubleReadSec("D:\\MM_PLC_Setting\\Setting.ini");
            ProdMgrFilePath = FileIO.IniStrReadKeySingle(SecList, "ProdMgrFile", "D:\\MM_PLC_Setting\\Setting.ini");

            //----------------產生起始生管單----------------
            //生管單基本資料
            FileIO.IniProdMgrFile();
            ProgStart = DateTime.Now;
            FileIO.IniProdMgrFile();
            FileIO.SetProdMgrFile("ACCOUNT", VP.UserAccount);
            FileIO.SetProdMgrFile("EQP_ID", "LaserWielding");
            FileIO.SetProdMgrFile("PROGRAM", "銲接自動化人機");
            PordMgrFileStart(ProgramStatus, ProgStart.ToString(), ProdMgrFilePath, ProdAlmMsg);//自動reamk為空

            //-----------------------


            // 檢查GOHOME狀態
            if (XGoHomeStatus == 0 || YGoHomeStatus == 0 ||
                (XGoHomeStatus == 0 && YGoHomeStatus == 0 && ZCurPos <= 0.5 && ZCurPos >= -0.5)) //Z軸 PLC M1127無反應,只好以數字限制
            {
                MessageBox.Show(this, "需要重新歸 Home!!");
                //未歸HOME需強制執行
                DisableOtherBtn();
                AutoGoHomeBtn.Enabled = true;
                AutoGoHomeBtn.BackColor = Color.Green;

            }
            this.Show();

            //------------------------
            //刪除過期之所有log檔案
            DeleteFileThread = new Thread(new ThreadStart(DeleteFile));
            Form.CheckForIllegalCrossThreadCalls = false; // 存取 UI 時需要用,較不安全的寫法,改用委派較佳(EX: UPDATE TXTBOX)
            DeleteFileThread.Start();
            //}
            //else
            //{

            //    VP.Dispose();
            //    this.Close();
            //}

        }

        private void SetLaserParamBtn_Click(object sender, EventArgs e)
        {
            FileIO.LogAction(SetLaserParamBtn.Text);
            string FilePath;
            SaveFileDialog SaveDlg = new SaveFileDialog();
            SaveDlg.Title = "儲存 MM 參數檔";
            SaveDlg.Filter = "ezm (*.ezm)|*.ezm";

            if (SaveDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK && SaveDlg.FileName != "")
            {
                FilePath = SaveDlg.FileName;

                double Speed = Convert.ToDouble(SetMMSpeedTxtBox.Text);
                axMMMark1.SetSpeed("root", Speed);

                double POWER = Convert.ToDouble(SetMMPowerTxtBox.Text);
                axMMMark1.SetPower("root", POWER);

                double Freq = Convert.ToDouble(SetMMFreqTxtBox.Text);
                axMMMark1.SetFrequency("root", Freq);

                double MarkDelay = Convert.ToDouble(SetMMMarkDelayTxtBox.Text);
                MarkDelay = MarkDelay * 1000;
                axMMMark1.SetMarkDelay("root", Convert.ToInt32(MarkDelay));

                double LaserOnDelay = Convert.ToDouble(SetMMLaserOnDelayTxtBox.Text);
                LaserOnDelay = LaserOnDelay * 1000;
                axMMMark1.SetLaserOnDelay("root", Convert.ToInt32(LaserOnDelay));

                double PolyDelay = Convert.ToDouble(SetMMPolyDelayTxtBox.Text);
                PolyDelay = PolyDelay * 1000;
                axMMMark1.SetPolyDelay("root", Convert.ToInt32(PolyDelay));

                double LaserOffDelay = Convert.ToDouble(SetMMLaserOffDelayTxtBox.Text);
                LaserOffDelay = LaserOffDelay * 1000;
                axMMMark1.SetLaserOffDelay("root", Convert.ToInt32(LaserOffDelay));

                double JumpSpeed = Convert.ToDouble(SetMMJumpSpeedTxtBox.Text);
                axMMMark1.SetJumpSpeed("root", JumpSpeed);

                double JumpDelay = Convert.ToDouble(SetMMJumpDelayTxtBox.Text);
                JumpDelay = JumpDelay * 1000;
                axMMMark1.SetJumpDelay("root", Convert.ToInt32(JumpDelay));

                axMMMark1.SaveFile(FilePath);
            }




        }

        private void ManLaserReqBtn_Click(object sender, EventArgs e)
        {
            if (ManLaserReqBtnFlag == 0)
            {
                //axActUtlType1.SetDevice("Y70", 1);
                //axActUtlType1.SetDevice("X36", 1);
                PLCAction.LaserReq();
                ManLaserReqBtnFlag = 1;

            }
            else
            {
                //axActUtlType1.SetDevice("Y70", 0);
                //axActUtlType1.SetDevice("X36", 0);
                ManLaserReqBtnFlag = 0;

            }
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            axActUtlType1.SetDevice("M1215", 0);
            axActUtlType1.SetDevice("M1205", 0);

            int LaserTrigStatus = 0;
            axActUtlType1.SetDevice("M1205", 1);
            while (true)
            {
                axActUtlType1.GetDevice("M1215", out LaserTrigStatus);
                if (LaserTrigStatus == 1)// 雷射開啟狀態ok
                {
                    axMMMark1.SetMarkSelect(1);
                    int temp = axMMMark1.GetMarkSelect();
                    axMMMark1.StartMarking(1);


                    axActUtlType1.SetDevice("M1205", 0);
                    //Console.WriteLine("雷射關閉M1205=0" + LaserTrigStatus);
                    FileIO.LogMotion("雷射關閉M1205=0" + LaserTrigStatus);
                    break;
                }
            }//end while

        }
        private void EnableOtherManualBtn()
        {
            ManualStg1SetRadioBtn.Enabled = true;
            ManualStg2SetRadioBtn.Enabled = true;
            ManualMarkBtn.Visible = true;
            ManualMarkBtn.Enabled = true;
            ManualMarkBtn.BackColor = Color.LightSkyBlue;
            ManualRedLightBtn.Visible = true;
            ManualRedLightBtn.Enabled = true;
            ManualRedLightBtn.BackColor = Color.Brown;

        }
        private void ManualAreaAsnMoveBtn_Click(object sender, EventArgs e)
        {
            //-------------------------------------------
            //找出USER所選對應到setting上哪個區塊
            MaunalAreaIdx = FindManualIdx();

            ClearPicShow();
            Bitmap bmp = new Bitmap(PicShowPicBox.Width, PicShowPicBox.Height);
            //畫出該區示意圖
            bmp = DrawItems.DrawBlock(bmp,
                 w * (IniAreaNumXList[MaunalAreaIdx] - 1) + OffsetW, h * (IniAreaNumYList[MaunalAreaIdx] - 1) + OffsetH,
                 w * IniAreaNumXList[MaunalAreaIdx] + OffsetW, h * (IniAreaNumYList[MaunalAreaIdx] - 1) + OffsetH,
                 w * (IniAreaNumXList[MaunalAreaIdx] - 1) + OffsetW, h * IniAreaNumYList[MaunalAreaIdx] + OffsetH,
                 IniPicFileList[MaunalAreaIdx]);
            //補畫方框
            bmp = DrawEmpthRect(bmp);
            PicShowPicBox.Image = bmp;
            PicShowPicBox.Refresh();

            //帶出所屬 MM FILE

            axMMMark1.LoadFile(IniMMFileList[MaunalAreaIdx]);
            MMfileNameLbl.Text = IniMMFileList[MaunalAreaIdx];

            //-------------------------------------------

            FileIO.LogAction("區塊" + ManualAreaAsnMoveBtn.Text);
            RedLightOff();
            ManualMarkThreadFlag = 1;
            ManualMarkThread = new Thread(new ThreadStart(ManualMark));
            Form.CheckForIllegalCrossThreadCalls = false; // 存取 UI 時需要用,較不安全的寫法,改用委派較佳(EX: UPDATE TXTBOX)
            ManualMarkThread.Start();


        }
        private int FindManualIdx()
        {
            int Idx = 0;
            int XAreaNum = (Convert.ToInt32(XSelectAreaComboBox.Text));
            int YAreaNum = (Convert.ToInt32(YSelectAreaComboBox.Text));

            for (int i = 0; i < IniAreaCnt; i++)
            {
                if (IniAreaNumXList[i] == XAreaNum && IniAreaNumYList[i] == YAreaNum)
                {
                    Idx = i;
                    break;
                }
            }
            return Idx;

        }

        private void ManualMark()
        {
            ThreadAlive = 1; // thread 執行
            RedLightOff(); // 紅光關閉防呆

            //全部卡住
            ManualAreaSelectGrpBox.Enabled = false;
            ManualAsnPosGrpBox.Enabled = false;
            ManualMoveGrpBox.Enabled = false;


            if (ManualGoToAreaRadioBox.Checked == true)
            {
                //平台走位
                PLCAction.AutoStageMove(IniXPointList[MaunalAreaIdx],
                                        IniYPointList[MaunalAreaIdx],
                                        IniZPointList[MaunalAreaIdx],
                                        IniSpeedRateList[MaunalAreaIdx]);


            }
            else if (ManualAsnPosRadioBtn.Checked == true)
            {
                //平台走位須乘上軸向
                double ManualXPos = Convert.ToDouble(ManualXPosTxtBox.Text) * Convert.ToDouble(IniXdirList[0]);
                double ManualYPos = Convert.ToDouble(ManualYPosTxtBox.Text) * Convert.ToDouble(IniYdirList[0]);
                double ManualZPos = Convert.ToDouble(ManualZPosTxtBox.Text) * Convert.ToDouble(IniZdirList[0]);
                PLCAction.AutoStageMove(ManualXPos, ManualYPos, ManualZPos, 100);//預設手動走3000,100%


            }
            MessageBox.Show(this, "已移動至指定位置!");

            //還原顯示
            //全部解開
            ManualAreaSelectGrpBox.Enabled = true;
            ManualAsnPosGrpBox.Enabled = true;
            ManualMoveGrpBox.Enabled = true;

            ManualMarkBtn.Enabled = true;
            ManualMarkBtn.BackColor = Color.LightSkyBlue;
            ManualRedLightBtn.Enabled = true;
            ManualRedLightBtn.BackColor = Color.Brown;


            ThreadAlive = 0; // thread 關閉

        }
        private void ClearPicShow()
        {
            Bitmap bmp = new Bitmap(PicShowPicBox.Width, PicShowPicBox.Height);
            bmp = DrawEmpthRect(bmp);
            PicShowPicBox.Image = bmp;
            PicShowPicBox.Refresh();
        }

        private void ManualAsnPosMoveBtn_Click(object sender, EventArgs e)
        {

            ClearPicShow();

            FileIO.LogAction("指定位置" + ManualAsnPosMoveBtn.Text);
            ManualMarkThreadFlag = 1;
            ManualMarkThread = new Thread(new ThreadStart(ManualMark));
            Form.CheckForIllegalCrossThreadCalls = false; // 存取 UI 時需要用,較不安全的寫法,改用委派較佳(EX: UPDATE TXTBOX)
            RedLightOff();
            ManualMarkThread.Start();

        }

        private void AutoLaserStage2MarkBtn_Click(object sender, EventArgs e)
        {
            if (CloseDoor == 0)
            {
                //-----------------------
                FinishLaserMarking = -1; //代表雷射尚未要出光
                ForDoorProcessing = 1; //代表機台銲接工作中,打開門時需要急停
                //-----------------------
                ProgramStatus = "自動銲接";
                //一進來就清空示意圖框
                Bitmap bmp = new Bitmap(PicShowPicBox.Width, PicShowPicBox.Height);
                PicShowPicBox.Image = bmp;
                bmp = DrawEmpthRect(bmp);
                FileIO.LogAction(AutoLaserStage2MarkBtn.Text);

                DialogResult AutoLaserChkResult;
                // if (ProdCodeTxtBox.Text == "Default")//條碼=default
                if (ProdCodeTxtBox.Text.Length == 0)//條碼=default
                {
                    AutoLaserChkResult =
                        MessageBox.Show(this, "條碼尚未掃描成功,確定要繼續嗎?",
                                        "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (AutoLaserChkResult == DialogResult.No)
                        return;
                }

                AutoLaserChkResult = MessageBox.Show(this, "確定要激發雷射嗎?", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (AutoLaserChkResult == DialogResult.No)
                    return;


                AutoMarkMode = 1; //雷射
                AutoLaserStage = 2; //自動流程第二階

                //if (ProdCodeTxtBox.Text.Length == 0)
                //{
                //    MessageBox.Show(this, "請輸入產品條碼!!");
                //    return;
                //}
                if (IniAreaCnt > 0)
                {
                    DisableOtherBtn();
                    // InitialMap();
                    Thread.Sleep(100);
                    //繪圖 + 平台移動 + 雷刻執行 (蛇行移動)
                    //使用 thread 才能按鈕中止


                    AutoMarkStage2Thread = new Thread(new ThreadStart(AutoMarkStage2));
                    Form.CheckForIllegalCrossThreadCalls = false; // 存取 UI 時需要用,較不安全的寫法,改用委派較佳(EX: UPDATE TXTBOX)

                    AutoMarkStage2Thread.Start();
                }
                else
                    MessageBox.Show(this, "請選擇工單!!");

            }
            else
                MessageBox.Show(this, "機台門未關閉!請重新確認!!");
        }
        private void PordMgrFileStart(String PMProgramStatus,
                                      String PMStartTime,
                                      String PMProdMgrFilePath,
                                      String PMRemark)
        {
            //----------------產生起始生管單----------------
            //生管單基本資料
            FileIO.IniProdMgrFile2();
            String PMProgramStatus2 = "開始" + PMProgramStatus;
            FileIO.SetProdMgrFile("ACCOUNT", VP.UserAccount);
            FileIO.SetProdMgrFile("MODULE", ProdCodeTxtBox.Text);
            FileIO.SetProdMgrFile("ORDER", OrderCodeTxtBox.Text);
            FileIO.SetProdMgrFile("STEP", PMProgramStatus2);
            FileIO.SetProdMgrFile("START_TIME", PMStartTime);
            FileIO.SetProdMgrFile("REMARK", PMRemark);
            FileIO.SetProdMgrFile("STARTEND", "開始");
            FileIO.LogProdMgrFile("", PMProdMgrFilePath);
            //---------------------------------------------------

        }
        private void PordMgrFileEnd(String PMProgramStatus,
                                     String PMEndTime,
                                     String PMRunTime,
                                     String PMProdMgrFilePath,
                                      String PMRemark)
        {
            //----------------產生結束生管單-----------------
            //生管單基本資料
            String PMProgramStatus2 = "結束" + PMProgramStatus;
            FileIO.SetProdMgrFile("STEP", PMProgramStatus2);
            FileIO.SetProdMgrFile("STARTEND", "結束");
            FileIO.SetProdMgrFile("END_TIME", PMEndTime);
            FileIO.SetProdMgrFile("RUN_TIME", PMRunTime); //執行時間(單位秒)
            String PMStatus;
            if (ProdAlmMsg.Length > 0)
            {
                PMStatus = "E";
                // PMRemark = ProdAlmMsg; //ALARM 訊息
            }
            else
            {
                PMStatus = "Y";
                //   PMRemark = null; //執行結果 Y成功,E失敗
            }
            FileIO.SetProdMgrFile("STATUS", PMStatus);
            FileIO.SetProdMgrFile("REMARK", PMRemark);
            FileIO.LogProdMgrFile("", ProdMgrFilePath);
            ProdAlmMsg = null;//寫完log清空alm字串
            //---------------------------------------------------

        }

        private void AutoMarkStage2()
        {
            //----------------產生起始生管單----------------
            //生管單基本資料
            dt1 = DateTime.Now;
            PordMgrFileStart(ProgramStatus, dt1.ToString(), ProdMgrFilePath, ProdAlmMsg);//自動reamk為空
            //---------------------------------------------------

            ThreadAlive = 1; // thread 執行
            AutoMarkStage2ThreadFlag = 1;
            AutoMark();
            AutoLaserStage = 0; //自動流程stage初始化
            ThreadAlive = 0; // thread 關閉
            ForDoorProcessing = 0; //代表機台銲接工作結束,打開門時不需要急停

            //----------------產生結束生管單-----------------
            //生管單基本資料
            TimeSpan ts = DateTime.Now - dt1;
            double TimeDiff = Convert.ToDouble(ts.TotalSeconds.ToString());
            string StrTimeDiff = TimeDiff.ToString("#0.00");

            PordMgrFileEnd(ProgramStatus, DateTime.Now.ToString(),
                            StrTimeDiff, ProdMgrFilePath, ProdAlmMsg);
            //---------------------------------------------------
        }

        private void AutoMMFileSetBtn_Click(object sender, EventArgs e)
        {

        }

        private void AutoRedLightStage2Btn_Click(object sender, EventArgs e)
        {
            //---------------------------
            FinishLaserMarking = -1; //代表雷射尚未要出光可直接abort
            //---------------------------
            ProgramStatus = "自動紅光";
            //一進來就清空示意圖框
            Bitmap bmp = new Bitmap(PicShowPicBox.Width, PicShowPicBox.Height);
            PicShowPicBox.Image = bmp;
            bmp = DrawEmpthRect(bmp);


            FileIO.LogAction(AutoRedLightStage2Btn.Text);

            AutoMarkMode = 0; //紅光
            AutoLaserStage = 2; //自動流程第二階
            //if (ProdCodeTxtBox.Text.Length == 0)
            //{
            //    MessageBox.Show(this, "請輸入產品條碼!!");
            //    return;
            //}
            if (IniAreaCnt > 0)
            {
                DisableOtherBtn();
                Thread.Sleep(100);

                //使用 thread 才能按鈕中止
                AutoMarkStage2Thread = new Thread(new ThreadStart(AutoMarkStage2));
                Form.CheckForIllegalCrossThreadCalls = false; // 存取 UI 時需要用,較不安全的寫法,改用委派較佳(EX: UPDATE TXTBOX)

                AutoMarkStage2Thread.Start();
            }
            else
                MessageBox.Show(this, "請選擇工單!!");



        }


        private int AccountVerify(string Account, string _PWStr)
        {
            int VerifyResult = 10000;
            string SettingFilePath = @"D:\MM_PLC_Setting\Setting.ini";
            string line, substr;
            System.IO.StreamReader sr = new System.IO.StreamReader(SettingFilePath, Encoding.GetEncoding(950));
            sr = System.IO.File.OpenText(SettingFilePath);

            while ((line = sr.ReadLine()) != null)
            {
                substr = line.Substring(line.IndexOf("]") + 1, line.Length - (line.IndexOf("]") + 1));

                if (line.IndexOf("[PE_PW]") >= 0 && Account == "PE" && substr == _PWStr)
                {
                    VerifyResult = 0;
                    break;
                }
                else VerifyResult = 1;
                if (line.IndexOf("[Admin_PW]") >= 0 && Account == "Admin" && substr == _PWStr)
                {
                    VerifyResult = 0;
                    break;
                }
                else VerifyResult = 2;
            }
            return VerifyResult;
        }
        private void AutoPageBtn_Click(object sender, EventArgs e)
        {
            axMMMark1.StopMarking();
            FileIO.LogAction(AutoPageBtn.Text);
            ActionMode = 0;
            AutoLaserStage = 1;
            this.tabPage2.Parent = null;
            this.tabPage6.Parent = null;
            this.tabPage7.Parent = null;
            this.tabPage3.Parent = null;
            this.tabPage8.Parent = null;
            this.tabPage4.Parent = null;
            // this.tabPage1.Parent = this.tabControl1;
            if (Timer4Flag == 1)
                timer4.Dispose();
            if (Timer5Flag == 1)
                timer5.Dispose();

            //一進入自動就要 LOAD 自動的第一階 EZM 圖檔
            if (x_width > 0 && y_height > 0)
            {
                axMMMark1.LoadFile(MMFilePath);
                MMfileNameLbl.Text = MMFilePath;
            }

            else { MessageBox.Show(this, "請選擇工單!!"); }

            InitialMap();
        }

        private void ManualPage_Click(object sender, EventArgs e)
        {
            FileIO.LogAction(ManualPageBtn.Text);
            //一進入自動就要 LOAD 自動的第一階 EZM 圖檔
            if (IniAreaCnt > 0)
            {
                axMMMark1.LoadFile(IniMMFileList[0]);
                MMfileNameLbl.Text = IniMMFileList[0];
            }

            else { MessageBox.Show(this, "請選擇工單!!"); return; }
            //if (ProdCodeTxtBox.Text.Length == 0)
            //{
            //    MessageBox.Show(this, "請輸入產品條碼!!");
            //    return;
            //}

            ActionMode = 1; //補焊
            AutoMarkMode = 0; //非自動流程模式(不可以自動打雷射)

            //-----顯示設定----------
            this.tabPage6.Parent = null;
            this.tabPage7.Parent = null;
            //   this.tabPage1.Parent = null;
            this.tabPage3.Parent = null;
            this.tabPage8.Parent = null;
            this.tabPage4.Parent = null;
            this.tabPage2.Parent = this.tabControl1;
            DisableOtherManualBtn2();
            ManualGoToAreaRadioBox.Checked = true;
            ManualAreaAsnMoveBtn.Enabled = true;
            ManualAreaAsnMoveBtn.BackColor = Color.SteelBlue;
            //-------------------------


            if (Timer4Flag == 1)
                timer4.Dispose();

            ManualMode = -1;

            ClearPicShow();

            ManualStepDistComboBox.Text = PLCAction.ReadPLCData("R1202").ToString(); //當前步進吋動移動距離

            //極限保護機制啟動
            TimerCallback callback5 = new TimerCallback(BoundProtect);
            //每五百毫秒起來一次
            timer5 = new System.Threading.Timer(callback5, null, 0, 1000);

        }
        private void InitialMap()
        {
            _Bitmap = new Bitmap(PicShowPicBox.Width, PicShowPicBox.Height);
            for (int j = 0; j <= YAreaCnt - 1; j++)
                for (int i = 0; i <= XAreaCnt - 1; i++)
                {
                    _Bitmap = DrawItems.FillRectsBySteps(_Bitmap, XAreaCnt, YAreaCnt, i, j, 0, "AUTO");
                }
            PicShowPicBox.Image = _Bitmap;

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
        }

        private void AutoSetMMFile_Click(object sender, EventArgs e)
        {
            FileIO.LogAction(AutoSetMMFile.Text);
            string OpenFilePath;
            OpenFileDialog.InitialDirectory = ".\\";

            OpenFileDialog.Filter = "ezm files (*.ezm)|*.h|All files (*.*)|*.*";
            OpenFileDialog.FilterIndex = 2;
            OpenFileDialog.RestoreDirectory = true;
            DialogResult result = OpenFileDialog.ShowDialog();
            OpenFilePath = OpenFileDialog.FileName;

            if (result == DialogResult.OK && OpenFileDialog.FileName != "")
                AutoSetMMFileTxtBox.Text = OpenFilePath;
        }


        private void AutoPage2Btn_Click(object sender, EventArgs e)
        {
            axMMMark1.StopMarking();
            FileIO.LogAction(AutoPage2Btn.Text);
            ActionMode = 0;
            AutoLaserStage = 2;
            this.tabPage2.Parent = null;
            this.tabPage6.Parent = null;
            this.tabPage7.Parent = null;
            this.tabPage3.Parent = null;
            // this.tabPage1.Parent = null;
            this.tabPage4.Parent = null;
            this.tabPage8.Parent = this.tabControl1;
            if (Timer4Flag == 1)
                timer4.Dispose();
            if (Timer5Flag == 1)
                timer5.Dispose();

            //一進入自動就要 LOAD 自動的第一階 EZM 圖檔
            //if (ProdCodeTxtBox.Text.Length == 0)
            //{
            //    MessageBox.Show(this, "請輸入產品條碼!!");
            //    return;
            //}
            if (IniAreaCnt > 0)
            {
                axMMMark1.LoadFile(IniMMFileList[0]);
                MMfileNameLbl.Text = IniMMFileList[0];
            }
            else { MessageBox.Show(this, "請選擇工單!!"); }


            ClearPicShow();
        }

        private void 開啟工單ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //防呆先關閉紅光
            axMMMark1.StopMarking();

            string OpenFilePath;

            OpenFileDialog = new OpenFileDialog();
            OpenFileDialog.InitialDirectory = ".\\";
            OpenFileDialog.Filter = "ini files (*.ini)|*.ini";
            OpenFileDialog.FilterIndex = 2;
            OpenFileDialog.RestoreDirectory = true;
            DialogResult result = OpenFileDialog.ShowDialog();
            OpenFilePath = OpenFileDialog.FileName;
            //擷取檔名
            RecipeName = FileIO.GetFileName(OpenFilePath);
            RecipeOrderLbl2.Text = RecipeName;

            if (result == DialogResult.OK && OpenFileDialog.FileName != "")
            {

                //ReadSection
                List<string> SecList = new List<string>();
                SecList = FileIO.IniDoubleReadSec(OpenFileDialog.FileName);
                IniAreaCnt = SecList.Count();
                IniXPointList = new List<double>();
                IniYPointList = new List<double>();
                IniZPointList = new List<double>();
                IniSpeedRateList = new List<double>();
                IniMMFileList = new List<string>();
                IniAreaNumList = new List<string>();
                IniAreaNumXList = new List<int>();
                IniAreaNumYList = new List<int>();
                IniPicFileList = new List<string>();


                IniXPointList = FileIO.IniDoubleReadKey(SecList, "XPoint", OpenFileDialog.FileName);
                IniYPointList = FileIO.IniDoubleReadKey(SecList, "YPoint", OpenFileDialog.FileName);
                IniZPointList = FileIO.IniDoubleReadKey(SecList, "ZPoint", OpenFileDialog.FileName);


                //每個位置點均須乘以軸向倍率(調整正負方向)
                for (int i = 0; i < IniXPointList.Count(); i++)
                    IniXPointList[i] = IniXPointList[i] * Convert.ToDouble(IniXdirList[0]);

                for (int i = 0; i < IniYPointList.Count(); i++)
                    IniYPointList[i] = IniYPointList[i] * Convert.ToDouble(IniYdirList[0]);

                for (int i = 0; i < IniZPointList.Count(); i++)
                    IniZPointList[i] = IniZPointList[i] * Convert.ToDouble(IniZdirList[0]);


                IniSpeedRateList = FileIO.IniDoubleReadKey(SecList, "SpeedRate", OpenFileDialog.FileName);
                IniMMFileList = FileIO.IniStrReadKey(SecList, "MMFile", OpenFileDialog.FileName);
                IniPicFileList = FileIO.IniStrReadKey(SecList, "PicFile", OpenFileDialog.FileName);
                IniAreaNumList = FileIO.IniStrReadKey(SecList, "AreaNum", OpenFileDialog.FileName);
                //動畫順序
                IniAreaNumXList = FileIO.IniAreaNumGet(IniAreaNumList, "x");
                IniAreaNumYList = FileIO.IniAreaNumGet(IniAreaNumList, "y");


                //------------------- load 第一個MM檔----------------------------
                axMMMark1.LoadFile(IniMMFileList[0]);
                MMfileNameLbl.Text = IniMMFileList[0];

                //-------------------畫第一張動畫圖-----------------------------
                //動畫前處理參數計算,只要開頭算一次就好

                //抓長寬最多各幾單位,才能分配圖片大小
                int w_cnt = -1, h_cnt = -1;
                for (int i = 0; i < IniAreaCnt; i++)
                {
                    if (IniAreaNumXList[i] > w_cnt) w_cnt = IniAreaNumXList[i];
                    if (IniAreaNumYList[i] > h_cnt) h_cnt = IniAreaNumYList[i];
                }


                //預估最佳化的圖片大小
                double UnitW = PicShowPicBox.Width / w_cnt;
                double UnitH = PicShowPicBox.Height / h_cnt;
                double Unit;
                if (UnitW > UnitH) Unit = UnitH;
                else Unit = UnitW;


                //評估置中offset多少
                OffsetW = 0;
                OffsetH = 0;
                if ((int)(PicShowPicBox.Width - Unit * w_cnt) < 5)
                {
                    double MiddleH = PicShowPicBox.Height / 2;
                    OffsetH = MiddleH - Unit * h_cnt / 2;

                }
                else if ((int)(PicShowPicBox.Height - Unit * h_cnt) < 5)
                {
                    double MiddleW = PicShowPicBox.Width / 2;
                    OffsetW = MiddleW - Unit * w_cnt / 2;

                }

                w = Unit;
                h = Unit;

                //畫空白外框
                Bitmap bmp = new Bitmap(PicShowPicBox.Width, PicShowPicBox.Height);
                PicShowPicBox.Image = bmp;
                bmp = DrawEmpthRect(bmp);
                PicShowPicBox.Image = bmp;
                PicShowPicBox.Refresh();


                //--------生管單初始化
                FileIO.IniProdMgrFile();
                FileIO.SetProdMgrFile("ACCOUNT", UserAccount);
                FileIO.SetProdMgrFile("EQP_ID", "LaserWielding");
                FileIO.SetProdMgrFile("PROGRAM", "銲接自動化人機");
                FileIO.SetProdMgrFile("RECIPE", RecipeName);
                //------------------

            }
        }
        private Bitmap DrawEmpthRect(Bitmap bmp)
        {

            //畫框
            for (int i = 0; i < IniAreaCnt; i++)
                bmp = DrawItems.DrawRect(bmp, w * (IniAreaNumXList[i] - 1) + OffsetW, h * (IniAreaNumYList[i] - 1) + OffsetH, w, h);

            return bmp;
        }

        private void groupBox13_Enter(object sender, EventArgs e)
        {

        }
        private void DisableOtherManualBtn()
        {

            ManualMarkBtn.Visible = true;
            ManualMarkBtn.Enabled = false;
            ManualMarkBtn.BackColor = Color.Gray;
            ManualRedLightBtn.Visible = true;
            ManualRedLightBtn.Enabled = false;
            ManualRedLightBtn.BackColor = Color.Gray;
            //Disable items
            ManualXPosBtn.Enabled = false;
            ManualYPosBtn.Enabled = false;
            ManualXNegBtn.Enabled = false;
            ManualYNegBtn.Enabled = false;
            ManualZPosBtn.Enabled = false;
            ManualZNegBtn.Enabled = false;

        }
        private void DisableOtherManualBtn2()
        {
            //1.自動選取區
            ManualGoToAreaRadioBox.Checked = false;
            XSelectAreaComboBox.Enabled = false;
            YSelectAreaComboBox.Enabled = false;
            ManualAreaAsnMoveBtn.Enabled = false;
            ManualAreaAsnMoveBtn.BackColor = Color.Gray;
            //2.指定座標區
            ManualAsnPosRadioBtn.Checked = false;
            ManualXPosTxtBox.Enabled = false;
            ManualYPosTxtBox.Enabled = false;
            ManualZPosTxtBox.Enabled = false;
            ManualAsnPosMoveBtn.Enabled = false;
            ManualAsnPosMoveBtn.BackColor = Color.Gray;

            //3.吋動與連續移動區
            ManualStepRadioBox.Checked = false;
            ManualStepDistComboBox.Enabled = false;
            ManualNoStopRadioBtn.Checked = false;

            ManualZNegBtn.Enabled = false;
            ManualYPosBtn.Enabled = false;
            ManualXNegBtn.Enabled = false;
            ManualXPosBtn.Enabled = false;
            ManualYNegBtn.Enabled = false;
            ManualZPosBtn.Enabled = false;

            //4.手動雷射與紅光按鈕
            ManualMarkBtn.Enabled = false;
            ManualRedLightBtn.Enabled = false;
            ManualMarkBtn.BackColor = Color.Gray;
            ManualRedLightBtn.BackColor = Color.Gray;

        }
        private void ManualStg1SetRadioBtn_Click(object sender, EventArgs e)
        {
            // 繪製 M*N示意圖
            FileIO.LogAction(ManualStg1SetRadioBtn.Text);

            InitialMap();

            axMMMark1.StopMarking();//預防紅光預覽切換時當機
            if (x_width > 0 && y_height > 0)
            {
                axMMMark1.LoadFile(MMFilePath);
                MMfileNameLbl.Text = MMFilePath;
            }
            else { MessageBox.Show(this, "請選擇工單!!"); return; }

            ManualSpeedSetAllGrpBox.Visible = true;
            ManualAreaSelectGrpBox.Visible = true;
            ManualAsnPosGrpBox.Visible = true;
            ManualMoveGrpBox.Visible = true;

            //可以看到但要先鎖住
            DisableOtherManualBtn();
            //FillRectsBySteps(XAreaCnt, YAreaCnt, 0, 0, 2, "MANUAL");
            _Bitmap = DrawItems.FillRectsBySteps(_Bitmap, XAreaCnt, YAreaCnt, 0, 0, 2, "MANUAL");
            PicShowPicBox.Image = _Bitmap;
            ManualLaserStage = 1; //第一階段

            // 指定 stage1 Z位置
            IniZPt = IniZStage1Pt;

        }

        private void ManualStg2SetRadioBtn_Click(object sender, EventArgs e)
        {
            FileIO.LogAction(ManualStg2SetRadioBtn.Text);
            //第一個default著色還原
            // 繪製 M*N示意圖
            InitialMap();

            axMMMark1.StopMarking();//預防紅光預覽切換時當機
            //一進入自動就要 LOAD 自動的第一階 EZM 圖檔
            if (x_width > 0 && y_height > 0)
            {
                axMMMark1.LoadFile(AutoMMArray[0][1]);
                MMfileNameLbl.Text = AutoMMArray[0][1];
            }
            else { MessageBox.Show(this, "請選擇工單!!"); }

            ManualSpeedSetAllGrpBox.Visible = true;
            ManualAreaSelectGrpBox.Visible = true;
            ManualAsnPosGrpBox.Visible = true;
            ManualMoveGrpBox.Visible = true;

            //可以看到但要先鎖住
            DisableOtherManualBtn();
            //FillRectsBySteps(XAreaCnt, YAreaCnt, 0, 0, 2, "MANUAL");
            _Bitmap = DrawItems.FillRectsBySteps(_Bitmap, XAreaCnt, YAreaCnt, 0, 0, 2, "MANUAL");
            PicShowPicBox.Image = _Bitmap;

            ManualLaserStage = 2; //第二階段
            // 指定 stage2 Z位置
            IniZPt = IniZStage2Pt;
        }

        private void ManualStg1SetRadioBtn_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ManualRedLightBtn_Click(object sender, EventArgs e)
        {
            ProgramStatus = "Manual_RedLight";

            FileIO.LogAction(ManualRedLightBtn.Text);
            int RedLightStatus;
            PLCAction.axActUtlType1.GetDevice("M1206", out RedLightStatus);


            if (RedLightStatus == 0)//開啟紅光
            {
                //----------------產生起始生管單----------------
                //生管單基本資料
                MaunalRedLightStart = DateTime.Now;

                string PMRemark = "Area=(" + XSelectAreaComboBox.Text + "_" + YSelectAreaComboBox.Text + ")";
                PordMgrFileStart(ProgramStatus, MaunalRedLightStart.ToString(), ProdMgrFilePath, PMRemark + "_" + ProdAlmMsg);//自動reamk為空
                //---------------------------------------------------

                PLCAction.axActUtlType1.SetDevice("M1206", 1);
                FileIO.LogMotion("ManualRedLight M1206=1");

                axMMMark1.SetMarkSelect(1);
                axMMMark1.StartMarking(3);
                //axMMMark1.StopMarking();

                ManualRedLightBtn.Text = "紅光關閉";
                ManualRedLightBtn.BackColor = Color.Crimson;
                ManualRedLightBtn.ForeColor = Color.White;
            }

            else //關閉紅光
            {
                PLCAction.axActUtlType1.SetDevice("M1206", 0);
                FileIO.LogMotion("ManualRedLight M1206=0");
                axMMMark1.StopMarking();
                ManualRedLightBtn.Text = "紅光開啟";
                ManualRedLightBtn.BackColor = Color.Brown;
                ManualRedLightBtn.ForeColor = Color.Black;

                //----------------產生結束生管單-----------------

                //生管單基本資料
                TimeSpan ts = DateTime.Now - MaunalRedLightStart;
                double TimeDiff = Convert.ToDouble(ts.TotalSeconds.ToString());
                string StrTimeDiff = TimeDiff.ToString("#0.00");

                PordMgrFileEnd(ProgramStatus, DateTime.Now.ToString(),
                                StrTimeDiff, ProdMgrFilePath, ProdAlmMsg);
                //---------------------------------------------------


            }
            //最後才LOG因為字會變
            FileIO.LogAction(ManualRedLightBtn.Text);



        }

        private void ManualStg2SetRadioBtn_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void RcpMaintainBtn_Click(object sender, EventArgs e)
        {
            ProgramStatus = "Parameter_Maintain";
            FileIO.LogAction(RcpMaintainBtn.Text);
            VerifyPW VerifyPW = new VerifyPW();
            VerifyPW.Owner = this;
            VerifyPW.ShowDialog();

            int Result;
            Result = AccountVerify("PE", PWStr);
            if (PWStr == null || PWStr == "CLOSE") return;
            else if (Result > 0)
            {
                MessageBox.Show(this, "輸入密碼錯誤!");
                return;
            }

            this.tabPage2.Parent = null;
            // this.tabPage1.Parent = null;
            this.tabPage3.Parent = null;
            this.tabPage8.Parent = null;
            this.tabPage6.Parent = null;
            this.tabPage7.Parent = null;
            this.tabPage4.Parent = this.tabControl1;
            if (Timer4Flag == 1)
                timer4.Dispose();


        }

        private void ManualYPosTxtBox_FontChanged(object sender, EventArgs e)
        {

        }

        private void ManLaserInterCntlBtn_Click(object sender, EventArgs e)
        {

        }

        private void ManLaserProStartBtn_Click(object sender, EventArgs e)
        {

        }

        private void ManLaserOnBtn_Click(object sender, EventArgs e)
        {

        }

        private void ManualZPosBtn_Click(object sender, EventArgs e)
        {
            FileIO.LogAction("Z下");
            ManualSet();
            if (ManualMode == 2)
            {
                PLCAction.ManualStep("ZPos");

            }
        }

        private void ManualZPosBtn_MouseDown(object sender, MouseEventArgs e)
        {
            //要記得先連通PLC才能執行
            if (ManualMode == 1)
            {
                ManualSet();
                PLCAction.ManualContinous("ZPos");

            }
        }

        private void ManualZPosBtn_MouseUp(object sender, MouseEventArgs e)
        {
            if (ManualMode == 1)
            {
                ManualSet();
                PLCAction.ManualContinousPause();
            }
        }

        private void ManualZNegBtn_MouseUp(object sender, MouseEventArgs e)
        {
            if (ManualMode == 1)
            {
                ManualSet();
                PLCAction.ManualContinousPause();
            }
        }

        private void ManualZNegBtn_MouseDown(object sender, MouseEventArgs e)
        {
            //要記得先連通PLC才能執行
            if (ManualMode == 1)
            {
                ManualSet();
                PLCAction.ManualContinous("ZNeg");

            }
        }

        private void ManualYSpeedTxtBox_TextChanged_1(object sender, EventArgs e)
        {
            ManualSet();
        }

        private void ManualZSpeedTxtBox_TextChanged(object sender, EventArgs e)
        {
            ManualSet();
        }

        private void ManualZNegBtn_Click(object sender, EventArgs e)
        {
            FileIO.LogAction("Z上");
            ManualSet();
            if (ManualMode == 2)
            {

                PLCAction.ManualStep("ZNeg");

            }
        }

        private void ManualNoStopRadioBtn_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void AutoCal_Click(object sender, EventArgs e)
        {
            axMMMark1.AutoCal(1, 0);
        }

        private void ManualStepDistComboBox_TextChanged(object sender, EventArgs e)
        {
            ManualSet();
        }

        private void ManualStepDistComboBox_TextUpdate(object sender, EventArgs e)
        {
            ManualSet();
        }

        private void axMMMark1_IdentObject(object sender, AxMMMARKLib._DMMMarkEvents_IdentObjectEvent e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void splitContainer4_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }

        private void PicShowPicBox_Resize(object sender, EventArgs e)
        {

        }

        private void PicShowPicBox_SizeChanged(object sender, EventArgs e)
        {
            //-------------------畫第一張動畫圖-----------------------------
            //動畫前處理參數計算,只要開頭算一次就好

            //抓長寬最多各幾單位,才能分配圖片大小
            int w_cnt = -1, h_cnt = -1;
            for (int i = 0; i < IniAreaCnt; i++)
            {
                if (IniAreaNumXList[i] > w_cnt) w_cnt = IniAreaNumXList[i];
                if (IniAreaNumYList[i] > h_cnt) h_cnt = IniAreaNumYList[i];
            }


            //預估最佳化的圖片大小
            double UnitW = PicShowPicBox.Width / w_cnt;
            double UnitH = PicShowPicBox.Height / h_cnt;
            double Unit;
            if (UnitW > UnitH) Unit = UnitH;
            else Unit = UnitW;


            //評估置中offset多少
            OffsetW = 0;
            OffsetH = 0;
            if ((int)(PicShowPicBox.Width - Unit * w_cnt) < 5)
            {
                double MiddleH = PicShowPicBox.Height / 2;
                OffsetH = MiddleH - Unit * h_cnt / 2;

            }
            else if ((int)(PicShowPicBox.Height - Unit * h_cnt) < 5)
            {
                double MiddleW = PicShowPicBox.Width / 2;
                OffsetW = MiddleW - Unit * w_cnt / 2;

            }

            w = Unit;
            h = Unit;

            //畫空白外框
            Bitmap bmp = new Bitmap(PicShowPicBox.Width, PicShowPicBox.Height);
            PicShowPicBox.Image = bmp;
            bmp = DrawEmpthRect(bmp);
            PicShowPicBox.Image = bmp;
            PicShowPicBox.Refresh();
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            //if (LaserReqTestFlag == 0 || LaserReqTestFlag == -1)
            //{

            //    PLCAction.axActUtlType1.SetDevice("M1205", 1);
            //    LaserReqTestFlag = 1;
            //    button1.BackColor = Color.Green;
            //}
            //else if (LaserReqTestFlag == 1)
            //{
            //    PLCAction.axActUtlType1.SetDevice("M1205", 0);
            //    LaserReqTestFlag = 0;
            //    button1.BackColor = Color.SteelBlue;
            //}
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            //if (RedLightReqTestFlag == 0 || RedLightReqTestFlag == -1)
            //{

            //    PLCAction.axActUtlType1.SetDevice("M1206", 1);
            //    RedLightReqTestFlag = 1;
            //    button2.BackColor = Color.Green;
            //}
            //else if (RedLightReqTestFlag == 1)
            //{
            //    PLCAction.axActUtlType1.SetDevice("M1206", 0);
            //    RedLightReqTestFlag = 0;
            //    button2.BackColor = Color.DarkRed;
            //}
        }

        private void tabPage8_Click(object sender, EventArgs e)
        {

        }










    }
}
