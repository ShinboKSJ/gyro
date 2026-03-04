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
using System.Windows.Threading;
using System.Diagnostics;

namespace Gyro_Analyzer.UserControls
{
    /// <summary>
    /// UserControlStopWatch.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UserControlStopWatch : UserControl
    {
        DispatcherTimer dt = new DispatcherTimer();
        Stopwatch stopWatch = new Stopwatch();
        string currentTime = string.Empty;

        bool controlButtonVisible = true;
        int timeTextFontSize = 80;

        public UserControlStopWatch()
        {
            InitializeComponent();

            dt.Tick += new EventHandler(dt_Tick);
            dt.Interval = new TimeSpan(0, 0, 0, 0, 50);
        }

        void dt_Tick(object sender, EventArgs e)
        {
            if (stopWatch.IsRunning)
            {
                TimeSpan ts = stopWatch.Elapsed;

                currentTime = String.Format("{0:000}:{1:0}", ts.Minutes * 60 + ts.Seconds, ts.Milliseconds / 100);
                textBlock_CurrentTime.Text = currentTime;

                ElapsedMilliSeconds = Convert.ToInt32(ts.TotalMilliseconds);
                // 자동 종료 시간 설정 확인 및 종료
                if (_autoStopSeconds != 0)
                {
                    if (ElapsedMilliSeconds > (AutoStopSeconds * 1000))
                        stopWatch.Stop();
                }
            }
        }

        private int _elapsedMilliSeconds;
        public int ElapsedMilliSeconds
        {
            get { return _elapsedMilliSeconds; }
            set
            {
                _elapsedMilliSeconds = value;
            }
        }

        private int _autoStopSeconds;
        public int AutoStopSeconds
        {
            get { return _autoStopSeconds; }
            set
            {
                _autoStopSeconds = value;
            }
        }

        public bool ControlButtonVisible
        {
            get { return controlButtonVisible; }
            set
            {
                if (controlButtonVisible == value)
                    return;
                else
                {
                    controlButtonVisible = value;

                    if (controlButtonVisible)
                    {
                        Grid.SetRowSpan(textBlock_CurrentTime, 1);
                        grid_Control_Button.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        Grid.SetRowSpan(textBlock_CurrentTime, 2);
                        grid_Control_Button.Visibility = System.Windows.Visibility.Hidden;
                    }
                }
            }
        }

        public int TimeTextFontSize
        {
            get { return timeTextFontSize; }
            set
            {
                if (timeTextFontSize == value)
                    return;
                else
                {
                    timeTextFontSize = value;
                    textBlock_CurrentTime.FontSize = timeTextFontSize;
                }
            }
        }

        private void button_Start_Click(object sender, RoutedEventArgs e)
        {
            start();
        }

        private void button_Stop_Click(object sender, RoutedEventArgs e)
        {
            stop();
        }

        private void button_Reset_Click(object sender, RoutedEventArgs e)
        {
            reset();
        }

        public void start()
        {
            if (!stopWatch.IsRunning)
            {
                stopWatch.Start();
                dt.Start();
            }
        }
        public void stop()
        {
            if (stopWatch.IsRunning)
                stopWatch.Stop();
        }

        public void reset()
        {
            stopWatch.Reset();
            currentTime = String.Format("{0:000}:{1:0}", 0, 0);
            textBlock_CurrentTime.Text = currentTime;
        }
    }
}
