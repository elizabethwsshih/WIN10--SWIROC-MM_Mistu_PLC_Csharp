namespace MM_Mistu_PLC_Csharp
{
    partial class AutoAreaSpeedSet
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
            this.components = new System.ComponentModel.Container();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.AutoAreaSpeedSaveBtn = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(907, 296);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            // 
            // AutoAreaSpeedSaveBtn
            // 
            this.AutoAreaSpeedSaveBtn.Font = new System.Drawing.Font("Microsoft JhengHei", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.AutoAreaSpeedSaveBtn.Location = new System.Drawing.Point(12, 314);
            this.AutoAreaSpeedSaveBtn.Name = "AutoAreaSpeedSaveBtn";
            this.AutoAreaSpeedSaveBtn.Size = new System.Drawing.Size(84, 38);
            this.AutoAreaSpeedSaveBtn.TabIndex = 1;
            this.AutoAreaSpeedSaveBtn.Text = "確認";
            this.AutoAreaSpeedSaveBtn.UseVisualStyleBackColor = true;
            this.AutoAreaSpeedSaveBtn.Click += new System.EventHandler(this.AutoAreaSpeedSaveBtn_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft JhengHei", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button2.Location = new System.Drawing.Point(102, 314);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(84, 38);
            this.button2.TabIndex = 2;
            this.button2.Text = "取消";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // AutoAreaSpeedSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 364);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.AutoAreaSpeedSaveBtn);
            this.Controls.Add(this.dataGridView1);
            this.Name = "AutoAreaSpeedSet";
            this.Text = "第二階段參數設置";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AutoAreaSpeedSet_FormClosed);
            this.Load += new System.EventHandler(this.AutoAreaSpeedSet_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button AutoAreaSpeedSaveBtn;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}