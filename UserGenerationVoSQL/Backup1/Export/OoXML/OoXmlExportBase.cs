using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

//using FastReport.Utils;
//using FastReport.Forms;
//using FastReport.Export;
//using FastReport.Format;

using System.Globalization;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.Collections;

namespace FastReport.Export.OoXML
{
    /// <summary>
    /// Base class for Microsoft Office 2007 export objects
    /// </summary>
    public class OOExportBase : ExportBase
    {
        /// <summary>
        /// Timer ticks count
        /// </summary>
        protected int FExportTickCount;

        /// <summary>
        /// Temporary folder for export files
        /// </summary>
        protected string FTempFolder;

        /// <summary>
        /// Default XML header
        /// </summary>
        #region Constants
        protected const string xml_header = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
        #endregion

        #region Properties
        internal string TempFolder { get { return FTempFolder; } }
        #endregion

        #region Helpers
        internal string Quoted(string p)
        {
            return "\"" + p + "\" ";
        }

        internal string QuotedRoot(string p)
        {
            return "\"/" + p + "\" ";
        }
        #endregion

    }

    /// <summary>
    /// Base class for export Office Open objects
    /// </summary>
    internal abstract class OoXMLBase
    {
        #region Private fileds
        private ArrayList FRelations = new ArrayList();
        private int Id;
        #endregion

        #region Constants
        public const string xml_header = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
        #endregion

        #region Abstract
        public abstract string RelationType { get; }
        public abstract string ContentType { get; }
        public abstract string FileName { get; }
        #endregion

        #region Helpers
        protected string Quoted(string p)
        {
            return "\"" + p + "\" ";
        }
        protected string Quoted(long p)
        {
            return "\"" + p.ToString() + "\" ";
        }

        protected string GetDashStyle(System.Drawing.Drawing2D.DashStyle style)
        {
            switch (style)
            {
                case System.Drawing.Drawing2D.DashStyle.Solid:      return "<a:prstDash val=\"solid\"/>";
                case System.Drawing.Drawing2D.DashStyle.Dot:        return "<a:prstDash val=\"dot\"/>";
                case System.Drawing.Drawing2D.DashStyle.Dash:       return "<a:prstDash val=\"dash\"/>";
                case System.Drawing.Drawing2D.DashStyle.DashDot:    return "<a:prstDash val=\"dashDot\"/>";
                case System.Drawing.Drawing2D.DashStyle.DashDotDot: return "<a:prstDash val=\"sysDashDotDot\"/>";
            }
            throw new Exception("Unsupported dash style");
        }

        private string TranslatePath(string source, string dest)
        {
            int j;
            string result = "";
            string[] rel_dir_name = Path.GetDirectoryName(source).Split('\\');
            string[] items_dir_name = Path.GetDirectoryName(dest).Split('\\');

            for (int i = 0; ; i++)
            {
                if( i==rel_dir_name.Length || i==items_dir_name.Length || items_dir_name[i].CompareTo( rel_dir_name[i] ) != 0 )
                {
                    for ( j = i; j < rel_dir_name.Length; j++ ) result += "../";
                    for ( j = i; j< items_dir_name.Length; j++ ) result += items_dir_name[j];
                    break;
                }
            }

            if (result != "") result += "/";

            return result;
        }
        #endregion

        #region Properties
        internal string rId { get { return "rId" + Id.ToString(); } }
        public ArrayList RelationList { get { return FRelations; } }
        #endregion

        #region Protected methods
        protected void ExportRelations( OOExportBase export_base )
        {
            if (FRelations.Count != 0)
            {
                string relation_dir_name = export_base.TempFolder + "\\" + Path.GetDirectoryName(FileName) + "\\_rels\\";
                string relation_file_name = Path.GetFileName(FileName) + ".rels";
                string related_path = "";

                if (!Directory.Exists( relation_dir_name ))
                {
                    Directory.CreateDirectory( relation_dir_name );
                }

                using (FileStream file = new FileStream(relation_dir_name + relation_file_name, FileMode.Create))
                using (StreamWriter Out = new StreamWriter(file))
                {
                    Out.WriteLine(xml_header);
                    Out.WriteLine("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
                    foreach (OoXMLBase relation_item in FRelations)
                    {
                        related_path = TranslatePath(FileName, relation_item.FileName) + Path.GetFileName(relation_item.FileName);

                        Out.WriteLine(
                            "<Relationship Id=" + Quoted(relation_item.rId) +
                            "Type=" + Quoted(relation_item.RelationType) +
                            "Target=" + Quoted(related_path) + "/>");
                    }
                    Out.WriteLine("</Relationships>");
                }
            }
        }
        #endregion

        #region Internal Methods
        internal void AddRelation( int Id, OoXMLBase related_object )
        {
            related_object.Id = Id;
            FRelations.Add(related_object);
        }
        #endregion
    }

