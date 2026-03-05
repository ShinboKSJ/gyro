using System;
using System.Collections.Generic;
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

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;


using NationalInstruments.DAQmx;

namespace Gyro_Analyzer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] daqDevList;

        DispatcherTimer timer;

        double[][] readData = new double[4][];
        double[][] readSqrData = new double[4][];

        double[][] totalAvr = new double[4][];
        double[][] totalSqrAvr = new double[4][];
        double[] totalAvrSum = new double[4];
        double[] totalSqrAvrSum = new double[4];
        double[] totalStv = new double[4];
        int frameCounter = 0;


        public MainWindow()
        {
            InitializeComponent();

            initLog();

            BackgroundWorker daqFinder = new BackgroundWorker();
            daqFinder.DoWork += DAQ_Find;
            daqFinder.RunWorkerAsync();

            readData[0] = new double[6000];
            readData[1] = new double[6000];
            readData[2] = new double[6000];
            readData[3] = new double[6000];


            readSqrData[0] = new double[6000];
            readSqrData[1] = new double[6000];
            readSqrData[2] = new double[6000];
            readSqrData[3] = new double[6000];


            totalAvr[0] = new double[60];
            totalAvr[1] = new double[60];
            totalAvr[2] = new double[60];
            totalAvr[3] = new double[60];

            totalSqrAvr[0] = new double[60];
            totalSqrAvr[1] = new double[60];
            totalSqrAvr[2] = new double[60];
            totalSqrAvr[3] = new double[60];

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += new EventHandler(Timer_Tick);
            //timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            double[][] tempValue = new double[4][];
            int i = 0;

            for (i = 0; i < tempValue.Length; i++)
            {
                tempValue[i] = new double[1000];
            }

            Random r = new Random();

            for (i = 0; i < tempValue.Length; i++)
            {
                for (int j = 0; j < tempValue[i].Length; j++)
                {
                    tempValue[i][j] = (r.NextDouble() - 0.5f) * 20;
                }
            }

            graph_All.DataUpdate(tempValue);

            if (LOG_ENABLE)
            {
                UpdateLogDataEvnetArgs args = new UpdateLogDataEvnetArgs(tempValue);
                updateLog.OnUpdateLogData(args);
            }
        }

        private void TextSet(TextBox tB, String s)
        {
            if (tB.Visibility == Visibility.Visible)
            {
                if (!tB.Dispatcher.CheckAccess())
                {
                    tB.Dispatcher.Invoke(
                        DispatcherPriority.Normal,
                        new Action(
                            delegate ()
                            {
                                tB.Text = s;                                
                            }
                        )
                    );
                }
                else
                {
                    tB.Text = s;
                }
            }
        }

        #region DAQ
        // DAQ 식별 및 초기화
        private void DAQ_Find(object sender, DoWorkEventArgs e)
        {
            try
            {
                Device daqDev = DaqSystem.Local.LoadDevice("cDAQ1");
                daqDev.Reset();

                System.Threading.Thread.Sleep(5000);

                daqDevList = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External);

                if (daqDevList.Length != 0)
                {
                    DAQ_Init();
                }
                else
                {
                    MessageBox.Show("DAQ 연결을 확인해 주세요.");
                }
            }
            catch
            {
                MessageBox.Show("DAQ 연결을 확인해 주세요.");
            }
        }

        NationalInstruments.DAQmx.Task myTask;
        NationalInstruments.DAQmx.Task runningTask;
        private AnalogMultiChannelReader analogInReader;

        private void DAQ_Init()
        {
            if (runningTask == null)
            {
                try
                {
                    // Create a new task
                    myTask = new NationalInstruments.DAQmx.Task();

                    // Create a virtual channel
                    myTask.AIChannels.CreateVoltageChannel(daqDevList.GetValue(0).ToString(), "",
                        AITerminalConfiguration.Differential, Convert.ToDouble(-10),
                        Convert.ToDouble(10), AIVoltageUnits.Volts);

                    myTask.AIChannels.CreateVoltageChannel(daqDevList.GetValue(1).ToString(), "",
                        AITerminalConfiguration.Differential, Convert.ToDouble(-10),
                        Convert.ToDouble(10), AIVoltageUnits.Volts);

                    myTask.AIChannels.CreateVoltageChannel(daqDevList.GetValue(2).ToString(), "",
                        AITerminalConfiguration.Differential, Convert.ToDouble(-10),
                        Convert.ToDouble(10), AIVoltageUnits.Volts);

                    myTask.AIChannels.CreateVoltageChannel(daqDevList.GetValue(3).ToString(), "",
                        AITerminalConfiguration.Differential, Convert.ToDouble(-10),
                        Convert.ToDouble(10), AIVoltageUnits.Volts);

                    //myTask.AIChannels.CreateVoltageChannel(daqDevList.GetValue(4).ToString(), "",
                    //    (AITerminalConfiguration)(-1), Convert.ToDouble(-10),
                    //    Convert.ToDouble(10), AIVoltageUnits.Volts);

                    //myTask.AIChannels.CreateVoltageChannel(daqDevList.GetValue(5).ToString(), "",
                    //    (AITerminalConfiguration)(-1), Convert.ToDouble(-10),
                    //    Convert.ToDouble(10), AIVoltageUnits.Volts);

                    // Configure the timing parameters
                    myTask.Timing.ConfigureSampleClock("", Convert.ToDouble(1000),
                        SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, 1000);

                    // Configure the Every N Samples Event
                    myTask.EveryNSamplesReadEventInterval = Convert.ToInt32(50);
                    myTask.EveryNSamplesRead += new EveryNSamplesReadEventHandler(myTask_EveryNSamplesRead);


                    // Verify the Task
                    myTask.Control(TaskAction.Verify);

                    // Prepare the table for Data
                    //InitializeDataTable(myTask.AIChannels, ref dataTable);
                    //acquisitionDataGrid.DataSource = dataTable;

                    runningTask = myTask;
                    analogInReader = new AnalogMultiChannelReader(myTask.Stream);
                    runningTask.SynchronizeCallbacks = true;


                    runningTask.Start();
                }
                catch (DaqException exception)
                {
                    // Display Errors
                    MessageBox.Show(exception.Message);
                    runningTask.Stop();
                    // Dispose of the task
                    runningTask = null;
                    myTask.Dispose();
                    //stopButton.Enabled = false;
                    //startButton.Enabled = true;
                }
            }
        }


        private NationalInstruments.AnalogWaveform<double>[] daq_ReadData;

        BackgroundWorker bgWork;

        private void myTask_EveryNSamplesRead(object sender, EveryNSamplesReadEventArgs e)
        {

            try
            {
                bgWork = new BackgroundWorker();

                // Read the available data from the channels
                daq_ReadData = analogInReader.ReadWaveform(Convert.ToInt32(50));

                Double[][] readData = new Double[daq_ReadData.Length][];

                
                for (int i = 0; i < daq_ReadData.Length; i++)
                {
                    readData[i] = new Double[daq_ReadData[i].Samples.Count];
                }
                for (int i = 0; i < daq_ReadData.Length; i++)
                {
                    for (int j = 0; j < daq_ReadData[i].Samples.Count; j++)
                    {
                        readData[i][j] = daq_ReadData[i].Samples[j].Value;

                        if (avgMode != 0)
                        {
                            if (Math.Abs(readData[i][j]) < 0.2f)
                            {
                                if(avgMode == 1)
                                {
                                    readData[i][j] = readData[i][j] / 3;
                                }
                                else if(avgMode == 2)
                                {
                                    readData[i][j] = readData[i][j] / 10;
                                }
                                else
                                {

                                }
                                
                            }
                        }
                    }
                }

                if (LOG_ENABLE)
                {
                    UpdateLogDataEvnetArgs args = new UpdateLogDataEvnetArgs(readData);
                    updateLog.OnUpdateLogData(args);
                }

                graph_All.DataUpdate(readData);
                
            }
            catch (DaqException exception)
            {
                // Display Errors
                Debug.WriteLine(exception.ToString());
                //MessageBox.Show(exception.Message);
                runningTask.Stop();
                // Dispose of the task
                runningTask = null;
                myTask.Dispose();

                DAQ_Init();
            }
        }
        #endregion

        #region LOG

        DirectoryInfo logDir;
        string logFilePath;
        uint logWritedCount;
        bool isLogFileSaved;

        bool requestLogFileClose = false;

        FileStream logFile;
        StreamWriter logWriter;

        UpdateLog updateLog;

        bool LOG_ENABLE = false;

        private void initLog()
        {
            if (Properties.Settings.Default.LOG_PATH == "")
            {
                FolderPathSetting();
            }
            
            // 저장용 디렉토리 확인 및 생성
            string path = Properties.Settings.Default.LOG_PATH;
            //string path = String.Format("C:\\Log\\{0:yyyyMMdd}", DateTime.Now);
            logDir = new DirectoryInfo(string.Format("{0}\\{1}",path, subFolder[currentMode]));

            try
            {
                if (logDir.Exists == false)
                {
                    logDir.Create();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            updateLog = new UpdateLog();
            updateLog.UpdateLogData += new EventHandler(updateLogData);
        }

        public void updateLogData(object sender, EventArgs e)
        {
            if (e is UpdateLogDataEvnetArgs)
            {
                UpdateLogDataEvnetArgs args = e as UpdateLogDataEvnetArgs;

                WriteLog_PeriodicMessage(args.logData);
            }
        }

        private void WriteLog_PeriodicMessage(double[][] perMsg)
        {
            try
            {
                if (perMsg.Length > 0)
                {
                    //Stopwatch sw = new Stopwatch();
                    //sw.Start();
                    //Debug.WriteLine(string.Format("{0}:{1}", perMsg.Length, perMsg[0].Length));

                    string logData = "";
                    DateTime now = DateTime.Now;
                    //logData += String.Format("{0:00}", System.DateTime.Now.Millisecond); logData += ",";

                    //int count = 0;

                    if (currentMode < 5)
                    {
                        for (int i = 0; i < perMsg[0].Length / 10; i++)
                        {
                            logData = "";
                            logData += String.Format("{0:00}", now.Hour); logData += ",";
                            logData += String.Format("{0:00}", now.Minute); logData += ",";
                            logData += String.Format("{0:00}", now.Second); logData += ",";

                            logData += logWritedCount + i + 1; logData += ","; // 메세지 카운터
                                                                               //count++;
                            
                            for (int j = 0; j < perMsg.Length; j++)
                            {
                                logData += perMsg[j][i * 10]; logData += ",";
                                readData[j][logWritedCount + i] = perMsg[j][i * 10];
                            }

                            if (logWriter.BaseStream != null)
                            {
                                logWriter.WriteLine(logData);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < perMsg[0].Length; i++)
                        {
                            logData = "";
                            logData += String.Format("{0:00}", now.Hour); logData += ",";
                            logData += String.Format("{0:00}", now.Minute); logData += ",";
                            logData += String.Format("{0:00}", now.Second); logData += ",";

                            logData += logWritedCount + i + 1; logData += ","; // 메세지 카운터
                                                                               //count++;

                            for (int j = 0; j < perMsg.Length; j++)
                            {
                                logData += perMsg[j][i]; logData += ",";
                                readData[j][logWritedCount + i] = perMsg[j][i];
                            }

                            if (logWriter.BaseStream != null)
                            {
                                logWriter.WriteLine(logData);
                            }
                        }
                    }

                    if (requestLogFileClose)
                    {
                        logWritedCount = 0;
                        requestLogFileClose = false;
                        CloseLogFile();
                        isLogFileSaved = false;
                        LOG_ENABLE = false;
                    }
                    else
                    {
                        if (currentMode < 5)
                        {
                            logWritedCount += Convert.ToUInt32(perMsg[0].Length / 10);
                        }
                        else
                        {
                            logWritedCount += Convert.ToUInt32(perMsg[0].Length);
                        }

                        // 저장갯수 가변
                        if (logWritedCount >= logCount[currentMode])
                        {
                            logWritedCount = 0;
                            CloseLogFile();
                            CreateNewLogFile();
                        }
                    }

                    //sw.Stop();

                    //TextSet(textBox_ElapsedTime, sw.ElapsedMilliseconds.ToString());
                }
                else
                {

                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            
            
        }



        private void CloseLogFile()
        {

            if (isLogFileSaved)
            {
                string logData = ",,,평균(raw),";

                double[][] average = new double[4][];
                double[] standardDeviation = new double[4];

                int avgCount = logCount[currentMode] / 100;

                average[0] = new double[avgCount];
                average[1] = new double[avgCount];
                average[2] = new double[avgCount];
                average[3] = new double[avgCount];

                logWriter.WriteLine("");

                double[] temp = new double[100];



                for (int i = 0; i < 4; i++)
                {
                    totalAvr[i][frameCounter] = 0; 
                    for (int j = 0; j < readData[i].Length / 100; j++)
                    {
                        Array.Copy(readData[i], j * 100, temp, 0, 100);
                        average[i][j] = temp.Average();

                        // 20220214 KJS
                        //if (avgMode != 0)
                        //{
                        //    if (Math.Abs(average[i][j]) < 0.05f)
                        //    {
                        //        if (avgMode == 1)
                        //            average[i][j] = average[i][j] / 3;
                        //        else if (avgMode == 2)
                        //            average[i][j] = average[i][j] / 4;
                        //        else
                        //        {

                        //        }
                        //    }
                        //}
                        // 20220214 KJS
                        totalAvr[i][frameCounter] += average[i][j];  //..jdj
                    }

                    totalAvr[i][frameCounter] = totalAvr[i][frameCounter]/60.0;  //..jdj

                    //..jdj
                    if (i < 2)
                    {
                        totalSqrAvr[i][frameCounter] = totalAvr[i][frameCounter] * totalAvr[i][frameCounter];
                        //for (int k = 0; k < 6000; k++)
                        //{
                        //    totalSqrAvr[i][frameCounter] += (readData[i][k] * readData[i][k]);
                        //}
                        //totalSqrAvr[i][frameCounter] = totalSqrAvr[i][frameCounter] / 6000.0;
                    }
                }

                for (int j = 0; j < avgCount; j++)
                {
                    logData = ",,평균," + j + ",";

                    for (int i = 0; i < 4; i++)
                    {
                        logData += average[i][j]; logData += ",";
                    }

                    logWriter.WriteLine(logData);
                }

                logData = ",,표준편차,,";

                for (int i = 0; i < 4; i++)
                {
                    standardDeviation[i] = getStDev(average[i], average[i].Average());
                    logData += standardDeviation[i]; logData += ",";
                }

                logWriter.WriteLine(logData);


                //..jdj
                frameCounter++;
                for (int i = 0; i < 2; i++)
                {
                    totalAvrSum[i] = 0;
                    totalSqrAvrSum[i] = 0;
                    for (int j = 0; j < frameCounter; j++)
                    {
                        totalAvrSum[i] += totalAvr[i][j];
                        totalSqrAvrSum[i] += totalSqrAvr[i][j];
                    }
                    totalAvrSum[i] = totalAvrSum[i] / ((double)frameCounter);
                    totalSqrAvrSum[i] = totalSqrAvrSum[i] / ((double)frameCounter);

                    //totalStv[i] = totalSqrAvrSum[i] / ((double)frameCounter) - (totalAvrSum[i] / ((double)frameCounter)) * (totalAvrSum[i] / ((double)frameCounter));
                    totalStv[i] = Math.Sqrt(totalSqrAvrSum[i] - Math.Pow(totalAvrSum[i], 2)); 
                }

                if (frameCounter >= 60)
                {
                    frameCounter = 0;
                }
                ////..jdj



                TextSet(textBox_Average_X, average[0].Average().ToString());
                TextSet(textBox_Average_Y, average[1].Average().ToString());
                TextSet(textBox_Average_R_X, average[2].Average().ToString());
                TextSet(textBox_Average_R_Y, average[3].Average().ToString());

                TextSet(textBox_StandardDeviation_X, standardDeviation[0].ToString());
                TextSet(textBox_StandardDeviation_Y, standardDeviation[1].ToString());

                ////..jdj
                //TextSet(textBox_StandardDeviation_R_X, standardDeviation[2].ToString());
                //TextSet(textBox_StandardDeviation_R_Y, standardDeviation[3].ToString());

                ////..jdj
                TextSet(textBox_StandardDeviation_R_X, totalStv[0].ToString());
                TextSet(textBox_StandardDeviation_R_Y, totalStv[1].ToString());


                logWriter.Close();
                logFile.Close();
            }
        }

        private double getStDev(double[] Array, double Ave)
        {
            double sdSum = Array.Select(val => (val - Ave) * (val - Ave)).Sum();
            return Math.Sqrt(sdSum / (Array.Length - 1));
        }

        private void CreateNewLogFile()
        {
            try
            {
                logFilePath = String.Format("{0}\\{1}.csv", logDir.FullName, DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff"));

                if (File.Exists(logFilePath))
                    File.Delete(logFilePath);

                logFile = new FileStream(logFilePath, FileMode.Create, FileAccess.Write);
                logWriter = new StreamWriter(logFile, System.Text.Encoding.UTF8);

                string[] item = new string[8]{"시","분","초","순번",
                    "X","Y","R_X","R_Y"
                    };

                string dataKind = "";
                for (int i = 0; i < item.Length; i++)
                {
                    dataKind += item[i] + ",";
                }

                logWriter.WriteLine(dataKind);
            }
            finally
            {
                // Clean up
            }
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            frameCounter = 0;

            ////..jdj
            totalStv[0] = 0;
            totalStv[1] = 0;

            TextSet(textBox_StandardDeviation_R_X, totalStv[0].ToString());
            TextSet(textBox_StandardDeviation_R_Y, totalStv[1].ToString());




            if (!LOG_ENABLE && currentMode != -1)
            {
                try
                {
                    readData[0] = new double[logCount[currentMode]];
                    readData[1] = new double[logCount[currentMode]];
                    readData[2] = new double[logCount[currentMode]];
                    readData[3] = new double[logCount[currentMode]];

                    string path = Properties.Settings.Default.LOG_PATH;
                    //string path = String.Format("C:\\Log\\{0:yyyyMMdd}", DateTime.Now);
                    logDir = new DirectoryInfo(string.Format("{0}\\{1}", path, subFolder[currentMode]));

                    try
                    {
                        if (logDir.Exists == false)
                        {
                            logDir.Create();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }

                    CreateNewLogFile();
                    requestLogFileClose = false;
                    isLogFileSaved = true;

                    LOG_ENABLE = true;

                    stopWatch.reset();
                    stopWatch.start();

                }
                catch (Exception ex)
                {
                    ;
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            requestLogFileClose = true;
            //LOG_ENABLE = false;


            stopWatch.stop();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            FolderPathSetting();
        }

        private void FolderPathSetting()
        {
            //System.Windows.MessageBox.Show(string.Format("저장 폴더를 지정합니다."));

            bool? result = false;

            while (result == false)
            {
                System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
                
                if (Properties.Settings.Default.LOG_PATH != "")
                    dlg.SelectedPath = Properties.Settings.Default.LOG_PATH;

                dlg.Description = "저장 폴더 지정";

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (dlg.SelectedPath != "")
                    {

                        Properties.Settings.Default.LOG_PATH = dlg.SelectedPath;
                        Properties.Settings.Default.Save();

                        logDir = new DirectoryInfo(string.Format("{0}\\{1}", dlg.SelectedPath, subFolder[currentMode]));
                    }

                    result = true;
                }
            }
        }

        int currentMode = 0;
        string[] subFolder = new string[6] { "최대입력각속도", "선형오차, 환산계수", "편류안정", "최소감지각속도", "시동시간", "대역폭" };
        int[] logCount = new int[6] { 6000, 3000, 6000, 6000, 500, 10000};

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            currentMode = Convert.ToInt32(rb.Tag);

            requestLogFileClose = true;

            stopWatch.reset();

            textBox_Average_X.Text = "0.00";
            textBox_StandardDeviation_X.Text = "0.00";
            textBox_Average_Y.Text = "0.00";
            textBox_StandardDeviation_Y.Text = "0.00";
            textBox_Average_R_X.Text = "0.00";
            textBox_StandardDeviation_R_X.Text = "0.00";
            textBox_Average_R_Y.Text = "0.00";
            textBox_StandardDeviation_R_Y.Text = "0.00";
        }

        private void textBox_Average_X_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        int avgMode = 0;
        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {

            RadioButton rb = sender as RadioButton;
            avgMode = Convert.ToInt32(rb.Tag);

            graph_All.DrawMode = avgMode;

        }

        private void Rectangle_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (grid_Mode.Visibility == Visibility.Hidden)
                grid_Mode.Visibility = Visibility.Visible;
            else
                grid_Mode.Visibility = Visibility.Hidden;

        }
    }

    public class UpdateLog
    {
        public event EventHandler UpdateLogData;

        public void OnUpdateLogData(UpdateLogDataEvnetArgs e)
        {
            if (UpdateLogData != null)
            {
                UpdateLogData(this.UpdateLogData, e);
            }
        }
    }

    public class UpdateLogDataEvnetArgs : EventArgs
    {
        public double[][] logData { get; private set; }

        public UpdateLogDataEvnetArgs(double[][] logData)
        {
            this.logData = logData;
        }
    }
}
