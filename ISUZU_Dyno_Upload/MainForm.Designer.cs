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
            this.datViewInfo = new System.Windows.Forms.DataGridView();
            this.btnUpload = new System.Windows.Forms.Button();
            this.txtBoxVIN = new System.Windows.Forms.TextBox();
            this.lblManualUpload = new System.Windows.Forms.Label();
            this.grpBoxEnv = new System.Windows.Forms.GroupBox();
            this.datViewEnv = new System.Windows.Forms.DataGridView();
            this.grpBoxResult = new System.Windows.Forms.GroupBox();
            this.datViewResult = new System.Windows.Forms.DataGridView();
            this.grpBoxDevice = new System.Windows.Forms.GroupBox();
            this.datViewDevice = new System.Windows.Forms.DataGridView();
            this.grpBox52 = new System.Windows.Forms.GroupBox();
            this.datView52 = new System.Windows.Forms.DataGridView();
            this.grpBox53 = new System.Windows.Forms.GroupBox();
            this.datView53 = new System.Windows.Forms.DataGridView();
            this.grpBox54 = new System.Windows.Forms.GroupBox();
            this.datView54 = new System.Windows.Forms.DataGridView();
            this.grpBox55 = new System.Windows.Forms.GroupBox();
            this.datView55 = new System.Windows.Forms.DataGridView();
            this.grpBox56 = new System.Windows.Forms.GroupBox();
            this.datView56 = new System.Windows.Forms.DataGridView();
            this.lblAutoUpload = new System.Windows.Forms.Label();
            this.grpBox51 = new System.Windows.Forms.GroupBox();
            this.datView51 = new System.Windows.Forms.DataGridView();
            this.btnShowData = new System.Windows.Forms.Button();
            this.grpBoxInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.datViewInfo)).BeginInit();
            this.grpBoxEnv.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.datViewEnv)).BeginInit();
            this.grpBoxResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.datViewResult)).BeginInit();
            this.grpBoxDevice.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.datViewDevice)).BeginInit();
            this.grpBox52.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.datView52)).BeginInit();
            this.grpBox53.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.datView53)).BeginInit();
            this.grpBox54.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.datView54)).BeginInit();
            this.grpBox55.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.datView55)).BeginInit();
            this.grpBox56.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.datView56)).BeginInit();
            this.grpBox51.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.datView51)).BeginInit();
            this.SuspendLayout();
            // 
            // grpBoxInfo
            // 
            this.grpBoxInfo.Controls.Add(this.datViewInfo);
            this.grpBoxInfo.Location = new System.Drawing.Point(12, 42);
            this.grpBoxInfo.Name = "grpBoxInfo";
            this.grpBoxInfo.Size = new System.Drawing.Size(200, 100);
            this.grpBoxInfo.TabIndex = 0;
            this.grpBoxInfo.TabStop = false;
            this.grpBoxInfo.Text = "车辆信息";
            // 
            // datViewInfo
            // 
            this.datViewInfo.AllowUserToAddRows = false;
            this.datViewInfo.AllowUserToDeleteRows = false;
            this.datViewInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datViewInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datViewInfo.Location = new System.Drawing.Point(3, 17);
            this.datViewInfo.Name = "datViewInfo";
            this.datViewInfo.ReadOnly = true;
            this.datViewInfo.RowTemplate.Height = 23;
            this.datViewInfo.Size = new System.Drawing.Size(194, 80);
            this.datViewInfo.TabIndex = 0;
            // 
            // btnUpload
            // 
            this.btnUpload.Location = new System.Drawing.Point(96, 13);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(75, 23);
            this.btnUpload.TabIndex = 1;
            this.btnUpload.Text = "手动重传";
            this.btnUpload.UseVisualStyleBackColor = true;
            // 
            // txtBoxVIN
            // 
            this.txtBoxVIN.Location = new System.Drawing.Point(177, 14);
            this.txtBoxVIN.Name = "txtBoxVIN";
            this.txtBoxVIN.Size = new System.Drawing.Size(150, 21);
            this.txtBoxVIN.TabIndex = 2;
            // 
            // lblManualUpload
            // 
            this.lblManualUpload.AutoSize = true;
            this.lblManualUpload.Location = new System.Drawing.Point(333, 19);
            this.lblManualUpload.Name = "lblManualUpload";
            this.lblManualUpload.Size = new System.Drawing.Size(77, 12);
            this.lblManualUpload.TabIndex = 3;
            this.lblManualUpload.Text = "手动上传就绪";
            // 
            // grpBoxEnv
            // 
            this.grpBoxEnv.Controls.Add(this.datViewEnv);
            this.grpBoxEnv.Location = new System.Drawing.Point(12, 149);
            this.grpBoxEnv.Name = "grpBoxEnv";
            this.grpBoxEnv.Size = new System.Drawing.Size(200, 100);
            this.grpBoxEnv.TabIndex = 4;
            this.grpBoxEnv.TabStop = false;
            this.grpBoxEnv.Text = "环境信息";
            // 
            // datViewEnv
            // 
            this.datViewEnv.AllowUserToAddRows = false;
            this.datViewEnv.AllowUserToDeleteRows = false;
            this.datViewEnv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datViewEnv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datViewEnv.Location = new System.Drawing.Point(3, 17);
            this.datViewEnv.Name = "datViewEnv";
            this.datViewEnv.ReadOnly = true;
            this.datViewEnv.RowTemplate.Height = 23;
            this.datViewEnv.Size = new System.Drawing.Size(194, 80);
            this.datViewEnv.TabIndex = 0;
            // 
            // grpBoxResult
            // 
            this.grpBoxResult.Controls.Add(this.datViewResult);
            this.grpBoxResult.Location = new System.Drawing.Point(12, 255);
            this.grpBoxResult.Name = "grpBoxResult";
            this.grpBoxResult.Size = new System.Drawing.Size(200, 100);
            this.grpBoxResult.TabIndex = 5;
            this.grpBoxResult.TabStop = false;
            this.grpBoxResult.Text = "检测结果";
            // 
            // datViewResult
            // 
            this.datViewResult.AllowUserToAddRows = false;
            this.datViewResult.AllowUserToDeleteRows = false;
            this.datViewResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datViewResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datViewResult.Location = new System.Drawing.Point(3, 17);
            this.datViewResult.Name = "datViewResult";
            this.datViewResult.ReadOnly = true;
            this.datViewResult.RowTemplate.Height = 23;
            this.datViewResult.Size = new System.Drawing.Size(194, 80);
            this.datViewResult.TabIndex = 0;
            // 
            // grpBoxDevice
            // 
            this.grpBoxDevice.Controls.Add(this.datViewDevice);
            this.grpBoxDevice.Location = new System.Drawing.Point(12, 361);
            this.grpBoxDevice.Name = "grpBoxDevice";
            this.grpBoxDevice.Size = new System.Drawing.Size(200, 100);
            this.grpBoxDevice.TabIndex = 6;
            this.grpBoxDevice.TabStop = false;
            this.grpBoxDevice.Text = "检测设备";
            // 
            // datViewDevice
            // 
            this.datViewDevice.AllowUserToAddRows = false;
            this.datViewDevice.AllowUserToDeleteRows = false;
            this.datViewDevice.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datViewDevice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datViewDevice.Location = new System.Drawing.Point(3, 17);
            this.datViewDevice.Name = "datViewDevice";
            this.datViewDevice.ReadOnly = true;
            this.datViewDevice.RowTemplate.Height = 23;
            this.datViewDevice.Size = new System.Drawing.Size(194, 80);
            this.datViewDevice.TabIndex = 0;
            // 
            // grpBox52
            // 
            this.grpBox52.Controls.Add(this.datView52);
            this.grpBox52.Location = new System.Drawing.Point(219, 255);
            this.grpBox52.Name = "grpBox52";
            this.grpBox52.Size = new System.Drawing.Size(200, 206);
            this.grpBox52.TabIndex = 8;
            this.grpBox52.TabStop = false;
            this.grpBox52.Text = "稳态工况法";
            // 
            // datView52
            // 
            this.datView52.AllowUserToAddRows = false;
            this.datView52.AllowUserToDeleteRows = false;
            this.datView52.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datView52.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datView52.Location = new System.Drawing.Point(3, 17);
            this.datView52.Name = "datView52";
            this.datView52.ReadOnly = true;
            this.datView52.RowTemplate.Height = 23;
            this.datView52.Size = new System.Drawing.Size(194, 186);
            this.datView52.TabIndex = 0;
            // 
            // grpBox53
            // 
            this.grpBox53.Controls.Add(this.datView53);
            this.grpBox53.Location = new System.Drawing.Point(425, 42);
            this.grpBox53.Name = "grpBox53";
            this.grpBox53.Size = new System.Drawing.Size(200, 207);
            this.grpBox53.TabIndex = 9;
            this.grpBox53.TabStop = false;
            this.grpBox53.Text = "简易瞬态工况法";
            // 
            // datView53
            // 
            this.datView53.AllowUserToAddRows = false;
            this.datView53.AllowUserToDeleteRows = false;
            this.datView53.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datView53.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datView53.Location = new System.Drawing.Point(3, 17);
            this.datView53.Name = "datView53";
            this.datView53.ReadOnly = true;
            this.datView53.RowTemplate.Height = 23;
            this.datView53.Size = new System.Drawing.Size(194, 187);
            this.datView53.TabIndex = 0;
            // 
            // grpBox54
            // 
            this.grpBox54.Controls.Add(this.datView54);
            this.grpBox54.Location = new System.Drawing.Point(426, 255);
            this.grpBox54.Name = "grpBox54";
            this.grpBox54.Size = new System.Drawing.Size(200, 206);
            this.grpBox54.TabIndex = 10;
            this.grpBox54.TabStop = false;
            this.grpBox54.Text = "加载减速法";
            // 
            // datView54
            // 
            this.datView54.AllowUserToAddRows = false;
            this.datView54.AllowUserToDeleteRows = false;
            this.datView54.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datView54.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datView54.Location = new System.Drawing.Point(3, 17);
            this.datView54.Name = "datView54";
            this.datView54.ReadOnly = true;
            this.datView54.RowTemplate.Height = 23;
            this.datView54.Size = new System.Drawing.Size(194, 186);
            this.datView54.TabIndex = 0;
            // 
            // grpBox55
            // 
            this.grpBox55.Controls.Add(this.datView55);
            this.grpBox55.Location = new System.Drawing.Point(631, 42);
            this.grpBox55.Name = "grpBox55";
            this.grpBox55.Size = new System.Drawing.Size(200, 207);
            this.grpBox55.TabIndex = 11;
            this.grpBox55.TabStop = false;
            this.grpBox55.Text = "自由加速法";
            // 
            // datView55
            // 
            this.datView55.AllowUserToAddRows = false;
            this.datView55.AllowUserToDeleteRows = false;
            this.datView55.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datView55.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datView55.Location = new System.Drawing.Point(3, 17);
            this.datView55.Name = "datView55";
            this.datView55.ReadOnly = true;
            this.datView55.RowTemplate.Height = 23;
            this.datView55.Size = new System.Drawing.Size(194, 187);
            this.datView55.TabIndex = 0;
            // 
            // grpBox56
            // 
            this.grpBox56.Controls.Add(this.datView56);
            this.grpBox56.Location = new System.Drawing.Point(631, 255);
            this.grpBox56.Name = "grpBox56";
            this.grpBox56.Size = new System.Drawing.Size(200, 206);
            this.grpBox56.TabIndex = 12;
            this.grpBox56.TabStop = false;
            this.grpBox56.Text = "瞬态工况法";
            // 
            // datView56
            // 
            this.datView56.AllowUserToAddRows = false;
            this.datView56.AllowUserToDeleteRows = false;
            this.datView56.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datView56.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datView56.Location = new System.Drawing.Point(3, 17);
            this.datView56.Name = "datView56";
            this.datView56.ReadOnly = true;
            this.datView56.RowTemplate.Height = 23;
            this.datView56.Size = new System.Drawing.Size(194, 186);
            this.datView56.TabIndex = 0;
            // 
            // lblAutoUpload
            // 
            this.lblAutoUpload.AutoSize = true;
            this.lblAutoUpload.Location = new System.Drawing.Point(423, 18);
            this.lblAutoUpload.Name = "lblAutoUpload";
            this.lblAutoUpload.Size = new System.Drawing.Size(77, 12);
            this.lblAutoUpload.TabIndex = 13;
            this.lblAutoUpload.Text = "自动上传就绪";
            // 
            // grpBox51
            // 
            this.grpBox51.Controls.Add(this.datView51);
            this.grpBox51.Location = new System.Drawing.Point(219, 42);
            this.grpBox51.Name = "grpBox51";
            this.grpBox51.Size = new System.Drawing.Size(200, 207);
            this.grpBox51.TabIndex = 7;
            this.grpBox51.TabStop = false;
            this.grpBox51.Text = "双怠速法";
            // 
            // datView51
            // 
            this.datView51.AllowUserToAddRows = false;
            this.datView51.AllowUserToDeleteRows = false;
            this.datView51.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datView51.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datView51.Location = new System.Drawing.Point(3, 17);
            this.datView51.Name = "datView51";
            this.datView51.ReadOnly = true;
            this.datView51.RowTemplate.Height = 23;
            this.datView51.Size = new System.Drawing.Size(194, 187);
            this.datView51.TabIndex = 0;
            // 
            // btnShowData
            // 
            this.btnShowData.Location = new System.Drawing.Point(15, 13);
            this.btnShowData.Name = "btnShowData";
            this.btnShowData.Size = new System.Drawing.Size(75, 23);
            this.btnShowData.TabIndex = 14;
            this.btnShowData.Text = "查看数据";
            this.btnShowData.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(844, 481);
            this.Controls.Add(this.btnShowData);
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
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.grpBoxInfo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.datViewInfo)).EndInit();
            this.grpBoxEnv.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.datViewEnv)).EndInit();
            this.grpBoxResult.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.datViewResult)).EndInit();
            this.grpBoxDevice.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.datViewDevice)).EndInit();
            this.grpBox52.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.datView52)).EndInit();
            this.grpBox53.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.datView53)).EndInit();
            this.grpBox54.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.datView54)).EndInit();
            this.grpBox55.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.datView55)).EndInit();
            this.grpBox56.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.datView56)).EndInit();
            this.grpBox51.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.datView51)).EndInit();
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
        private System.Windows.Forms.DataGridView datViewInfo;
        private System.Windows.Forms.DataGridView datViewEnv;
        private System.Windows.Forms.DataGridView datViewResult;
        private System.Windows.Forms.DataGridView datViewDevice;
        private System.Windows.Forms.DataGridView datView52;
        private System.Windows.Forms.DataGridView datView53;
        private System.Windows.Forms.DataGridView datView54;
        private System.Windows.Forms.DataGridView datView55;
        private System.Windows.Forms.DataGridView datView56;
        private System.Windows.Forms.GroupBox grpBox51;
        private System.Windows.Forms.DataGridView datView51;
        private System.Windows.Forms.Button btnShowData;
    }
}

