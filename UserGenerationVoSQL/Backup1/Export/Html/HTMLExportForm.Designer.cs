namespace FastReport.Forms
{
    partial class HTMLExportForm
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
            this.lblFormat = new System.Windows.Forms.Label();
            this.cbbFormat = new System.Windows.Forms.ComboBox();
            this.cbNavigator = new System.Windows.Forms.CheckBox();
            this.cbSinglePage = new System.Windows.Forms.CheckBox();
            this.cbSubFolder = new System.Windows.Forms.CheckBox();
            this.cbPictures = new System.Windows.Forms.CheckBox();
            this.cbWysiwyg = new System.Windows.Forms.CheckBox();
            this.gbPageRange.SuspendLayout();
            this.pcPages.SuspendLayout();
            this.panPages.SuspendLayout();
            this.gbOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbPageRange
            // 
            this.gbPageRange.Location = new System.Drawing.Point(8, 4);
            this.gbPageRange.Enter += new System.EventHandler(this.gbPageRange_Enter);
            // 
            // pcPages
            // 
            this.pcPages.Location = new System.Drawing.Point(0, 0);
            this.pcPages.Size = new System.Drawing.Size(276, 308);
            // 
            // panPages
            // 
            this.panPages.Controls.Add(this.gbOptions);
            this.panPages.Dock = System.Windows.Forms.DockStyle.None;
            this.panPages.Size = new System.Drawing.Size(276, 309);
            this.panPages.Controls.SetChildIndex(this.gbPageRange, 0);
            this.panPages.Controls.SetChildIndex(this.gbOptions, 0);
            // 
            // cbOpenAfter
            // 
            this.cbOpenAfter.Location = new System.Drawing.Point(8, 314);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(109, 338);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(189, 338);
            this.btnCancel.TabIndex = 1;
            // 
            // gbOptions
            // 
            this.gbOptions.Controls.Add(this.lblFormat);
            this.gbOptions.Controls.Add(this.cbbFormat);
            this.gbOptions.Controls.Add(this.cbNavigator);
            this.gbOptions.Controls.Add(this.cbSinglePage);
            this.gbOptions.Controls.Add(this.cbSubFolder);
            this.gbOptions.Controls.Add(this.cbPictures);
            this.gbOptions.Controls.Add(this.cbWysiwyg);
            this.gbOptions.Location = new System.Drawing.Point(8, 136);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(260, 172);
            this.gbOptions.TabIndex = 5;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = "Options";
            // 
            // lblFormat
            // 
            this.lblFormat.AutoSize = true;
            this.lblFormat.Location = new System.Drawing.Point(9, 22);
            this.lblFormat.Name = "lblFormat";
            this.lblFormat.Size = new System.Drawing.Size(41, 13);
            this.lblFormat.TabIndex = 9;
            this.lblFormat.Text = "Format";
            // 
            // cbbFormat
            // 
            this.cbbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbFormat.FormattingEnabled = true;
            this.cbbFormat.Items.AddRange(new object[] {
            "HTML",
            "MHT"});
            this.cbbFormat.Location = new System.Drawing.Point(104, 19);
            this.cbbFormat.Name = "cbbFormat";
            this.cbbFormat.Size = new System.Drawing.Size(121, 21);
            this.cbbFormat.TabIndex = 8;
            this.cbbFormat.SelectedIndexChanged += new System.EventHandler(this.cbbFormat_SelectedIndexChanged);
            // 
            // cbNavigator
            // 
            this.cbNavigator.AutoSize = true;
            this.cbNavigator.Checked = true;
            this.cbNavigator.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbNavigator.Location = new System.Drawing.Point(12, 119);
            this.cbNavigator.Name = "cbNavigator";
            this.cbNavigator.Size = new System.Drawing.Size(73, 17);
            this.cbNavigator.TabIndex = 7;
            this.cbNavigator.Text = "Navigator";
            this.cbNavigator.UseVisualStyleBackColor = true;
            // 
            // cbSinglePage
            // 
            this.cbSinglePage.AutoSize = true;
            this.cbSinglePage.Location = new System.Drawing.Point(12, 143);
            this.cbSinglePage.Name = "cbSinglePage";
            this.cbSinglePage.Size = new System.Drawing.Size(81, 17);
            this.cbSinglePage.TabIndex = 6;
            this.cbSinglePage.Text = "Single page";
            this.cbSinglePage.UseVisualStyleBackColor = true;
            // 
            // cbSubFolder
            // 
            this.cbSubFolder.AutoSize = true;
            this.cbSubFolder.Checked = true;
            this.cbSubFolder.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSubFolder.Location = new System.Drawing.Point(12, 95);
            this.cbSubFolder.Name = "cbSubFolder";
            this.cbSubFolder.Size = new System.Drawing.Size(76, 17);
            this.cbSubFolder.TabIndex = 5;
            this.cbSubFolder.Text = "Sub-folder";
            this.cbSubFolder.UseVisualStyleBackColor = true;
            // 
            // cbPictures
            // 
            this.cbPictures.AutoSize = true;
            this.cbPictures.Checked = true;
            this.cbPictures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbPictures.Location = new System.Drawing.Point(12, 71);
            this.cbPictures.Name = "cbPictures";
            this.cbPictures.Size = new System.Drawing.Size(64, 17);
            this.cbPictures.TabIndex = 3;
            this.cbPictures.Text = "Pictures";
            this.cbPictures.UseVisualStyleBackColor = true;
            // 
            // cbWysiwyg
            // 
            this.cbWysiwyg.AutoSize = true;
            this.cbWysiwyg.Checked = true;
            this.cbWysiwyg.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbWysiwyg.Location = new System.Drawing.Point(12, 47);
            this.cbWysiwyg.Name = "cbWysiwyg";
            this.cbWysiwyg.Size = new System.Drawing.Size(69, 17);
            this.cbWysiwyg.TabIndex = 1;
            this.cbWysiwyg.Text = "Wysiwyg";
            this.cbWysiwyg.UseVisualStyleBackColor = true;
            // 
            // HTMLExportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.ClientSize = new System.Drawing.Size(276, 373);
            this.Name = "HTMLExportForm";
            this.Text = "Export to HTML";
            this.gbPageRange.ResumeLayout(false);
            this.gbPageRange.PerformLayout();
            this.pcPages.ResumeLayout(false);
            this.panPages.ResumeLayout(false);
            this.gbOptions.ResumeLayout(false);
            this.gbOptions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbOptions;
      private System.Windows.Forms.CheckBox cbWysiwyg;
        private System.Windows.Forms.CheckBox cbPictures;
        private System.Windows.Forms.CheckBox cbSubFolder;
        private System.Windows.Forms.CheckBox cbNavigator;
        private System.Windows.Forms.CheckBox cbSinglePage;
        private System.Windows.Forms.ComboBox cbbFormat;
        private System.Windows.Forms.Label lblFormat;

    }
}
