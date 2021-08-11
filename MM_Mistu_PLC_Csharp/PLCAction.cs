using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace MM_Mistu_PLC_Csharp
{
    public partial class PLCAction : Form
    {
        FileIO FileIO = new FileIO();
        public PLCAction()
        {
            InitializeComponent();
        }
        public int PLC_Connect(int iLogicalStationNumber)//LogicalStationNumber for ActUtlType
        {
            int iReturnCode = 0;				//Return code
            
            try
            {
                //Set the value of 'LogicalStationNumber' to the property.
                axActUtlType1.ActLogicalStationNumber = iLogicalStationNumber;

                //The Open method is executed.
                iReturnCode = axActUtlType1.Open();

                //When the Open method is succeeded, make the EventHandler of ActProgType Controle.
            }
            catch (Exception exception)
            {

                MessageBox.Show(exception.Message,
                            Name, MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            return iReturnCode;

        }
        //public double[] ReadPLCDataRandom(string DeviceName, int DataSize)//一次讀多個位置
        //{

        //    short[] DataRead = new short[DataSize];
        //    int ReadValINT32;
        //    double ReadValSingle = 0.0;
        //    try
        //    {
        //        axActUtlType1.ReadDeviceRandom2(DeviceName, DataSize, out DataRead[0]);

        //    }
        //    catch (Exception exception)
        //    {
        //        MessageBox.Show(exception.Message, Name,
        //                         MessageBoxButtons.OK, MessageBoxIcon.Error);


        //    }
        //    //解析 DeviceName 有幾個D,幾個R, 幾個M
        //    Regex rD = new Regex("D");
        //    MatchCollection MCD;
        //    MCD = rD.Matches(DeviceName);

        //    Regex rR = new Regex("R");
        //    MatchCollection MCR;
        //    MCR = rR.Matches(DeviceName);

        //    double[] ReadVal = new double[((MCD.Count + MCR.Count) / 2) + (DataRead.Length - (MCD.Count + MCR.Count))];

        //    //前幾組D/R要一次讀兩個
        //    int k = 0;
        //    for (int i = 0; i < (MCD.Count + MCR.Count); i = i + 2)
        //    {
        //        ReadValINT32 = Short2Int32(DataRead[i], DataRead[i + 1]);
        //        ReadValSingle = Convert.ToDouble(ReadValINT32) / Convert.ToDouble(10000);
        //        ReadVal[k] = ReadValSingle;
        //        k++;
        //    }
        //    //M一次只要讀一個
        //    for (int i = (MCD.Count + MCR.Count); i < DataRead.Length; i++)
        //    {
        //        ReadValSingle = DataRead[i];
        //        ReadVal[k] = ReadValSingle;
        //        k++;
        //    }

        //    return ReadVal;

        //}
        public double[] ReadPLCDataRandom(string DeviceName, int DataSize)//一次讀多個位置
        {

            short[] DataRead = new short[DataSize];
            int ReadValINT32;
            double ReadValSingle = 0.0;
            try
            {axActUtlType1.ReadDeviceRandom2(DeviceName, DataSize, out DataRead[0]);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, Name,
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //解析 DeviceName 有幾個D,幾個R, 幾個M
            Regex rD = new Regex("D");
            MatchCollection MCD;
            MCD = rD.Matches(DeviceName);

            Regex rR = new Regex("R");
            MatchCollection MCR;
            MCR = rR.Matches(DeviceName);

            double[] ReadVal = new double[((MCD.Count + MCR.Count) / 2) + (DataRead.Length - (MCD.Count + MCR.Count))];

            //前幾組D/R要一次讀兩個
           
            //for (int i=0; i< ;i++)
            //{
            //    if()
            
            //}
            string [] SubDevices;
            SubDevices = ParsingDevice(DeviceName);
            int j=0, k = 0;
            for (int i = 0; i < SubDevices.Length; i++ )
            {
                // D or R 要一次讀兩個,兩個一組
               if (SubDevices[i].Substring(0, 1) == "D" || SubDevices[i].Substring(0, 1) == "R")
                {
                    ReadValINT32 = Short2Int32(DataRead[j], DataRead[j + 1]);
                    ReadValSingle = Convert.ToDouble(ReadValINT32) / Convert.ToDouble(10000);
                    ReadVal[k] = ReadValSingle;
                    i++;  // 因為一次讀兩個,i也要跳過增加1
                    k++;
                    j += 2;
                }
                // M 一次讀一個
                else if (SubDevices[i].Substring(0, 1) == "M")
                {
                    ReadValSingle = DataRead[j];
                    ReadVal[k] = ReadValSingle;
                    k++;
                    j++;
                }
            }
            ////前幾組D/R要一次讀兩個
            //for (int i = 0; i < (MCD.Count + MCR.Count); i = i + 2)
            //{
            //    ReadValINT32 = Short2Int32(DataRead[i], DataRead[i + 1]);
            //    ReadValSingle = Convert.ToDouble(ReadValINT32) / Convert.ToDouble(10000);
            //    ReadVal[k] = ReadValSingle;
            //    k++;
            //}
            ////M一次只要讀一個
            //for (int i = (MCD.Count + MCR.Count); i < DataRead.Length; i++)
            //{
            //    ReadValSingle = DataRead[i];
            //    ReadVal[k] = ReadValSingle;
            //    k++;
            //}

            return ReadVal;

        }
       public  string [] ParsingDevice(string _DeviceName)
        {
            Char delimiter = '\n';
            string [] _SubDevices = _DeviceName.Split(delimiter);
            //foreach (var SubDevice in SubDevices)
            //    Console.WriteLine(SubDevice);
            return _SubDevices;
        }
        public int Short2Int32(short argument1, short argument2)
        {
            int ValueInt32;
            byte[] byteArray1 = BitConverter.GetBytes(argument1);
            byte[] byteArray2 = BitConverter.GetBytes(argument2);
            byte[] byteArray3 = new byte[byteArray1.Length + byteArray2.Length];
            Array.Copy(byteArray1, 0, byteArray3, 0, byteArray1.Length);//合併 2 bytes+ 2 bytes
            Array.Copy(byteArray2, 0, byteArray3, byteArray1.Length, byteArray2.Length);

            ValueInt32 = BitConverter.ToInt32(byteArray3, 0);
            return ValueInt32;
        }
        public short[] Int32Short(double argument1, int ratio)
        {
            // input double=>32bits=>4 bytes=>2 short
            int arg1;
            arg1 = Convert.ToInt32(argument1 * ratio);
            short[] ValueInt16 = new short[2];
            byte[] byteArray1 = BitConverter.GetBytes(arg1);
            byte[] byteArray2 = new byte[2];
            byte[] byteArray3 = new byte[2];
            for (int i = 0; i <= 1; i++)
            {
                byteArray2[i] = byteArray1[i];
            }
            for (int i = 0; i <= 1; i++)
            {
                byteArray3[i] = byteArray1[i + 2];
            }
            ValueInt16[0] = BitConverter.ToInt16(byteArray2, 0);
            ValueInt16[1] = BitConverter.ToInt16(byteArray3, 0);
            return ValueInt16;
        }
        public short[] Int32ShortSpeed(double argument1)
        {
            // input double=>32bits=>4 bytes=>2 short
            int arg1;
            arg1 = Convert.ToInt32(argument1 * 100);
            short[] ValueInt16 = new short[2];
            byte[] byteArray1 = BitConverter.GetBytes(arg1);
            byte[] byteArray2 = new byte[2];
            byte[] byteArray3 = new byte[2];
            for (int i = 0; i <= 1; i++)
            {
                byteArray2[i] = byteArray1[i];
            }
            for (int i = 0; i <= 1; i++)
            {
                byteArray3[i] = byteArray1[i + 2];
            }
            ValueInt16[0] = BitConverter.ToInt16(byteArray2, 0);
            ValueInt16[1] = BitConverter.ToInt16(byteArray3, 0);
            return ValueInt16;
        }
        public double ReadPLCData(string DeviceName)
        {
            short[] DataRead = new short[2];
            int ReadValINT32;
            double ReadVal = 0.0;
            try
            {
                axActUtlType1.ReadDeviceBlock2(DeviceName, 2, out DataRead[0]);
                ReadValINT32 = Short2Int32(DataRead[0], DataRead[1]);
                ReadVal = Convert.ToDouble(ReadValINT32) / Convert.ToDouble(10000);
                // ManualMsgTxtBox.Text += Convert.ToString(StepDistVal) + "\r\n";

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, Name,
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);


            }
            return ReadVal;
        }
        public void GoHome()//(double OriX, double OriY, double OriZ)
        {

            // 20210330 宗穎要求改賦歸順序
            //201903 復歸MARK
            //Z軸復歸
            axActUtlType1.SetDevice("M1023", 0);
            axActUtlType1.SetDevice("M1023", 1);
            FileIO.LogMotion("GoHome Z軸復歸 M1023=1");
            while (true)
            {
                double[] _ReadVal = ReadPLCDataRandom("D1020\nD1021\nM1128", 3);


                if (_ReadVal[0] == 300 && //OriZ && 
                    _ReadVal[1] == 0)
                    break;
            }

            //y軸復歸
            axActUtlType1.SetDevice("M1013", 0);
            axActUtlType1.SetDevice("M1013", 1);
            FileIO.LogMotion("GoHome y軸復歸 M1003=1");
            while (true)
            {
                double[] _ReadVal = ReadPLCDataRandom("D1010\nD1011\nM1118", 3);


                if (_ReadVal[0] == 300 && // OriY && 
                    _ReadVal[1] == 0)
                    break;
            }

            //x軸復歸
            axActUtlType1.SetDevice("M1003", 0);
            FileIO.LogMotion("GoHome M1003=0");
            axActUtlType1.SetDevice("M1013", 0);
            FileIO.LogMotion("GoHome M1013=0");
            axActUtlType1.SetDevice("M1003", 1);
            FileIO.LogMotion("GoHome x軸復歸 M1003=1");

            while (true)
            {
                double[] _ReadVal = ReadPLCDataRandom("D1000\nD1001\nM1108", 3);


                if (_ReadVal[0] == 300 &&//OriX && 
                    _ReadVal[1] == 0)
                    break;
            }
            
           



            axActUtlType1.SetDevice("M1003", 0);
            FileIO.LogMotion("GoHome X軸復歸 M1003=0");
            axActUtlType1.SetDevice("M1013", 0);
            FileIO.LogMotion("GoHome Y軸復歸 M1013=0");
            axActUtlType1.SetDevice("M1023", 0);
            FileIO.LogMotion("GoHome Z軸復歸 M1023=0");

            axActUtlType1.SetDevice("M1202", 1);
            FileIO.LogMotion("GoHome M1202=1");
            axActUtlType1.SetDevice("M1202", 0);
            FileIO.LogMotion("GoHome M1202=0");
            axActUtlType1.SetDevice("M1200", 1);
            FileIO.LogMotion("GoHome M1200=1");
            axActUtlType1.SetDevice("M1200", 0);
            FileIO.LogMotion("GoHome M1200=0");

            MessageBox.Show(this, "復歸完成!");
            FileIO.LogMotion( "GoHome 復歸完成");
           
        }
        public void ManualContinous(string ActionPos)
        {
            axActUtlType1.SetDevice("M1201", 0);//連續移動(滑鼠一值按著)

            if (ActionPos=="XPos")
            {
                axActUtlType1.SetDevice("M1001", 1);//X正向
                FileIO.LogMotion("ManualContinous M1001=1");
            }
            else if (ActionPos == "XNag")
            {
                axActUtlType1.SetDevice("M1000", 1);//X反向
                FileIO.LogMotion("ManualContinous M1000=1");
            }
            else if (ActionPos == "YPos")
            {
                axActUtlType1.SetDevice("M1010", 1);//Y正向
                FileIO.LogMotion("ManualContinous M1010=1");
            }
            else if (ActionPos == "YNag")
            {
                axActUtlType1.SetDevice("M1011", 1);//Y反向
                FileIO.LogMotion("ManualContinous M1011=1");
            }
            else if (ActionPos == "ZPos")
            {
                axActUtlType1.SetDevice("M1020", 1);//Z反向
                FileIO.LogMotion("ManualContinous M1020=1");
            }
            else if (ActionPos == "ZNeg")
            {
                axActUtlType1.SetDevice("M1021", 1);//Z反向
                FileIO.LogMotion("ManualContinous M1021=1");
            }
        }

        public void ManualContinousPause()
        {
            //停止
            axActUtlType1.SetDevice("M1001", 0);//X正向
            FileIO.LogMotion("ManualContinousPause M1001=1");
            axActUtlType1.SetDevice("M1000", 0);//X反向
            FileIO.LogMotion("ManualContinousPause M1000=1");
            axActUtlType1.SetDevice("M1010", 0);//Y正向
            FileIO.LogMotion("ManualContinousPause M1010=1");
            axActUtlType1.SetDevice("M1011", 0);//Y反向
            FileIO.LogMotion("ManualContinousPause M1011=1");
            axActUtlType1.SetDevice("M1020", 0);//Z正向
            FileIO.LogMotion("ManualContinousPause M1020=1");
            axActUtlType1.SetDevice("M1021", 0);//Z反向
            FileIO.LogMotion("ManualContinousPause M1021=1");

        }
        public void ManualStep(string ActionPos)
        {
            axActUtlType1.SetDevice("M1201", 1);//連續移動(滑鼠按一次只移動一定距離)
            FileIO.LogMotion("ManualStep M1201=1");
            if (ActionPos == "XPos")
            {
                axActUtlType1.SetDevice("M1001", 1);//X正向
                FileIO.LogMotion("ManualStep M1001=1");
                while (true)
                {
                    double[] _ReadVal = ReadPLCDataRandom("M1101\nM1105", 2);
                    if (_ReadVal[0] ==1 && _ReadVal[1] == 0)
                    break;
                }

                axActUtlType1.SetDevice("M1001", 0);
                FileIO.LogMotion("ManualStep M1001=0");
            }
            else if (ActionPos == "XNag")
            {
                axActUtlType1.SetDevice("M1000", 1);//X反向
                FileIO.LogMotion("ManualStep M1000=1");
                while (true)
                {
                    double[] _ReadVal = ReadPLCDataRandom("M1100\nM1105", 2);
                    if (_ReadVal[0] == 1 && _ReadVal[1] == 0)
                        break;
                }
                axActUtlType1.SetDevice("M1000", 0);
                FileIO.LogMotion("ManualStep M1000=0");
            }
            else if (ActionPos == "YPos")
            {
                axActUtlType1.SetDevice("M1010", 1);//Y正向
                FileIO.LogMotion("ManualStep M1010=1");
                while (true)
                {
                    double[] _ReadVal = ReadPLCDataRandom("M1110\nM1115", 2);
                    if (_ReadVal[0] == 1 && _ReadVal[1] == 0)
                        break;
                }
                axActUtlType1.SetDevice("M1010", 0);
                FileIO.LogMotion("ManualStep M1010=0");
            }
            else if (ActionPos == "YNag")
            {
                axActUtlType1.SetDevice("M1011", 1);//Y反向
                FileIO.LogMotion("ManualStep M1011=1");
                while (true)
                {
                    double[] _ReadVal = ReadPLCDataRandom("M1111\nM1115", 2);
                    if (_ReadVal[0] == 1 && _ReadVal[1] == 0)
                        break;
                }
                axActUtlType1.SetDevice("M1011", 0);
                FileIO.LogMotion("ManualStep M1011=1");
            }
            else if (ActionPos == "ZPos")
            {
                axActUtlType1.SetDevice("M1020", 1);//Z正向
                FileIO.LogMotion("ManualStep M1020=1");
                while (true)
                {
                    double[] _ReadVal = ReadPLCDataRandom("M1120\nM1125", 2);
                    if (_ReadVal[0] == 1 && _ReadVal[1] == 0)
                        break;
                }
                axActUtlType1.SetDevice("M1020", 0);
                FileIO.LogMotion("ManualStep M1020=0");
            }
            else if (ActionPos == "ZNeg")
            {
                axActUtlType1.SetDevice("M1021", 1);//Z正向
                FileIO.LogMotion("ManualStep M1021=1");
                while (true)
                {
                    double[] _ReadVal = ReadPLCDataRandom("M1121\nM1125", 2);
                    if (_ReadVal[0] == 1 && _ReadVal[1] == 0)
                        break;
                }
                axActUtlType1.SetDevice("M1021", 0);
                FileIO.LogMotion("ManualStep M1021=0");
            }
        }
        public int FindMaxNum(List<int> AreaNumList)
        {
            int MaxNum=0;
            for (int i = 0; i < AreaNumList.Count(); i++)
            {
                if (AreaNumList[i] > MaxNum) MaxNum = AreaNumList[i];
            }
                return (MaxNum);
        }
        public void ManualSet(double StepDistVal, double XSpeedVal, double YSpeedVal, double ZSpeedVal, double SpeedRateVal)
        {

            short[] StepDist = new short[2];
            short[] XSpeed = new short[2];
            short[] YSpeed = new short[2];
            short[] ZSpeed = new short[2];
            short[] SpeedRate = new short[2];
            short[] XTarget = new short[2];
            short[] YTarget = new short[2];
            short[] ZTarget = new short[2];


            //if (ManualStepDistComboBox.Text.Length > 0)
            StepDist = Int32Short(StepDistVal, 10000);

            //if (ManualXSpeedTxtBox.Text.Length > 0)
            XSpeed = Int32ShortSpeed(XSpeedVal);
           
            //if (ManualYSpeedTxtBox.Text.Length > 0)
            YSpeed = Int32ShortSpeed(YSpeedVal);

            ZSpeed = Int32ShortSpeed(ZSpeedVal);

            SpeedRate = Int32Short(SpeedRateVal, 10);

            //寫入吋動距離
            try
            { axActUtlType1.WriteDeviceBlock2("R1202", 2, ref StepDist[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }


            //寫入X軸速度
            try
            { axActUtlType1.WriteDeviceBlock2("R1002", 2, ref XSpeed[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }



            //寫入Y軸速度
            try
            { axActUtlType1.WriteDeviceBlock2("R1012", 2, ref YSpeed[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            //寫入Z軸速度
            try
            { axActUtlType1.WriteDeviceBlock2("R1022", 2, ref ZSpeed[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            //寫入速度比例
            try
            { axActUtlType1.WriteDeviceBlock2("R1200", 2, ref SpeedRate[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }


        }
        public void AutoZAxisMove(int i, int j, double IniZPt, string[][] AutoSpeedArray)
        {
            double ZTgtPos1, ZTgtPos2;
            short[] ZTarget = new short[2];
            short[] ZSpeed = new short[2];
            ZTgtPos1 = IniZPt;

            //寫入Z 軸目標位置
            ZTarget = Int32Short(ZTgtPos1, 10000);
            try
            { axActUtlType1.WriteDeviceBlock2("R1020", 2, ref ZTarget[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            ZTgtPos2 = ReadPLCData("R1020");

            //寫入Z軸速度
            for (int k = 0; k < AutoSpeedArray.Length; k++)
            {
                if (AutoSpeedArray[k][0] == "(" + (i + 1) + ',' + (j + 1) + ")") //user介面比實際陣列編號多1
                {
                    ZSpeed = Int32Short(Convert.ToDouble(AutoSpeedArray[k][3]), 100);
                    break;
                }
            }
            //--------------Z移動動作--------------
            axActUtlType1.SetDevice("M1122", 0);//自動模式要用一速移,先設為0避免卡住
            axActUtlType1.SetDevice("M1122", 1);//自動模式要用一速移動


            //Console.WriteLine("Z一速移動開始 M1122=1");
            FileIO.LogMotion("Z一速移動開始 M1122=1");
            while (true)
            {
                double[] _ReadVal = ReadPLCDataRandom("D1020\nD1021\nM1122\nM1125", 4);
                if (_ReadVal[1] == 1 && _ReadVal[2] == 0 && _ReadVal[0] == ZTgtPos2)
                    break;

            }

            axActUtlType1.SetDevice("M1122", 0);//自動模式要用一速移動
            //Console.WriteLine("X結束一速接收命令狀態 M1122=0");
            FileIO.LogMotion("X結束一速接收命令狀態 M1122=0");


        }
        public void AutoStageMove(double IniXPt, double IniYPt, double IniZPt,double SpeedRate)
        {
            double XTgtPos1, XTgtPos2, YTgtPos1 = 0, YTgtPos2;
            short[] XTarget = new short[2];
            short[] YTarget = new short[2];
            short[] XSpeed = new short[2];
            short[] YSpeed = new short[2];

            double ZTgtPos1, ZTgtPos2;
            short[] ZTarget = new short[2];
            short[] ZSpeed = new short[2];

            //================ X ======================
            XTgtPos1 = IniXPt;
            //寫入X 軸目標位置
            XTarget = Int32Short(XTgtPos1, 10000);
            try
            { axActUtlType1.WriteDeviceBlock2("R1000", 2, ref XTarget[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

           // XTgtPos2 = ReadPLCData("R1000");
            //寫入X軸速度-----------------------------------
            XSpeed = Int32Short(3000*SpeedRate/100, 100);
            try
            { axActUtlType1.WriteDeviceBlock2("R1002", 2, ref XSpeed[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            //--------------X移動動作--------------
            axActUtlType1.SetDevice("M1002", 0);//自動模式要用一速移,先設為0避免卡住
            axActUtlType1.SetDevice("M1002", 1);//自動模式要用一速移動
            FileIO.LogMotion("X一速移動開始 M1002=1");
            ////-----------------判斷是否移動至目標位置完成--------------------
            while (true)
            {
                double[] _ReadVal = ReadPLCDataRandom("D1000\nD1001\nM1102\nM1105", 4);
                if (_ReadVal[1] == 1 && _ReadVal[2] == 0 && Math.Abs(_ReadVal[0] - XTgtPos1)<0.001)
                    break;

            }
            axActUtlType1.SetDevice("M1002", 0);//自動模式要用一速移動
            FileIO.LogMotion("X結束一速接收命令狀態 M1002=0");
            //-----------------------------------------------------

            //================ Y ======================
            YTgtPos1 = IniYPt;
            //寫入Y 軸目標位置
            YTarget = Int32Short(YTgtPos1, 10000);
            try
            { axActUtlType1.WriteDeviceBlock2("R1010", 2, ref YTarget[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            //Console.WriteLine("寫入Y 軸目標位置 M1010" + "YTgtPos1=" + YTgtPos1);
            //寫入Y軸速度
            YSpeed = Int32Short(3000 * SpeedRate / 100, 100);
            try
            { axActUtlType1.WriteDeviceBlock2("R1012", 2, ref YSpeed[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            
            //--------------Y移動動作--------------
            axActUtlType1.SetDevice("M1012", 0);//自動模式要用一速移,先設為0避免卡住
            axActUtlType1.SetDevice("M1012", 1);//自動模式要用一速移動
            FileIO.LogMotion("Y一速移動開始 M1012=1");
           
            //-----------------判斷是否移動至目標位置完成--------------------
            while (true)
            {
                double[] _ReadVal = ReadPLCDataRandom("D1010\nD1011\nM1112\nM1115", 4);
                if (_ReadVal[1] == 1 && _ReadVal[2] == 0 &&  Math.Abs(_ReadVal[0]- YTgtPos1)<0.001)
                    break;
             //   Console.WriteLine("Y一速移動,YTgtPos1=" + YTgtPos1 + ", _ReadVal[0]=" + _ReadVal[0]);
            }
            axActUtlType1.SetDevice("M1012", 0);//自動模式要用一速移動
            FileIO.LogMotion("Y結束一速接收命令狀態 M1012=0");
           
            //-----------------------------------------------------
            //201903 復歸MARK
            //================ Z ======================
            ZTgtPos1 = IniZPt;
            //寫入Z 軸目標位置
            ZTarget = Int32Short(ZTgtPos1, 10000);
            ZSpeed = Int32Short(3000 * SpeedRate / 100, 100);
            try
            { axActUtlType1.WriteDeviceBlock2("R1020", 2, ref ZTarget[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }


            //寫入z軸速度
            try
            { axActUtlType1.WriteDeviceBlock2("R1022", 2, ref ZSpeed[0]); }
            catch (Exception exception)
            { MessageBox.Show(exception.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            //--------------Z移動動作--------------
            axActUtlType1.SetDevice("M1022", 0);//自動模式要用一速移,先設為0避免卡住
            axActUtlType1.SetDevice("M1022", 1);//自動模式要用一速移動

            //Console.WriteLine("Z一速移動開始 M1022=1");
            FileIO.LogMotion("Z一速移動開始 M1022=1");
            while (true)
            {
                double[] _ReadVal = ReadPLCDataRandom("D1020\nD1021\nM1122\nM1125", 4);
                if (_ReadVal[1] == 1 && _ReadVal[2] == 0 &&  Math.Abs(_ReadVal[0] - ZTgtPos1)<0.001 )
                    break;

            }

            axActUtlType1.SetDevice("M1022", 0);//自動模式要用一速移動
            FileIO.LogMotion("Z 結束一速接收命令狀態 M1122=0");




        }
        public void LaserReq()
        {
            axActUtlType1.SetDevice("X36", 0);
            axActUtlType1.SetDevice("Y70", 0);
           
        }
        public int PLC_HandShakingRead()
        {
            //系統交握訊號讀回
            double[] _ReadVal1 = ReadPLCDataRandom("M1209", 1);
            int M1209_ = Convert.ToInt32(_ReadVal1[0]);
            return M1209_;
        }

        public void PLC_AllInitial()
        {
            //初始化所有平台動作指令
            //X軸相關
            axActUtlType1.SetDevice("M1000", 0);
            axActUtlType1.SetDevice("M1001", 0);
            axActUtlType1.SetDevice("M1002", 0);
            axActUtlType1.SetDevice("M1003", 0);
            //axActUtlType1.SetDevice("M1100", 0);
            //axActUtlType1.SetDevice("M1101", 0);
            //axActUtlType1.SetDevice("M1102", 0);
            axActUtlType1.SetDevice("M1103", 0);
            //axActUtlType1.SetDevice("M1105", 0);
            //axActUtlType1.SetDevice("M1106", 0);
            //axActUtlType1.SetDevice("M1107", 0);
            //axActUtlType1.SetDevice("M1108", 0);

            //Y軸相關
            axActUtlType1.SetDevice("M1010", 0);
            axActUtlType1.SetDevice("M1011", 0);
            axActUtlType1.SetDevice("M1012", 0);
            axActUtlType1.SetDevice("M1013", 0);
            //axActUtlType1.SetDevice("M1110", 0);
            //axActUtlType1.SetDevice("M1111", 0);
            //axActUtlType1.SetDevice("M1112", 0);
            axActUtlType1.SetDevice("M1113", 0);
            //axActUtlType1.SetDevice("M1115", 0);
            //axActUtlType1.SetDevice("M1116", 0);
            //axActUtlType1.SetDevice("M1117", 0);
            //axActUtlType1.SetDevice("M1118", 0);

            //Z軸相關
            axActUtlType1.SetDevice("M1020", 0);
            axActUtlType1.SetDevice("M1021", 0);
            axActUtlType1.SetDevice("M1022", 0);
            axActUtlType1.SetDevice("M1023", 0);
            //axActUtlType1.SetDevice("M1120", 0);
            //axActUtlType1.SetDevice("M1121", 0);
            //axActUtlType1.SetDevice("M1122", 0);
            axActUtlType1.SetDevice("M1123", 0);
            //axActUtlType1.SetDevice("M1125", 0);
            //axActUtlType1.SetDevice("M1126", 0);
            //axActUtlType1.SetDevice("M1127", 0);
            //axActUtlType1.SetDevice("M1128", 0);
            //reset & stop

            axActUtlType1.SetDevice("M1200", 1);
            axActUtlType1.SetDevice("M1202", 1);

            while (true)
            {
               
                double[] _ReadVal = ReadPLCDataRandom("M1210", 1);
                if (_ReadVal[0] == 1)
                {
                    axActUtlType1.SetDevice("M1200", 0);
                    axActUtlType1.SetDevice("M1202", 0);
                    break;
                }
            }
            
            
        }

        private void PLCAction_Load(object sender, EventArgs e)
        {

        }
    }
}
