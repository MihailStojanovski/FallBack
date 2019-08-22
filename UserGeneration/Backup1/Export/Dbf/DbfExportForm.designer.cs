namespace FastReport.Forms
{
    partial class DbfExportForm
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
          this.gbOptions = new System.Windows.Forms.GroupBox();
          this.tbFieldNamesFile = new FastReport.Controls.TextBoxButton();
          this.cbDataOnly = new System.Windows.Forms.CheckBox();
          this.cbbCodepage = new System.Windows.Forms.ComboBox();
          this.lblCodepage = new System.Windows.Forms.Label();
          this.cbLoadFieldNamesFromFile = new System.Windows.Forms.CheckBox();
          this.gbPageRange.SuspendLayout();
          this.pcPages.SuspendLayout();
          this.panPages.SuspendLayout();
          this.gbOptions.SuspendLayout();
          this.SuspendLayout();
          // 
          // gbPageRange
          // 
          this.gbPageRange.Location = new System.Drawing.Point(8, 4);
          // 
          // pcPages
          // 
          this.pcPages.Location = new System.Drawing.Point(0, 0);
          this.pcPages.Size = new System.Drawing.Size(276, 272);
          // 
          // panPages
          // 
          this.panPages.Controls.Add(this.gbOptions);
          this.panPages.Dock = System.Windows.Forms.DockStyle.None;
          this.panPages.Size = new System.Drawing.Size(276, 272);
          this.panPages.Controls.SetChildIndex(this.gbPageRange, 0);
          this.panPages.Controls.SetChildIndex(this.gbOptions, 0);
          // 
          // cbOpenAfter
          // 
          this.cbOpenAfter.Location = new System.Drawing.Point(12, 281);
          // 
          // btnOk
          // 
          this.btnOk.Location = new System.Drawing.Point(108, 304);
          // 
          // btnCancel
          // 
          this.btnCancel.Location = new System.Drawing.Point(189, 304);
          this.btnCancel.TabIndex = 1;
          // 
          // gbOptions
          // 
          this.gbOptions.Controls.Add(this.tbFieldNamesFile);
          this.gbOptions.Controls.Add(this.cbDataOnly);
          this.gbOptions.Controls.Add(this.cbbCodepage);
          this.gbOptions.Controls.Add(this.lblCodepage);
          this.gbOptions.Controls.Add(this.cbLoadFieldNamesFromFile);
          this.gbOptions.Location = new System.Drawing.Point(8, 136);
          this.gbOptions.Name = "gbOptions";
          this.gbOptions.Size = new System.Drawing.Size(260, 132);
          this.gbOptions.TabIndex = 5;
          this.gbOptions.TabStop = false;
          this.gbOptions.Text = "Options";
          // 
          // tbFieldNamesFile
          // 
          this.tbFieldNamesFile.Image = null;
          this.tbFieldNamesFile.Location = new System.Drawing.Point(12, 96);
          this.tbFieldNamesFile.Name = "tbFieldNamesFile";
          this.tbFieldNamesFile.Size = new System.Drawing.Size(236, 21);
          this.tbFieldNamesFile.TabIndex = 12;
          this.tbFieldNamesFile.ButtonClick += new System.EventHandler(this.tbFieldNamesFile_ButtonClick);
          // 
          // cbDataOnly
          // 
          this.cbDataOnly.AutoSize = true;
          this.cbDataOnly.Location = new System.Drawing.Point(12, 52);
          this.cbDataOnly.Name = "cbDataOnly";
          this.cbDataOnly.Size = new System.Drawing.Size(72, 17);
          this.cbDataOnly.TabIndex = 9;
          this.cbDataOnly.Text = "Data only";
          this.cbDataOnly.UseVisualStyleBackColor = true;
          // 
          // cbbCodepage
          // 
          this.cbbCodepage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
          this.cbbCodepage.FormattingEnabled = true;
          this.cbbCodepage.Items.AddRange(new object[] {
            "Default",
            "OEM"});
          this.cbbCodepage.Location = new System.Drawing.Point(104, 20);
          this.cbbCodepage.Name = "cbbCodepage";
          this.cbbCodepage.Size = new System.Drawing.Size(144, 21);
          this.cbbCodepage.TabIndex = 8;
          // 
          // lblCodepage
          // 
          this.lblCodepage.AutoSize = true;
          this.lblCodepage.Location = new System.Drawing.Point(12, 24);
          this.lblCodepage.Name = "lblCodepage";
          this.lblCodepage.Size = new System.Drawing.Size(56, 13);
          this.lblCodepage.TabIndex = 7;
          this.lblCodepage.Text = "Codepage";
          // 
          // cbLoadFieldNamesFromFile
          // 
          this.cbLoadFieldNamesFromFile.AutoSize = true;
          this.cbLoadFieldNamesFromFile.Location = new System.Drawing.Point(12, 72);
          this.cbLoadFieldNamesFromFile.Name = "cbLoadFieldNamesFromFile";
          this.cbLoadFieldNamesFromFile.Size = new System.Drawing.Size(171, 17);
          this.cbLoadFieldNamesFromFile.TabIndex = 6;
          this.cbLoadFieldNamesFromFile.Text = "Load field names from the file:";
          this.cbLoadFieldNamesFromFile.UseVisualStyleBackColor = true;
          // 
          // DbfExportForm
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
          this.ClientSize = new System.Drawing.Size(276, 340);
          this.Name = "DbfExportForm";
          this.Text = "Export to dBase";
          this.gbPageRange.ResumeLayout(false);
          this.gbPageRange.PerformLayout();
          this.pcPages.ResumeLayout(false);
          this.panPages.ResumeLayout(false);
          this.gbOptions.ResumeLayout(false);
          this.gbOptions.PerformLayout();
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion // Windows Form Designer generated code

        private System.Windows.Forms.GroupBox gbOptions;
        private System.Windows.Forms.ComboBox cbbCodepage;
        private System.Windows.Forms.Label lblCodepage;
        private System.Windows.Forms.CheckBox cbDataOnly;
      private System.Windows.Forms.CheckBox cbLoadFieldNamesFromFile;
      private FastReport.Controls.TextBoxButton tbFieldNamesFile;
    }
}