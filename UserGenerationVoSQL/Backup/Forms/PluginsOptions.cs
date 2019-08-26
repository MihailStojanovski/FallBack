using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Utils;
using FastReport.Design;

namespace FastReport.Forms
{
  internal partial class PluginsOptions : DesignerOptionsPage
  {
    private Designer FDesigner;
    
    private void Localize()
    {
      MyRes res = new MyRes("Forms,UIOptions");
      tab1.Text = res.Get("");
      lblUIStyle.Text = res.Get("Appearance");
      cbxUIStyle.Items.AddRange(res.Get("AppearanceStyles").Split(new char[] { ',' }));
      
      res = new MyRes("Forms,PluginsOptions");
      tab2.Text = res.Get("");
      lblHint.Text = res.Get("Hint");
      btnAdd.Text = res.Get("Add");
      btnRemove.Text = res.Get("Remove");
      lblNote.Text = res.Get("Note");
    }
    
    private void AddPlugin(string name)
    {
      lbPlugins.Items.Add(name);
    }

    private void btnAdd_Click(object sender, EventArgs e)
    {
      using (OpenFileDialog dialog = new OpenFileDialog())
      {
        dialog.Filter = Res.Get("FileFilters,Assembly");
        if (dialog.ShowDialog() == DialogResult.OK)
        {
          AddPlugin(dialog.FileName);
        }
      }
    }

    private void btnRemove_Click(object sender, EventArgs e)
    {
      lbPlugins.Items.Remove(lbPlugins.SelectedItem);
    }

    private void btnUp_Click(object sender, EventArgs e)
    {
      int index = lbPlugins.SelectedIndex;
      if (index > 0)
      {
        Object item = lbPlugins.SelectedItem;
        lbPlugins.Items.Remove(item);
        lbPlugins.Items.Insert(index - 1, item);
        lbPlugins.SelectedItem = item;
      }
    }

    private void btnDown_Click(object sender, EventArgs e)
    {
      int index = lbPlugins.SelectedIndex;
      if (index < lbPlugins.Items.Count - 1)
      {
        Object item = lbPlugins.SelectedItem;
        lbPlugins.Items.Remove(item);
        lbPlugins.Items.Insert(index + 1, item);
        lbPlugins.SelectedItem = item;
      }
    }

    private void lbPlugins_SelectedIndexChanged(object sender, EventArgs e)
    {
      bool enabled = lbPlugins.SelectedItems.Count != 0;
      btnRemove.Enabled = enabled;
      btnUp.Enabled = enabled;
      btnDown.Enabled = enabled;
    }

    private void lbPlugins_DrawItem(object sender, DrawItemEventArgs e)
    {
      e.DrawBackground();
      if (e.Index >= 0)
      {
        e.Graphics.DrawImage(Res.GetImage(89), e.Bounds.Left, e.Bounds.Top + 1);
        TextRenderer.DrawText(e.Graphics, lbPlugins.Items[e.Index].ToString(), e.Font,
          new Rectangle(e.Bounds.Left + 20, e.Bounds.Top + 2, e.Bounds.Width - 20, e.Bounds.Height), e.ForeColor,
          TextFormatFlags.PathEllipsis);
      }  
    }

    public override void Init()
    {
      cbxUIStyle.SelectedIndex = (int)FDesigner.UIStyle;
      
      btnUp.Image = Res.GetImage(208);
      btnDown.Image = Res.GetImage(209);

      XmlItem pluginsItem = Config.Root.FindItem("Plugins");
      for (int i = 0; i < pluginsItem.Count; i++)
      {
        XmlItem xi = pluginsItem[i];
        AddPlugin(xi.GetProp("Name"));
      }
      
      lblNote.Width = tab2.Width - lblNote.Left * 2;
      lblNote.Height = tab2.Height - lblNote.Top;
      lbPlugins_SelectedIndexChanged(this, EventArgs.Empty);
    }

    public override void Done(DialogResult result)
    {
      if (result == DialogResult.OK)
      {
        FDesigner.UIStyle = (UIStyle)cbxUIStyle.SelectedIndex;
        Config.UIStyle = FDesigner.UIStyle;
        
        XmlItem pluginsItem = Config.Root.FindItem("Plugins");
        pluginsItem.Clear();
        
        foreach (object item in lbPlugins.Items)
        {
          XmlItem xi = pluginsItem.Add();
          xi.Name = "Plugin";
          xi.SetProp("Name", item.ToString());
        }
      }
    }

    public PluginsOptions(Designer designer) : base()
    {
      FDesigner = designer;
      InitializeComponent();
      Localize();
    }
  }
}

