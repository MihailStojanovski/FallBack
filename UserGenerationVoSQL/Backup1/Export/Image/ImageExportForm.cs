using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Export;
using FastReport.Utils;
using FastReport.Forms;

namespace FastReport.Export.Image
{
  internal partial class ImageExportForm : BaseExportForm
  {
    private void cbxImageFormat_SelectedIndexChanged(object sender, EventArgs e)
    {
      int index = cbxImageFormat.SelectedIndex;
      bool isJpeg = index == 2;
      bool isTiff = index == 4;

      lblQuality.Enabled = isJpeg;
      udQuality.Enabled = isJpeg;
      cbMultiFrameTiff.Enabled = isTiff;
      cbMonochrome.Enabled = isTiff;
      lblX.Visible = isTiff;
      udResolutionY.Visible = isTiff;
      cbMultiFrameTiff_CheckedChanged(null, EventArgs.Empty);
    }

    private void cbMultiFrameTiff_CheckedChanged(object sender, EventArgs e)
    {
      cbSeparateFiles.Enabled = !cbMultiFrameTiff.Checked || !cbMultiFrameTiff.Enabled;
    }

    public override void Init(ExportBase export)
    {
      base.Init(export);
      ImageExport imageExport = Export as ImageExport;
      cbxImageFormat.SelectedIndex = (int)imageExport.ImageFormat;
      udResolution.Value = imageExport.ResolutionX;
      udResolutionY.Value = imageExport.ResolutionY;
      udQuality.Value = imageExport.JpegQuality;
      cbSeparateFiles.Checked = imageExport.SeparateFiles;
      cbMultiFrameTiff.Checked = imageExport.MultiFrameTiff;
      cbMonochrome.Checked = imageExport.MonochromeTiff;
      cbMultiFrameTiff_CheckedChanged(null, EventArgs.Empty);
    }

    protected override void Done()
    {
      base.Done();
      ImageExport imageExport = Export as ImageExport;
      imageExport.ImageFormat = (ImageExportFormat)cbxImageFormat.SelectedIndex;
      if (imageExport.ImageFormat == ImageExportFormat.Tiff)
      {
        imageExport.ResolutionX = (int)udResolution.Value;
        imageExport.ResolutionY = (int)udResolutionY.Value;
      }
      else
      {
        imageExport.Resolution = (int)udResolution.Value;
      }
      imageExport.JpegQuality = (int)udQuality.Value;
      imageExport.SeparateFiles = cbSeparateFiles.Checked;
      imageExport.MultiFrameTiff = cbMultiFrameTiff.Checked;
      imageExport.MonochromeTiff = cbMonochrome.Checked;
    }

    public override void Localize()
    {
      base.Localize();
      MyRes res = new MyRes("Export,Image");
      Text = res.Get("");
      gbOptions.Text = Res.Get("Export,Misc,Options");
      lblImageFormat.Text = res.Get("ImageFormat");
      lblResolution.Text = res.Get("Resolution");
      lblQuality.Text = res.Get("Quality");
      cbSeparateFiles.Text = res.Get("SeparateFiles");
      cbMultiFrameTiff.Text = res.Get("MultiFrame");
      cbMonochrome.Text = res.Get("Monochrome");
      cbxImageFormat.Items.AddRange(new string[] {
        res.Get("Bmp"), res.Get("Png"), res.Get("Jpeg"), res.Get("Gif"), res.Get("Tiff"), res.Get("Metafile") });
    }
    
    public ImageExportForm()
    {
      InitializeComponent();
    }
  }
}

