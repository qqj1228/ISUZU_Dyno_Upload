namespace ISUZU_Dyno_Upload {
    partial class MainForm {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent() {
            this.grpBoxInfo = new System.Windows.Forms.GroupBox();
            this.GridViewInfo = new System.Windows.Forms.DataGridView();
            this.btnUpload = new System.Windows.Forms.Button();
            this.txtBoxVIN = new System.Windows.Forms.TextBox();
            this.lblManualUpload = new System.Windows.Forms.Label();
            this.grpBoxEnv = new System.Windows.Forms.GroupBox();
            this.GridViewEnv = new System.Windows.Forms.DataGridView();
            this.grpBoxResult = new System.Windows.Forms.GroupBox();
            this.GridViewResult = new System.Windows.Forms.DataGridView();
            this.grpBoxDevice = new System.Windows.Forms.GroupBox();
            this.GridViewDevice = new System.Windows.Forms.DataGridView();
            this.grpBox52 = new System.Windows.Forms.GroupBox();
            this.GridView52 = new System.Windows.Forms.DataGridView();
            this.grpBox53 = new System.Windows.Forms.GroupBox();
            this.GridView53 = new System.Windows.Forms.DataGridView();
            this.grpBox54 = new System.Windows.Forms.GroupBox();
            this.GridView54 = new System.Windows.Forms.DataGridView();
            this.grpBox55 = new System.Windows.Forms.GroupBox();
            this.GridView55 = new System.Windows.Forms.DataGridView();
            this.grpBox56 = new System.Windows.Forms.GroupBox();
            this.GridView56 = new System.Windows.Forms.DataGridView();
            this.lblAutoUpload = new System.Windows.Forms.Label();
            this.grpBox51 = new System.Windows.Forms.GroupBox();
            this.GridView51 = new System.Windows.Forms.DataGridView();
            this.grpBoxInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridViewInfo)).BeginInit();
            this.grpBoxEnv.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridViewEnv)).BeginInit();
            this.grpBoxResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridViewResult)).BeginInit();
            this.grpBoxDevice.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridViewDevice)).BeginInit();
            this.grpBox52.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridView52)).BeginInit();
            this.grpBox53.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridView53)).BeginInit();
            this.grpBox54.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridView54)).BeginInit();
            this.grpBox55.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridView55)).BeginInit();
            this.grpBox56.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridView56)).BeginInit();
            this.grpBox51.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridView51)).BeginInit();
            this.SuspendLayout();
            // 
            // grpBoxInfo
            // 
            this.grpBoxInfo.Controls.Add(this.GridViewInfo);
            this.grpBoxInfo.Location = new System.Drawing.Point(12, 42);
            this.grpBoxInfo.Name = "grpBoxInfo";
            this.grpBoxInfo.Size = new System.Drawing.Size(200, 100);
            this.grpBoxInfo.TabIndex = 4;
            this.grpBoxInfo.TabStop = false;
            this.grpBoxInfo.Text = "车辆信息";
            // 
            // GridViewInfo
            // 
            this.GridViewInfo.AllowUserToAddRows = false;
            this.GridViewInfo.AllowUserToDeleteRows = false;
            this.GridViewInfo.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.GridViewInfo.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.GridViewInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridViewInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridViewInfo.Location = new System.Drawing.Point(3, 17);
            this.GridViewInfo.Name = "GridViewInfo";
            this.GridViewInfo.ReadOnly = true;
            this.GridViewInfo.RowHeadersVisible = false;
            this.GridViewInfo.RowTemplate.Height = 23;
            this.GridViewInfo.Size = new System.Drawing.Size(194, 80);
            this.GridViewInfo.TabIndex = 0;
            // 
            // btnUpload
            // 
            this.btnUpload.Location = new System.Drawing.Point(12, 12);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(75, 23);
            this.btnUpload.TabIndex = 1;
            this.btnUpload.Text = "手动上传";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.BtnUpload_Click);
            // 
            // txtBoxVIN
            // 
            this.txtBoxVIN.Location = new System.Drawing.Point(93, 13);
            this.txtBoxVIN.Name = "txtBoxVIN";
            this.txtBoxVIN.Size = new System.Drawing.Size(150, 21);
            this.txtBoxVIN.TabIndex = 0;
            this.txtBoxVIN.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtBoxVIN_KeyPress);
            // 
            // lblManualUpload
            // 
            this.lblManualUpload.AutoSize = true;
            this.lblManualUpload.Location = new System.Drawing.Point(249, 18);
            this.lblManualUpload.Name = "lblManualUpload";
            this.lblManualUpload.Size = new System.Drawing.Size(77, 12);
            this.lblManualUpload.TabIndex = 2;
            this.lblManualUpload.Text = "手动上传就绪";
            // 
            // grpBoxEnv
            // 
            this.grpBoxEnv.Controls.Add(this.GridViewEnv);
            this.grpBoxEnv.Location = new System.Drawing.Point(12, 149);
            this.grpBoxEnv.Name = "grpBoxEnv";
            this.grpBoxEnv.Size = new System.Drawing.Size(200, 100);
            this.grpBoxEnv.TabIndex = 5;
            this.grpBoxEnv.TabStop = false;
            this.grpBoxEnv.Text = "环境信息";
            // 
            // GridViewEnv
            // 
            this.GridViewEnv.AllowUserToAddRows = false;
            this.GridViewEnv.AllowUserToDeleteRows = false;
            this.GridViewEnv.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.GridViewEnv.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.GridViewEnv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridViewEnv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridViewEnv.Location = new System.Drawing.Point(3, 17);
            this.GridViewEnv.Name = "GridViewEnv";
            this.GridViewEnv.ReadOnly = true;
            this.GridViewEnv.RowHeadersVisible = false;
            this.GridViewEnv.RowTemplate.Height = 23;
            this.GridViewEnv.Size = new System.Drawing.Size(194, 80);
            this.GridViewEnv.TabIndex = 0;
            // 
            // grpBoxResult
            // 
            this.grpBoxResult.Controls.Add(this.GridViewResult);
            this.grpBoxResult.Location = new System.Drawing.Point(12, 255);
            this.grpBoxResult.Name = "grpBoxResult";
            this.grpBoxResult.Size = new System.Drawing.Size(200, 100);
            this.grpBoxResult.TabIndex = 6;
            this.grpBoxResult.TabStop = false;
            this.grpBoxResult.Text = "检测结果";
            // 
            // GridViewResult
            // 
            this.GridViewResult.AllowUserToAddRows = false;
            this.GridViewResult.AllowUserToDeleteRows = false;
            this.GridViewResult.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.GridViewResult.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.GridViewResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridViewResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridViewResult.Location = new System.Drawing.Point(3, 17);
            this.GridViewResult.Name = "GridViewResult";
            this.GridViewResult.ReadOnly = true;
            this.GridViewResult.RowHeadersVisible = false;
            this.GridViewResult.RowTemplate.Height = 23;
            this.GridViewResult.Size = new System.Drawing.Size(194, 80);
            this.GridViewResult.TabIndex = 0;
            // 
            // grpBoxDevice
            // 
            this.grpBoxDevice.Controls.Add(this.GridViewDevice);
            this.grpBoxDevice.Location = new System.Drawing.Point(12, 361);
            this.grpBoxDevice.Name = "grpBoxDevice";
            this.grpBoxDevice.Size = new System.Drawing.Size(200, 100);
            this.grpBoxDevice.TabIndex = 7;
            this.grpBoxDevice.TabStop = false;
            this.grpBoxDevice.Text = "检测设备";
            // 
            // GridViewDevice
            // 
            this.GridViewDevice.AllowUserToAddRows = false;
            this.GridViewDevice.AllowUserToDeleteRows = false;
            this.GridViewDevice.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.GridViewDevice.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.GridViewDevice.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridViewDevice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridViewDevice.Location = new System.Drawing.Point(3, 17);
            this.GridViewDevice.Name = "GridViewDevice";
            this.GridViewDevice.ReadOnly = true;
            this.GridViewDevice.RowHeadersVisible = false;
            this.GridViewDevice.RowTemplate.Height = 23;
            this.GridViewDevice.Size = new System.Drawing.Size(194, 80);
            this.GridViewDevice.TabIndex = 0;
            // 
            // grpBox52
            // 
            this.grpBox52.Controls.Add(this.GridView52);
            this.grpBox52.Location = new System.Drawing.Point(219, 255);
            this.grpBox52.Name = "grpBox52";
            this.grpBox52.Size = new System.Drawing.Size(200, 206);
            this.grpBox52.TabIndex = 9;
            this.grpBox52.TabStop = false;
            this.grpBox52.Text = "2 - 稳态工况法";
            // 
            // GridView52
            // 
            this.GridView52.AllowUserToAddRows = false;
            this.GridView52.AllowUserToDeleteRows = false;
            this.GridView52.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.GridView52.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.GridView52.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridView52.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridView52.Location = new System.Drawing.Point(3, 17);
            this.GridView52.Name = "GridView52";
            this.GridView52.ReadOnly = true;
            this.GridView52.RowHeadersVisible = false;
            this.GridView52.RowTemplate.Height = 23;
            this.GridView52.Size = new System.Drawing.Size(194, 186);
            this.GridView52.TabIndex = 0;
            // 
            // grpBox53
            // 
            this.grpBox53.Controls.Add(this.GridView53);
            this.grpBox53.Location = new System.Drawing.Point(425, 42);
            this.grpBox53.Name = "grpBox53";
            this.grpBox53.Size = new System.Drawing.Size(200, 207);
            this.grpBox53.TabIndex = 10;
            this.grpBox53.TabStop = false;
            this.grpBox53.Text = "3 - 简易瞬态工况法";
            // 
            // GridView53
            // 
            this.GridView53.AllowUserToAddRows = false;
            this.GridView53.AllowUserToDeleteRows = false;
            this.GridView53.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.GridView53.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.GridView53.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridView53.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridView53.Location = new System.Drawing.Point(3, 17);
            this.GridView53.Name = "GridView53";
            this.GridView53.ReadOnly = true;
            this.GridView53.RowHeadersVisible = false;
            this.GridView53.RowTemplate.Height = 23;
            this.GridView53.Size = new System.Drawing.Size(194, 187);
            this.GridView53.TabIndex = 0;
            // 
            // grpBox54
            // 
            this.grpBox54.Controls.Add(this.GridView54);
            this.grpBox54.Location = new System.Drawing.Point(426, 255);
            this.grpBox54.Name = "grpBox54";
            this.grpBox54.Size = new System.Drawing.Size(200, 206);
            this.grpBox54.TabIndex = 11;
            this.grpBox54.TabStop = false;
            this.grpBox54.Text = "4 - 加载减速法";
            // 
            // GridView54
            // 
            this.GridView54.AllowUserToAddRows = false;
            this.GridView54.AllowUserToDeleteRows = false;
            this.GridView54.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.GridView54.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.GridView54.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridView54.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridView54.Location = new System.Drawing.Point(3, 17);
            this.GridView54.Name = "GridView54";
            this.GridView54.ReadOnly = true;
            this.GridView54.RowHeadersVisible = false;
            this.GridView54.RowTemplate.Height = 23;
            this.GridView54.Size = new System.Drawing.Size(194, 186);
            this.GridView54.TabIndex = 0;
            // 
            // grpBox55
            // 
            this.grpBox55.Controls.Add(this.GridView55);
            this.grpBox55.Location = new System.Drawing.Point(631, 42);
            this.grpBox55.Name = "grpBox55";
            this.grpBox55.Size = new System.Drawing.Size(200, 207);
            this.grpBox55.TabIndex = 12;
            this.grpBox55.TabStop = false;
            this.grpBox55.Text = "6 - 自由加速法";
            // 
            // GridView55
            // 
            this.GridView55.AllowUserToAddRows = false;
            this.GridView55.AllowUserToDeleteRows = false;
            this.GridView55.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.GridView55.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.GridView55.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridView55.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridView55.Location = new System.Drawing.Point(3, 17);
            this.GridView55.Name = "GridView55";
            this.GridView55.ReadOnly = true;
            this.GridView55.RowHeadersVisible = false;
            this.GridView55.RowTemplate.Height = 23;
            this.GridView55.Size = new System.Drawing.Size(194, 187);
            this.GridView55.TabIndex = 0;
            // 
            // grpBox56
            // 
            this.grpBox56.Controls.Add(this.GridView56);
            this.grpBox56.Location = new System.Drawing.Point(631, 255);
            this.grpBox56.Name = "grpBox56";
            this.grpBox56.Size = new System.Drawing.Size(200, 206);
            this.grpBox56.TabIndex = 13;
            this.grpBox56.TabStop = false;
            this.grpBox56.Text = "8 - 瞬态工况法";
            // 
            // GridView56
            // 
            this.GridView56.AllowUserToAddRows = false;
            this.GridView56.AllowUserToDeleteRows = false;
            this.GridView56.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.GridView56.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.GridView56.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridView56.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridView56.Location = new System.Drawing.Point(3, 17);
            this.GridView56.Name = "GridView56";
            this.GridView56.ReadOnly = true;
            this.GridView56.RowHeadersVisible = false;
            this.GridView56.RowTemplate.Height = 23;
            this.GridView56.Size = new System.Drawing.Size(194, 186);
            this.GridView56.TabIndex = 0;
            // 
            // lblAutoUpload
            // 
            this.lblAutoUpload.AutoSize = true;
            this.lblAutoUpload.Location = new System.Drawing.Point(543, 18);
            this.lblAutoUpload.Name = "lblAutoUpload";
            this.lblAutoUpload.Size = new System.Drawing.Size(77, 12);
            this.lblAutoUpload.TabIndex = 3;
            this.lblAutoUpload.Text = "自动上传就绪";
            // 
            // grpBox51
            // 
            this.grpBox51.Controls.Add(this.GridView51);
            this.grpBox51.Location = new System.Drawing.Point(219, 42);
            this.grpBox51.Name = "grpBox51";
            this.grpBox51.Size = new System.Drawing.Size(200, 207);
            this.grpBox51.TabIndex = 8;
            this.grpBox51.TabStop = false;
            this.grpBox51.Text = "1 - 双怠速法";
            // 
            // GridView51
            // 
            this.GridView51.AllowUserToAddRows = false;
            this.GridView51.AllowUserToDeleteRows = false;
            this.GridView51.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.GridView51.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.GridView51.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridView51.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridView51.Location = new System.Drawing.Point(3, 17);
            this.GridView51.Name = "GridView51";
            this.GridView51.ReadOnly = true;
            this.GridView51.RowHeadersVisible = false;
            this.GridView51.RowTemplate.Height = 23;
            this.GridView51.Size = new System.Drawing.Size(194, 187);
            this.GridView51.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(844, 481);
            this.Controls.Add(this.lblAutoUpload);
            this.Controls.Add(this.grpBox56);
            this.Controls.Add(this.grpBox55);
            this.Controls.Add(this.grpBox54);
            this.Controls.Add(this.grpBox53);
            this.Controls.Add(this.grpBox52);
            this.Controls.Add(this.grpBox51);
            this.Controls.Add(this.grpBoxDevice);
            this.Controls.Add(this.grpBoxResult);
            this.Controls.Add(this.grpBoxEnv);
            this.Controls.Add(this.lblManualUpload);
            this.Controls.Add(this.txtBoxVIN);
            this.Controls.Add(this.btnUpload);
            this.Controls.Add(this.grpBoxInfo);
            this.Name = "MainForm";
            this.Text = "ISUZU_Dyno_Upload";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.grpBoxInfo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridViewInfo)).EndInit();
            this.grpBoxEnv.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridViewEnv)).EndInit();
            this.grpBoxResult.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridViewResult)).EndInit();
            this.grpBoxDevice.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridViewDevice)).EndInit();
            this.grpBox52.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridView52)).EndInit();
            this.grpBox53.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridView53)).EndInit();
            this.grpBox54.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridView54)).EndInit();
            this.grpBox55.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridView55)).EndInit();
            this.grpBox56.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridView56)).EndInit();
            this.grpBox51.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridView51)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpBoxInfo;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.TextBox txtBoxVIN;
        private System.Windows.Forms.Label lblManualUpload;
        private System.Windows.Forms.GroupBox grpBoxEnv;
        private System.Windows.Forms.GroupBox grpBoxResult;
        private System.Windows.Forms.GroupBox grpBoxDevice;
        private System.Windows.Forms.GroupBox grpBox52;
        private System.Windows.Forms.GroupBox grpBox53;
        private System.Windows.Forms.GroupBox grpBox54;
        private System.Windows.Forms.GroupBox grpBox55;
        private System.Windows.Forms.GroupBox grpBox56;
        private System.Windows.Forms.Label lblAutoUpload;
        private System.Windows.Forms.DataGridView GridViewInfo;
        private System.Windows.Forms.DataGridView GridViewEnv;
        private System.Windows.Forms.DataGridView GridViewResult;
        private System.Windows.Forms.DataGridView GridViewDevice;
        private System.Windows.Forms.DataGridView GridView52;
        private System.Windows.Forms.DataGridView GridView53;
        private System.Windows.Forms.DataGridView GridView54;
        private System.Windows.Forms.DataGridView GridView55;
        private System.Windows.Forms.DataGridView GridView56;
        private System.Windows.Forms.GroupBox grpBox51;
        private System.Windows.Forms.DataGridView GridView51;
    }
}