    /// <summary>
    /// Core document properties
    /// </summary>
    class OoXMLCoreDocumentProperties : OoXMLBase
    {
        #region Class overrides
        public override string RelationType { get { return "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties"; } }
        public override string ContentType { get { return "application/vnd.openxmlformats-package.core-properties+xml"; } }
        public override string FileName { get { return "docProps/core.xml"; } }
        #endregion

        public void Export(OOExportBase OoXML)
        {
            using (FileStream file = new FileStream(OoXML.TempFolder + "/" + FileName, FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine(xml_header);
                Out.WriteLine("<cp:coreProperties xmlns:cp=\"http://schemas.openxmlformats.org/package/2006/metadata/core-properties\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:dcmitype=\"http://purl.org/dc/dcmitype/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
                Out.WriteLine("<dcterms:created xsi:type=\"dcterms:W3CDTF\">2009-06-17T07:33:19Z</dcterms:created>");
                Out.WriteLine("<dc:creator>FastReport.NET</dc:creator>");
                Out.WriteLine("</cp:coreProperties>");
            }
        }
    }

    /// <summary>
    /// Core document properties
    /// </summary>
    class OoXMLApplicationProperties : OoXMLBase
    {
        #region Class overrides
        public override string RelationType { get { return "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties"; } }
        public override string ContentType { get { return "application/vnd.openxmlformats-officedocument.extended-properties+xml"; } }
        public override string FileName { get { return "docProps/app.xml"; } }
        #endregion

        public void Export(OOExportBase OoXML)
        {
            using (FileStream file = new FileStream(OoXML.TempFolder + "/" + FileName, FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine(xml_header);
                Out.WriteLine("<Properties xmlns=\"http://schemas.openxmlformats.org/officeDocument/2006/extended-properties\" xmlns:vt=\"http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes\">");
                Out.WriteLine("<DocSecurity>0</DocSecurity>");
                Out.WriteLine("<ScaleCrop>false</ScaleCrop>");

                // Heading description
                Out.WriteLine("<HeadingPairs>");

                Out.WriteLine("<vt:vector size=\"2\" baseType=\"variant\">");

                Out.WriteLine("<vt:variant>");
                Out.WriteLine("<vt:lpstr>Worksheets</vt:lpstr>");
                Out.WriteLine("</vt:variant>");

                Out.WriteLine("<vt:variant>");
                Out.WriteLine("<vt:i4>2</vt:i4>");
                Out.WriteLine("</vt:variant>");

                Out.WriteLine("</vt:vector>");

                Out.WriteLine("</HeadingPairs>");

                // Titles description
                Out.WriteLine("<TitlesOfParts>");
                Out.WriteLine("<vt:vector size=\"1\" baseType=\"lpstr\">");
                Out.WriteLine("<vt:lpstr>Лист1</vt:lpstr>");
                Out.WriteLine("</vt:vector>");
                Out.WriteLine("</TitlesOfParts>");

                Out.WriteLine("<LinksUpToDate>false</LinksUpToDate>");
                Out.WriteLine("<SharedDoc>false</SharedDoc>");
                Out.WriteLine("<HyperlinksChanged>false</HyperlinksChanged>");
                Out.WriteLine("<AppVersion>12.0000</AppVersion>");

                Out.WriteLine("</Properties>");
            }
        }
    }

    internal class OoXMLThemes : OoXMLBase
    {
        #region Class overrides
        public override string RelationType { get { return "http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme"; } }
        public override string ContentType { get { return "application/vnd.openxmlformats-officedocument.theme+xml"; } }
        public override string FileName { get { return "ppt/theme/theme1.xml"; } }
        #endregion

        public void Export(OOExportBase OoXML, string ThemeRes, string ThemePath)
        {
            //ResourceSet set = new ResourceSet();

            // get a reference to the current assembly
            Assembly a = Assembly.GetExecutingAssembly();

            // get a list of resource names from the manifest
            string[] resNames = a.GetManifestResourceNames();

            Stream o = a.GetManifestResourceStream("FastReport.Export.OoXML.theme1.xml");

            int length = 4096;
            int bytesRead = 0;
            Byte[] buffer = new Byte[length];

            // write the required bytes
            using (FileStream fs = new FileStream(OoXML.TempFolder + ThemePath, FileMode.OpenOrCreate))
            {
                do
                {
                    bytesRead = o.Read(buffer, 0, length);
                    fs.Write(buffer, 0, bytesRead);
                }
                while (bytesRead == length);
            }

            o.Dispose();
        }
    }
}
