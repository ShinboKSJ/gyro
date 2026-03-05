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

using Gigasoft.ProEssentials;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;

namespace Gyro_Analyzer.UserControls
{
    /// <summary>
    /// UserGraphControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UserGraphControl : UserControl
    {
        public Gigasoft.ProEssentials.Pesgo Pesgo1;

        DispatcherTimer timer;


        private double[][] _graphData;
        public double[][] GraphData
        {
            get { return _graphData; }
            set
            {
                _graphData = value;
            }
        }

        private int _drawMode;
        public int DrawMode
        {
            get { return _drawMode; }
            set
            {
                _drawMode = value;
            }
        }

        public void DataUpdate(double[][] data)
        {
            for (int i = 0; i < GraphData.Length; i++)
            {
                Array.Copy(GraphData[i], 50, GraphData[i], 0, 9950);
                Array.Copy(data[i], 0, GraphData[i], 9950, 50);
            }
        }

        public UserGraphControl()
        {
            InitializeComponent();

            Pesgo1 = new Gigasoft.ProEssentials.Pesgo();

            //! Chart changes all data each udpate

            Pesgo1.PeData.Subsets = 4;
            Pesgo1.PeData.Points = 10000;

            // Manually configure x and y axes //
            Pesgo1.PeGrid.Configure.ManualScaleControlX = Gigasoft.ProEssentials.Enums.ManualScaleControl.MinMax;
            Pesgo1.PeGrid.Configure.ManualMinX = 0;
            Pesgo1.PeGrid.Configure.ManualMaxX = 10000;
            Pesgo1.PeGrid.Configure.ManualScaleControlY = Gigasoft.ProEssentials.Enums.ManualScaleControl.MinMax;
            Pesgo1.PeGrid.Configure.ManualMinY = -10;
            Pesgo1.PeGrid.Configure.ManualMaxY = 10;

            // Clear out default data //
            Pesgo1.PeData.X[0, 0] = 0;
            Pesgo1.PeData.X[0, 1] = 0;
            Pesgo1.PeData.X[0, 2] = 0;
            Pesgo1.PeData.X[0, 3] = 0;
            //Pesgo1.PeData.X[0, 4] = 0;
            //Pesgo1.PeData.X[0, 5] = 0;
            Pesgo1.PeData.Y[0, 0] = 0;
            Pesgo1.PeData.Y[0, 1] = 0;
            Pesgo1.PeData.Y[0, 2] = 0;
            Pesgo1.PeData.Y[0, 3] = 0;
            //Pesgo1.PeData.Y[0, 4] = 0;
            //Pesgo1.PeData.Y[0, 5] = 0;

            //Set various properties //
            Pesgo1.PeString.MainTitle = "";

            Pesgo1.PeString.SubTitle = "";
            Pesgo1.PeString.SubsetLabels[0] = "X";
            Pesgo1.PeString.SubsetLabels[1] = "Y";
            Pesgo1.PeString.SubsetLabels[2] = "R_X";
            Pesgo1.PeString.SubsetLabels[3] = "R_Y";
            //Pesgo1.PeString.SubsetLabels[4] = "C1";
            //Pesgo1.PeString.SubsetLabels[5] = "C2";

            Pesgo1.PeUserInterface.Dialog.RandomPointsToExport = false;
            Pesgo1.PeUserInterface.Allow.FocalRect = false;
            Pesgo1.PePlot.Allow.Bar = false;
            Pesgo1.PeUserInterface.Allow.Popup = false;

            // This is important of PEpartialresetimage //
            Pesgo1.PeConfigure.CacheBmp = true;
            Pesgo1.PeConfigure.PrepareImages = true;

            // Set Various Other Properties //
            Pesgo1.PeColor.BitmapGradientMode = false;
            Pesgo1.PeColor.QuickStyle = Gigasoft.ProEssentials.Enums.QuickStyle.LightNoBorder;
            //Pesgo1.PeColor.AxisBackColor = System.Drawing.Color.FromArgb(0, 255, 255, 255);
            Pesgo1.PeFont.Fixed = false;

            Pesgo1.PeColor.SubsetColors[0] = System.Drawing.Color.FromArgb(255, 255, 0, 0);
            Pesgo1.PeColor.SubsetColors[1] = System.Drawing.Color.FromArgb(255, 0, 255, 0);
            Pesgo1.PeColor.SubsetColors[2] = System.Drawing.Color.FromArgb(255, 0, 0, 255);
            Pesgo1.PeColor.SubsetColors[3] = System.Drawing.Color.FromArgb(255, 255, 255, 0);
            //Pesgo1.PeColor.SubsetColors[4] = System.Drawing.Color.FromArgb(255, 255, 0, 255);
            //Pesgo1.PeColor.SubsetColors[5] = System.Drawing.Color.FromArgb(255, 0, 255, 255);

            //Pesgo1.PeColor.SubsetShades[1] = Pesgo1.PeColor.SubsetShades[0];
            //Pesgo1.PeColor.SubsetShades[2] = Pesgo1.PeColor.SubsetShades[0];
            //Pesgo1.PeColor.SubsetShades[3] = Pesgo1.PeColor.SubsetShades[0];
            //Pesgo1.PeColor.SubsetShades[4] = Pesgo1.PeColor.SubsetShades[0];
            //Pesgo1.PeColor.SubsetShades[5] = Pesgo1.PeColor.SubsetShades[0];

            Pesgo1.PePlot.SubsetLineTypes[0] = Gigasoft.ProEssentials.Enums.LineType.ThinSolid;
            Pesgo1.PePlot.SubsetLineTypes[1] = Gigasoft.ProEssentials.Enums.LineType.ThinSolid;
            Pesgo1.PePlot.SubsetLineTypes[2] = Gigasoft.ProEssentials.Enums.LineType.ThinSolid;
            Pesgo1.PePlot.SubsetLineTypes[3] = Gigasoft.ProEssentials.Enums.LineType.ThinSolid;
            //Pesgo1.PePlot.SubsetLineTypes[4] = Gigasoft.ProEssentials.Enums.LineType.ThinSolid;
            //Pesgo1.PePlot.SubsetLineTypes[5] = Gigasoft.ProEssentials.Enums.LineType.ThinSolid;

            //Pesgo1.PeColor.GraphForeground = System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor.Black);

            //Pesgo1.PeString.XAxisLabel = "시간(ms)";
            //Pesgo1.PeString.YAxisLabel = "전압(V)";

            // Improves Metafile Export //
            Pesgo1.PeSpecial.DpiX = 96;
            Pesgo1.PeSpecial.DpiY = 96;

            Pesgo1.PeConfigure.RenderEngine = Gigasoft.ProEssentials.Enums.RenderEngine.GdiPlus;

            // Reset image //
            Pesgo1.PeFunction.ReinitializeResetImage();

            windowsFormHost.Child = Pesgo1;

            GraphData = new double[4][];

            for (int i = 0; i < 4; i++)
            {
                GraphData[i] = new double[10000];
            }

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += timer_Tick;

            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            // class scope pre allocated, make sure to use Single (4 byte floats) //
            float[] tmpYData = new float[40000];
            float[] tmpXData = new float[40000];
            Int32 i;

            for (i = 0; i <= 9999; i++)
            {
                tmpXData[i] = i + 1;
                
                //if (Math.Abs(GraphData[0][i]) < 0.05f)
                //{
                //    if (DrawMode == 1)
                //        tmpYData[i] = Convert.ToSingle(GraphData[0][i] / 3);
                //    else if (DrawMode == 2)
                //        tmpYData[i] = Convert.ToSingle(GraphData[0][i] / 4);
                //    else
                //        tmpYData[i] = Convert.ToSingle(GraphData[0][i]);
                //}
                //else
                //{
                //    tmpYData[i] = Convert.ToSingle(GraphData[0][i]);
                //}

                tmpYData[i] = Convert.ToSingle(GraphData[0][i]);

            }

            for (i = 0; i <= 9999; i++)
            {
                tmpXData[i + 10000] = i + 1;

                //if (Math.Abs(GraphData[1][i]) < 0.05f)
                //{
                //    if (DrawMode == 1)
                //        tmpYData[i + 10000] = Convert.ToSingle(GraphData[1][i] / 3);
                //    else if (DrawMode == 2)
                //        tmpYData[i + 10000] = Convert.ToSingle(GraphData[1][i] / 4);
                //    else
                //        tmpYData[i + 10000] = Convert.ToSingle(GraphData[1][i]);
                //}
                //else
                //{
                //    tmpYData[i + 10000] = Convert.ToSingle(GraphData[1][i]);
                //}

                tmpYData[i + 10000] = Convert.ToSingle(GraphData[1][i]);
            }

            for (i = 0; i <= 9999; i++)
            {
                tmpXData[i + 20000] = i + 1;

                //if (Math.Abs(GraphData[2][i]) < 0.05f)
                //{
                //    if (DrawMode == 1)
                //        tmpYData[i + 20000] = Convert.ToSingle(GraphData[2][i] / 3);
                //    else if (DrawMode == 2)
                //        tmpYData[i + 20000] = Convert.ToSingle(GraphData[2][i] / 4);
                //    else
                //        tmpYData[i + 20000] = Convert.ToSingle(GraphData[2][i]);
                //}
                //else
                //{
                //    tmpYData[i + 20000] = Convert.ToSingle(GraphData[2][i]);
                //}

                tmpYData[i + 20000] = Convert.ToSingle(GraphData[2][i]);
            }

            for (i = 0; i <= 9999; i++)
            {
                tmpXData[i + 30000] = i + 1;

                //if (Math.Abs(GraphData[3][i]) < 0.05f)
                //{
                //    if (DrawMode == 1)
                //        tmpYData[i + 30000] = Convert.ToSingle(GraphData[3][i] / 3);
                //    else if (DrawMode == 2)
                //        tmpYData[i + 30000] = Convert.ToSingle(GraphData[3][i] / 4);
                //    else
                //        tmpYData[i + 30000] = Convert.ToSingle(GraphData[3][i]);
                //}
                //else
                //{
                //    tmpYData[i + 30000] = Convert.ToSingle(GraphData[3][i]);
                //}

                tmpYData[i + 30000] = Convert.ToSingle(GraphData[3][i]);
            }

            //for (i = 0; i <= 9999; i++)
            //{
            //    tmpXData[i + 40000] = i + 1;
            //    tmpYData[i + 40000] = Convert.ToSingle(GraphData[4][i * 2]);
            //}

            //for (i = 0; i <= 9999; i++)
            //{
            //    tmpXData[i + 50000] = i + 1;
            //    tmpYData[i + 50000] = Convert.ToSingle(GraphData[5][i * 2]);
            //}

            Gigasoft.ProEssentials.Api.PEvsetW(Pesgo1.PeSpecial.HObject, Gigasoft.ProEssentials.DllProperties.XData, tmpXData, 40000);
            Gigasoft.ProEssentials.Api.PEvsetW(Pesgo1.PeSpecial.HObject, Gigasoft.ProEssentials.DllProperties.YData, tmpYData, 40000);

            //Pesgo1.PeFont.Label.Bold = true;
            Pesgo1.PeColor.Text = System.Drawing.Color.FromArgb(255, 0, 0, 0);
            Pesgo1.PeFunction.ResetImage(0, 0);
            Pesgo1.Invalidate();
        }
    }
}
