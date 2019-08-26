using System;
using System.IO;

namespace FastReport.Web
{

    internal class WebExportItem
    {
        private MemoryStream FFile;
        private string FFileName;
        private string FFormat;

        public MemoryStream File
        {
            get { return FFile; }
            set { FFile = value; }
        }

        public string FileName
        {
            get { return FFileName; }
            set { FFileName = value; }
        }

        public string Format
        {
            get { return FFormat; }
            set { FFormat = value; }
        }

        public WebExportItem()
        {
            FFile = new MemoryStream();            
        }
    }
}