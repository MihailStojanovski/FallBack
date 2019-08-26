using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using FastReport.Utils;
using FastReport.Forms;

namespace FastReport.Data.ConnectionEditors
{
  internal partial class MsAccessConnectionEditor : ConnectionEditorBase
  {
    private string FConnectionString;
    
    private void Localize()
    {
      MyRes res = new MyRes("ConnectionEditors,Common");
      gbDatabase.Text = res.Get("Database");
      lblDatabase.Text = res.Get("DatabaseFile");
      lblUserName.Text = res.Get("UserName");
      lblPassword.Text = res.Get("Password");
      btnAdvanced.Text = Res.Get("Buttons,Advanced");
      tbDatabase.Image = Res.GetImage(1);
    }

    private void tbDatabase_ButtonClick(object sender, EventArgs e)
    {
      using (OpenFileDialog dialog = new OpenFileDialog())
      {
        dialog.Filter = Res.Get("FileFilters,MdbFile");
        if (dialog.ShowDialog() == DialogResult.OK)
          tbDatabase.Text = dialog.FileName;
      }
    }

    private void btnAdvanced_Click(object sender, EventArgs e)
    {
      using (AdvancedConnectionPropertiesForm form = new AdvancedConnectionPropertiesForm())
      {
        OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(ConnectionString);
        builder.BrowsableConnectionString = false;
        form.AdvancedProperties = builder;
        
        if (form.ShowDialog() == DialogResult.OK)
          ConnectionString = form.AdvancedProperties.ToString();
      }
    }

    protected override string GetConnectionString()
    {
      OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(FConnectionString);
      builder.DataSource = tbDatabase.Text;
      
      if (!String.IsNullOrEmpty(tbUserName.Text))
        builder.Add("User ID", tbUserName.Text);
      else
        builder.Remove("User ID");
      
      if (!String.IsNullOrEmpty(tbPassword.Text))
        builder.Add("Password", tbPassword.Text);
      else
        builder.Remove("Password");  
      
      return builder.ToString();
    }
    
    protected override void SetConnectionString(string value)
    {
      FConnectionString = value;
      OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(value);
      tbDatabase.Text = builder.DataSource;

      object userName;
      builder.TryGetValue("User ID", out userName);
      tbUserName.Text = userName == null ? "" : userName.ToString();

      object password;
      builder.TryGetValue("Password", out password);
      tbPassword.Text = password == null ? "" : password.ToString();
    }
    
    public MsAccessConnectionEditor()
    {
      InitializeComponent();
      Localize();
    }
  }
}

