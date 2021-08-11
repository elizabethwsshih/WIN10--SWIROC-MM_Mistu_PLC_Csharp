namespace MM_Mistu_PLC_Csharp
{
    partial class AutoModeAreaSelect
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ReStartAreaBtn = new System.Windows.Forms.Button();
            this.ReStartCancelAreaBtn = new System.Windows.Forms.Button();
            this.ManualStepDistTextBox = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.AutoReYAreaComboBox = new System.Windows.Forms.ComboBox();
            this.label20 = new System.Windows.Forms.Label();
            this.AutoReXAreaComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // ReStartAreaBtn
            // 
            this.ReStartAreaBtn.Location = new System.Drawing.Point(25, 102);
            this.ReStartAreaBtn.Name = "ReStartAreaBtn";
            this.ReStartAreaBtn.Size = new System.Drawing.Size(93, 41);
            this.ReStartAreaBtn.TabIndex = 0;
            this.ReStartAreaBtn.Text = "確定";
            this.ReStartAreaBtn.UseVisualStyleBackColor = true;
            this.ReStartAreaBtn.Click += new System.EventHandler(this.ReStartAreaBtn_Click);
            // 
            // ReStartCancelAreaBtn
            // 
            this.ReStartCancelAreaBtn.Location = new System.Drawing.Point(124, 102);
            this.ReStartCancelAreaBtn.Name = "ReStartCancelAreaBtn";
            this.ReStartCancelAreaBtn.Size = new System.Drawing.Size(95, 41);
            this.ReStartCancelAreaBtn.TabIndex = 1;
            this.ReStartCancelAreaBtn.Text = "取消";
            this.ReStartCancelAreaBtn.UseVisualStyleBackColor = true;
            this.ReStartCancelAreaBtn.Click += new System.EventHandler(this.ReStartCancelAreaBtn_Click);
            // 
            // ManualStepDistTextBox
            // 
            this.ManualStepDistTextBox.Font = new System.Drawing.Font("Microsoft JhengHei", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ManualStepDistTextBox.Location = new System.Drawing.Point(313, -12);
            this.ManualStepDistTextBox.Name = "ManualStepDistTextBox";
            this.ManualStepDistTextBox.Size = new System.Drawing.Size(100, 33);
            this.ManualStepDistTextBox.TabIndex = 23;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft JhengHei", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label17.Location = new System.Drawing.Point(8, 19);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(136, 24);
            this.label17.TabIndex = 24;
            this.label17.Text = "X方向指定區域";
            // 
            // AutoReYAreaComboBox
            // 
            this.AutoReYAreaComboBox.Font = new System.Drawing.Font("Microsoft JhengHei", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.AutoReYAreaComboBox.FormattingEnabled = true;
            this.AutoReYAreaComboBox.Location = new System.Drawing.Point(150, 51);
            this.AutoReYAreaComboBox.Name = "AutoReYAreaComboBox";
            this.AutoReYAreaComboBox.Size = new System.Drawing.Size(121, 32);
            this.AutoReYAreaComboBox.TabIndex = 27;
            this.AutoReYAreaComboBox.Text = "1";
            this.AutoReYAreaComboBox.DropDown += new System.EventHandler(this.AutoReYAreaComboBox_DropDown);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Microsoft JhengHei", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label20.Location = new System.Drawing.Point(8, 51);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(135, 24);
            this.label20.TabIndex = 25;
            this.label20.Text = "Y方向指定區域";
            // 
            // AutoReXAreaComboBox
            // 
            this.AutoReXAreaComboBox.Font = new System.Drawing.Font("Microsoft JhengHei", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.AutoReXAreaComboBox.FormattingEnabled = true;
            this.AutoReXAreaComboBox.Location = new System.Drawing.Point(150, 12);
            this.AutoReXAreaComboBox.Name = "AutoReXAreaComboBox";
            this.AutoReXAreaComboBox.Size = new System.Drawing.Size(121, 32);
            this.AutoReXAreaComboBox.TabIndex = 26;
            this.AutoReXAreaComboBox.Text = "1";
            this.AutoReXAreaComboBox.DropDown += new System.EventHandler(this.AutoReXAreaComboBox_DropDown);
            // 
            // AutoModeAreaSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(309, 177);
            this.Controls.Add(this.AutoReYAreaComboBox);
            this.Controls.Add(this.AutoReXAreaComboBox);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.ManualStepDistTextBox);
            this.Controls.Add(this.ReStartCancelAreaBtn);
            this.Controls.Add(this.ReStartAreaBtn);
            this.Name = "AutoModeAreaSelect";
            this.Text = "選擇要重新焊接的起始區域";
            this.Load += new System.EventHandler(this.AutoModeAreaSelect_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ReStartAreaBtn;
        private System.Windows.Forms.Button ReStartCancelAreaBtn;
        private System.Windows.Forms.TextBox ManualStepDistTextBox;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.ComboBox AutoReYAreaComboBox;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.ComboBox AutoReXAreaComboBox;
    }
}