using System.IO;
using System;
using FastReport.Utils;

namespace FastReport.Web
{
    public partial class WebReport
    {
        private void SetEnableButtons()
        {
            if (ShowToolbar)
            {
                btnNext.Enabled = CurrentPage < (TotalPages - 1);
                btnFirst.Enabled = CurrentPage > 0;
                btnPrev.Enabled = CurrentPage > 0;
                btnLast.Enabled = CurrentPage < (TotalPages - 1);
                btnFirst.ImageUrl = GetButtonImageURL("First", btnFirst.Enabled);
                btnPrev.ImageUrl = GetButtonImageURL("Prev", btnPrev.Enabled);
                btnNext.ImageUrl = GetButtonImageURL("Next", btnNext.Enabled);
                btnLast.ImageUrl = GetButtonImageURL("Last", btnLast.Enabled);
            }
        }

        private Stream GetButtonStream(string resource)
        {
            foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                System.Reflection.AssemblyName name = new System.Reflection.AssemblyName(a.FullName);
                if (name.Name == "FastReport")
                    return a.GetManifestResourceStream("FastReport.Web.Buttons." + resource);
            }
            return null;
        }

        private string GetButtonDesignPath(string Name)
        {
            string resName = Name + ".gif";
            string custPath = ButtonsPath + "/" + resName;
            if (!String.IsNullOrEmpty(ButtonsPath))
                return (custPath).Replace("\\", "/");
            else
            {
                string PicName = "frx" + resName;
                string ImagePath = Environment.GetEnvironmentVariable("Temp") + "\\FastReport";
                string FileName = ImagePath + "\\" + resName;
                if (!Directory.Exists(ImagePath))
                    Directory.CreateDirectory(ImagePath);
                if (!File.Exists(FileName))
                {
                    using (Stream stream = GetButtonStream(resName))
                    {
                        if (stream != null)
                        {
                            byte[] buff = new byte[stream.Length];
                            stream.Read(buff, 0, (int)stream.Length);
                            using (FileStream file = new FileStream(FileName, FileMode.Create))
                                file.Write(buff, 0, buff.Length);
                        }
                    }
                }
                return FileName;
            }
        }

        private string GetButtonImageURL(string Name, bool Enabled)
        {
          string resName = Name + (Enabled ? "" : "_disabled") + ".gif";
          string custPath = Page.Request.ApplicationPath + "\\" + ButtonsPath + "\\" + resName;
          if (!String.IsNullOrEmpty(ButtonsPath))
            return (custPath).Replace("\\", "/").Replace("//", "/");
          else
          {
            string PicName = "frx" + resName;
            using (Stream stream = GetButtonStream(resName))
            {
              if (stream != null)
              {
                byte[] buff = new byte[stream.Length];
                stream.Read(buff, 0, (int)stream.Length);
                CacheAdd(PicName, buff, null, 3);
              }
            }
            return this.Page.Request.CurrentExecutionFilePath + "?" + FBtnPrefix + "=" + PicName;
          }
        }
      }
}