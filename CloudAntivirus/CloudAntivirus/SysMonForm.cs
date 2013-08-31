using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace SystemMonitor
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.UserControl
	{
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label labelCpu;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label labelMemP;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label labelMemV;
		private System.Windows.Forms.Label labelDiskR;
		private System.Windows.Forms.Label labelDiskW;
		private System.Windows.Forms.Label labelNetO;
        private System.Windows.Forms.Label labelNetI;
		private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;

		private SystemMonitor.DataBar dataBarCPU;
		private SystemMonitor.DataBar dataBarMemP;
		private SystemMonitor.DataBar dataBarMemV;
		private SystemMonitor.DataChart dataChartDiskR;
		private SystemMonitor.DataChart dataChartDiskW;
		private SystemMonitor.DataChart dataChartNetI;
		private SystemMonitor.DataChart dataChartNetO;
		private SystemMonitor.DataChart dataChartCPU;
		private SystemMonitor.DataChart dataChartMem;

		private ArrayList  _listDiskR = new ArrayList();
		private ArrayList  _listDiskW = new ArrayList();
		private ArrayList  _listNetI = new ArrayList();
		private ArrayList  _listNetO = new ArrayList();

		private ArrayList  _listCPU = new ArrayList();
		private ArrayList  _listMem = new ArrayList();

		private SystemData sd = new SystemData();
		private Size _sizeOrg;
		private Size _size;

		public Form1()
		{
			InitializeComponent();

			_size = Size;
			_sizeOrg = Size;

			UpdateData();
			timer.Interval = 1000;
			timer.Start();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.labelNetI = new System.Windows.Forms.Label();
            this.labelNetO = new System.Windows.Forms.Label();
            this.labelDiskW = new System.Windows.Forms.Label();
            this.labelDiskR = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelMemV = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labelMemP = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.labelCpu = new System.Windows.Forms.Label();
            this.dataBarCPU = new SystemMonitor.DataBar();
            this.dataBarMemP = new SystemMonitor.DataBar();
            this.dataBarMemV = new SystemMonitor.DataBar();
            this.dataChartDiskR = new SystemMonitor.DataChart();
            this.dataChartDiskW = new SystemMonitor.DataChart();
            this.dataChartNetO = new SystemMonitor.DataChart();
            this.dataChartNetI = new SystemMonitor.DataChart();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.dataChartCPU = new SystemMonitor.DataChart();
            this.dataChartMem = new SystemMonitor.DataChart();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelNetI
            // 
            this.labelNetI.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNetI.Location = new System.Drawing.Point(8, 238);
            this.labelNetI.Name = "labelNetI";
            this.labelNetI.Size = new System.Drawing.Size(160, 16);
            this.labelNetI.TabIndex = 18;
            this.labelNetI.Text = "Net I";
            this.labelNetI.Click += new System.EventHandler(this.labelNetI_Click);
            // 
            // labelNetO
            // 
            this.labelNetO.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNetO.Location = new System.Drawing.Point(176, 238);
            this.labelNetO.Name = "labelNetO";
            this.labelNetO.Size = new System.Drawing.Size(160, 16);
            this.labelNetO.TabIndex = 17;
            this.labelNetO.Text = "Net O";
            this.labelNetO.Click += new System.EventHandler(this.labelNetO_Click);
            // 
            // labelDiskW
            // 
            this.labelDiskW.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDiskW.Location = new System.Drawing.Point(176, 190);
            this.labelDiskW.Name = "labelDiskW";
            this.labelDiskW.Size = new System.Drawing.Size(160, 16);
            this.labelDiskW.TabIndex = 16;
            this.labelDiskW.Text = "Disk W";
            this.labelDiskW.Click += new System.EventHandler(this.labelDiskW_Click);
            // 
            // labelDiskR
            // 
            this.labelDiskR.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDiskR.Location = new System.Drawing.Point(8, 190);
            this.labelDiskR.Name = "labelDiskR";
            this.labelDiskR.Size = new System.Drawing.Size(160, 16);
            this.labelDiskR.TabIndex = 15;
            this.labelDiskR.Text = "Disk R";
            this.labelDiskR.Click += new System.EventHandler(this.labelDiskR_Click);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(8, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 16);
            this.label4.TabIndex = 14;
            this.label4.Text = "Mem V";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // labelMemV
            // 
            this.labelMemV.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMemV.Location = new System.Drawing.Point(160, 94);
            this.labelMemV.Name = "labelMemV";
            this.labelMemV.Size = new System.Drawing.Size(184, 16);
            this.labelMemV.TabIndex = 13;
            this.labelMemV.Text = "Mem V";
            this.labelMemV.Click += new System.EventHandler(this.labelMemV_Click);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(8, 118);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 16);
            this.label2.TabIndex = 12;
            this.label2.Text = "Mem P";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // labelMemP
            // 
            this.labelMemP.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMemP.Location = new System.Drawing.Point(160, 118);
            this.labelMemP.Name = "labelMemP";
            this.labelMemP.Size = new System.Drawing.Size(184, 16);
            this.labelMemP.TabIndex = 11;
            this.labelMemP.Text = "Mem P";
            this.labelMemP.Click += new System.EventHandler(this.labelMemP_Click);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 16);
            this.label1.TabIndex = 9;
            this.label1.Text = "CPU";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // labelCpu
            // 
            this.labelCpu.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCpu.Location = new System.Drawing.Point(160, 14);
            this.labelCpu.Name = "labelCpu";
            this.labelCpu.Size = new System.Drawing.Size(184, 16);
            this.labelCpu.TabIndex = 10;
            this.labelCpu.Text = "CPU";
            this.labelCpu.Click += new System.EventHandler(this.labelCpu_Click);
            // 
            // dataBarCPU
            // 
            this.dataBarCPU.BackColor = System.Drawing.Color.Silver;
            this.dataBarCPU.BarColor = System.Drawing.Color.Green;
            this.dataBarCPU.Location = new System.Drawing.Point(56, 14);
            this.dataBarCPU.Name = "dataBarCPU";
            this.dataBarCPU.Size = new System.Drawing.Size(96, 16);
            this.dataBarCPU.TabIndex = 19;
            this.dataBarCPU.Value = 0;
            this.dataBarCPU.Load += new System.EventHandler(this.dataBarCPU_Load);
            // 
            // dataBarMemP
            // 
            this.dataBarMemP.BackColor = System.Drawing.Color.Silver;
            this.dataBarMemP.BarColor = System.Drawing.Color.SlateBlue;
            this.dataBarMemP.Location = new System.Drawing.Point(56, 118);
            this.dataBarMemP.Name = "dataBarMemP";
            this.dataBarMemP.Size = new System.Drawing.Size(96, 16);
            this.dataBarMemP.TabIndex = 20;
            this.dataBarMemP.Value = 0;
            this.dataBarMemP.Load += new System.EventHandler(this.dataBarMemP_Load);
            // 
            // dataBarMemV
            // 
            this.dataBarMemV.BackColor = System.Drawing.Color.Silver;
            this.dataBarMemV.BarColor = System.Drawing.Color.DarkBlue;
            this.dataBarMemV.Location = new System.Drawing.Point(56, 94);
            this.dataBarMemV.Name = "dataBarMemV";
            this.dataBarMemV.Size = new System.Drawing.Size(96, 16);
            this.dataBarMemV.TabIndex = 21;
            this.dataBarMemV.Value = 0;
            this.dataBarMemV.Load += new System.EventHandler(this.dataBarMemV_Load);
            // 
            // dataChartDiskR
            // 
            this.dataChartDiskR.BackColor = System.Drawing.Color.Silver;
            this.dataChartDiskR.ChartType = SystemMonitor.ChartType.Stick;
            this.dataChartDiskR.GridColor = System.Drawing.Color.Gold;
            this.dataChartDiskR.GridPixels = 0;
            this.dataChartDiskR.InitialHeight = 100000;
            this.dataChartDiskR.LineColor = System.Drawing.Color.Blue;
            this.dataChartDiskR.Location = new System.Drawing.Point(8, 206);
            this.dataChartDiskR.Name = "dataChartDiskR";
            this.dataChartDiskR.Size = new System.Drawing.Size(160, 24);
            this.dataChartDiskR.TabIndex = 22;
            this.dataChartDiskR.Load += new System.EventHandler(this.dataChartDiskR_Load);
            // 
            // dataChartDiskW
            // 
            this.dataChartDiskW.BackColor = System.Drawing.Color.Silver;
            this.dataChartDiskW.ChartType = SystemMonitor.ChartType.Stick;
            this.dataChartDiskW.GridColor = System.Drawing.Color.Gold;
            this.dataChartDiskW.GridPixels = 0;
            this.dataChartDiskW.InitialHeight = 100000;
            this.dataChartDiskW.LineColor = System.Drawing.Color.Blue;
            this.dataChartDiskW.Location = new System.Drawing.Point(176, 206);
            this.dataChartDiskW.Name = "dataChartDiskW";
            this.dataChartDiskW.Size = new System.Drawing.Size(160, 24);
            this.dataChartDiskW.TabIndex = 23;
            this.dataChartDiskW.Load += new System.EventHandler(this.dataChartDiskW_Load);
            // 
            // dataChartNetO
            // 
            this.dataChartNetO.BackColor = System.Drawing.Color.Silver;
            this.dataChartNetO.ChartType = SystemMonitor.ChartType.Stick;
            this.dataChartNetO.GridColor = System.Drawing.Color.Yellow;
            this.dataChartNetO.GridPixels = 0;
            this.dataChartNetO.InitialHeight = 1000;
            this.dataChartNetO.LineColor = System.Drawing.Color.DarkBlue;
            this.dataChartNetO.Location = new System.Drawing.Point(176, 254);
            this.dataChartNetO.Name = "dataChartNetO";
            this.dataChartNetO.Size = new System.Drawing.Size(160, 24);
            this.dataChartNetO.TabIndex = 24;
            this.dataChartNetO.Load += new System.EventHandler(this.dataChartNetO_Load);
            // 
            // dataChartNetI
            // 
            this.dataChartNetI.BackColor = System.Drawing.Color.Silver;
            this.dataChartNetI.ChartType = SystemMonitor.ChartType.Stick;
            this.dataChartNetI.GridColor = System.Drawing.Color.Yellow;
            this.dataChartNetI.GridPixels = 0;
            this.dataChartNetI.InitialHeight = 1000;
            this.dataChartNetI.LineColor = System.Drawing.Color.DarkBlue;
            this.dataChartNetI.Location = new System.Drawing.Point(8, 254);
            this.dataChartNetI.Name = "dataChartNetI";
            this.dataChartNetI.Size = new System.Drawing.Size(160, 24);
            this.dataChartNetI.TabIndex = 25;
            this.dataChartNetI.Load += new System.EventHandler(this.dataChartNetI_Load);
            // 
            // timer
            // 
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(8, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(128, 16);
            this.label3.TabIndex = 26;
            this.label3.Text = "CPU Usage History";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // dataChartCPU
            // 
            this.dataChartCPU.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataChartCPU.BackColor = System.Drawing.Color.Silver;
            this.dataChartCPU.ChartType = SystemMonitor.ChartType.Line;
            this.dataChartCPU.Cursor = System.Windows.Forms.Cursors.Default;
            this.dataChartCPU.GridColor = System.Drawing.Color.Yellow;
            this.dataChartCPU.GridPixels = 8;
            this.dataChartCPU.InitialHeight = 100;
            this.dataChartCPU.LineColor = System.Drawing.Color.Green;
            this.dataChartCPU.Location = new System.Drawing.Point(8, 54);
            this.dataChartCPU.Name = "dataChartCPU";
            this.dataChartCPU.Size = new System.Drawing.Size(328, 24);
            this.dataChartCPU.TabIndex = 27;
            this.dataChartCPU.Load += new System.EventHandler(this.dataChartCPU_Load);
            // 
            // dataChartMem
            // 
            this.dataChartMem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataChartMem.BackColor = System.Drawing.Color.Silver;
            this.dataChartMem.ChartType = SystemMonitor.ChartType.Line;
            this.dataChartMem.Cursor = System.Windows.Forms.Cursors.Default;
            this.dataChartMem.GridColor = System.Drawing.Color.Gold;
            this.dataChartMem.GridPixels = 12;
            this.dataChartMem.InitialHeight = 100;
            this.dataChartMem.LineColor = System.Drawing.Color.DarkBlue;
            this.dataChartMem.Location = new System.Drawing.Point(8, 158);
            this.dataChartMem.Name = "dataChartMem";
            this.dataChartMem.Size = new System.Drawing.Size(328, 24);
            this.dataChartMem.TabIndex = 29;
            this.dataChartMem.Load += new System.EventHandler(this.dataChartMem_Load);
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(8, 142);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(144, 16);
            this.label5.TabIndex = 28;
            this.label5.Text = "Memory Usage History";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // Form1
            // 
            this.BackColor = System.Drawing.Color.Orange;
            this.Controls.Add(this.dataChartMem);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.dataChartCPU);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.dataChartNetI);
            this.Controls.Add(this.dataChartNetO);
            this.Controls.Add(this.dataChartDiskW);
            this.Controls.Add(this.dataChartDiskR);
            this.Controls.Add(this.dataBarMemV);
            this.Controls.Add(this.dataBarMemP);
            this.Controls.Add(this.dataBarCPU);
            this.Controls.Add(this.labelNetI);
            this.Controls.Add(this.labelNetO);
            this.Controls.Add(this.labelDiskW);
            this.Controls.Add(this.labelDiskR);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.labelMemV);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelMemP);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelCpu);
            this.Name = "Form1";
            this.Size = new System.Drawing.Size(344, 374);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

		}
		#endregion
        /*
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.EnableVisualStyles();
			Application.Run(new Form1());
		}*/

		private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			timer.Stop();
		}

		private void timer_Tick(object sender, System.EventArgs e)
		{
			UpdateData();
		}

		private void UpdateData()
		{
			string s = sd.GetProcessorData();
			labelCpu.Text = s;
			double d = double.Parse(s.Substring(0, s.IndexOf("%")));
			dataBarCPU.Value = (int)d;
			dataChartCPU.UpdateChart(d);

			s = sd.GetMemoryVData();
			labelMemV.Text = s;
			d = double.Parse(s.Substring(0, s.IndexOf("%")));
			dataBarMemV.Value = (int)d;
			dataChartMem.UpdateChart(d);

			s = sd.GetMemoryPData();
			labelMemP.Text = s;
			dataBarMemP.Value = (int)double.Parse(s.Substring(0, s.IndexOf("%")));

			d = sd.GetDiskData(SystemData.DiskData.Read);
			labelDiskR.Text = "Disk R (" + sd.FormatBytes(d) + "/s)";
			dataChartDiskR.UpdateChart(d);

			d = sd.GetDiskData(SystemData.DiskData.Write); 
			labelDiskW.Text = "Disk W (" + sd.FormatBytes(d) + "/s)";
			dataChartDiskW.UpdateChart(d);

			d = sd.GetNetData(SystemData.NetData.Received);
			labelNetI.Text = "Net I (" + sd.FormatBytes(d) + "/s)";
			dataChartNetI.UpdateChart(d);

			d = sd.GetNetData(SystemData.NetData.Sent);
			labelNetO.Text = "Net O (" + sd.FormatBytes(d) + "/s)";
			dataChartNetO.UpdateChart(d);
		}

		protected override void OnResize(EventArgs e)
		{
			if (0==_sizeOrg.Width || 0== _sizeOrg.Height) return;

			if (Size.Width != _size.Width && Size.Width > _sizeOrg.Width)		
			{
				int xChange = Size.Width - _size.Width;	  // Client window

				// Adjust three bars
				int newWidth = dataBarCPU.Size.Width +xChange;
				int labelStart = labelCpu.Location.X + xChange;

				dataBarCPU.Size = new Size(newWidth, dataBarCPU.Size.Height);
				labelCpu.Location = new Point(labelStart, labelCpu.Location.Y);

				dataBarMemV.Size = new Size(newWidth, dataBarCPU.Size.Height);
				labelMemV.Location = new Point(labelStart, labelMemV.Location.Y);

				dataBarMemP.Size = new Size(newWidth, dataBarCPU.Size.Height);
				labelMemP.Location = new Point(labelStart, labelMemP.Location.Y);
					
				// Resize four charts
				int margin = 8;
				Rectangle rt = this.ClientRectangle;

				newWidth = (rt.Width - 3*margin)/2;
				labelStart = newWidth +2*margin;

				dataChartDiskR.Size = new Size(newWidth, dataChartDiskR.Size.Height);
				labelDiskW.Location = new Point(labelStart, labelDiskW.Location.Y);
				dataChartDiskW.Location = new Point(labelStart, dataChartDiskW.Location.Y);
				dataChartDiskW.Size = new Size(newWidth, dataChartDiskW.Size.Height);

				dataChartNetI.Size = new Size(newWidth, dataChartNetI.Size.Height);
				labelNetO.Location = new Point(labelStart, labelNetO.Location.Y);
				dataChartNetO.Location = new Point(labelStart, dataChartNetO.Location.Y);
				dataChartNetO.Size = new Size(newWidth, dataChartNetO.Size.Height);

				// I use Anchor so not to Resize two usage charts
//				dataChartCPU.Size = new Size(rt.Width - 2*margin, dataChartCPU.Size.Height);
//				dataChartMem.Size = new Size(rt.Width - 2*margin, dataChartMem.Size.Height);

				_size = Size;
			}

			Size = new Size(Size.Width, _sizeOrg.Height);
		}

        private void dataChartDiskW_Load(object sender, EventArgs e)
        {

        }

        private void dataChartNetO_Load(object sender, EventArgs e)
        {

        }

        private void dataChartDiskR_Load(object sender, EventArgs e)
        {

        }

        private void dataChartNetI_Load(object sender, EventArgs e)
        {

        }

        private void dataChartMem_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void dataChartCPU_Load(object sender, EventArgs e)
        {

        }

        private void dataBarCPU_Load(object sender, EventArgs e)
        {

        }

        private void labelNames_Click(object sender, EventArgs e)
        {

        }

        private void labelModel_Click(object sender, EventArgs e)
        {

        }

        private void labelCpu_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void dataBarMemV_Load(object sender, EventArgs e)
        {

        }

        private void dataBarMemP_Load(object sender, EventArgs e)
        {

        }

        private void labelNetI_Click(object sender, EventArgs e)
        {

        }

        private void labelNetO_Click(object sender, EventArgs e)
        {

        }

        private void labelDiskW_Click(object sender, EventArgs e)
        {

        }

        private void labelDiskR_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void labelMemV_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void labelMemP_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

	}
}
