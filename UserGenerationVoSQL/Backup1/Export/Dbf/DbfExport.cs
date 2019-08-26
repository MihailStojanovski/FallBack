using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using FastReport.Utils;
using FastReport.Forms;
using FastReport.Export;
using System.Globalization;

namespace FastReport.Export.Dbf
{
    /// <summary>
    /// DBF export
    /// </summary>
    public class DBFExport : ExportBase
    {
        #region Constants

        private const byte Ox00 = 0x00;
        private const byte SIMPLE_TABLE = 0x03;
        private const char FIELD_TYPE = 'C';
        private const byte FILE_HEADER_END = 0x0D;
        private const byte RECORD_NOT_REMOVED = 0x20;
        private const byte RECORD_REMOVED = 0x2A;
        private const byte DBF_FILE_END = 0x1A;
        private const int RECORDS_COUNT_LENGTH = 4;
        private const int HEADER_NULL_FILL = 20;
        private const int FIELD_NULL_FILL = 15;
        private const int FILE_HEADER_SIZE = 32;
        private const int FIELD_HEADER_SIZE = 32;
        private const char SPACE = ' ';
        private const char UNDERSCORE = '_';
        private const string DEFAULT_FIELD_NAME = "FIELD";

        #endregion // Constants

        #region Fields

        private ExportMatrix FMatrix;
        private Encoding FEncoding;
        private bool FDataOnly;
        private string FFieldNamesFileName;
        private bool FLoadFieldNamesFromFile;
        private List<byte> header;
        private List<Record> records;
        private Record record;
        private StringBuilder field;
        private List<int> maxFieldsSize;
        private List<StringBuilder> fieldNames;

        #endregion // Fields

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public Encoding Encoding
        {
            get { return FEncoding; }
            set { FEncoding = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool DataOnly
        {
            get { return FDataOnly; }
            set { FDataOnly = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool LoadFieldNamesFromFile
        {
            get { return FLoadFieldNamesFromFile; }
            set { FLoadFieldNamesFromFile = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string FieldNamesFileName
        {
            get { return FFieldNamesFileName; }
            set { FFieldNamesFileName = value; }
        }

        #endregion // Properties

        #region Private Methods

        private void PrepareRecords()
        {
            int i, x, y;
            ExportIEMObject obj;
            for (y = 0; y < FMatrix.Height - 1; y++)
            {
                for (x = 0; x < FMatrix.Width; x++)
                {
                    i = FMatrix.Cell(x, y);
                    if (i != -1)
                    {
                        obj = FMatrix.ObjectById(i);
                        if (obj.Counter == 0)
                        {
                            field = new StringBuilder(obj.Text);
                            record.Add(field);
                            obj.Counter = 1;
                        }
                    }
                }
                if (record.Count == 0)
                {
                    field = new StringBuilder("");
                    record.Add(field);
                }
                records.Add(record);
                record = new Record();
            }
        }

        private int GetRecordSize()
        {
            int maxSize = 0;
            foreach (int m in maxFieldsSize)
            {
                maxSize += m;
            }
            return ++maxSize;
        }

        private int GetMaxRecordCount()
        {
            int maxCount = 0;
            foreach (Record rec in records)
            {
                if (rec.Count > maxCount)
                {
                    maxCount = rec.Count;
                }
            }
            return maxCount;
        }

        private List<int> GetMaxFieldsSize()
        {
            List<int> maxFieldsSize = new List<int>();
            int maxCount = GetMaxRecordCount();
            for (int i = 0; i < maxCount; i++)
            {
                int maxFieldSize = 0;
                foreach (Record rec in records)
                {
                    if (rec.Count < i + 1)
                    {
                        continue;
                    }
                    if (rec[i].Length > maxFieldSize)
                    {
                        maxFieldSize = rec[i].Length;
                    }
                }
                maxFieldsSize.Add(maxFieldSize);
            }
            return maxFieldsSize;
        }

        private int GetHeaderSize()
        {
            return (FILE_HEADER_SIZE + FIELD_HEADER_SIZE * maxFieldsSize.Count + 1);
        }

        private void CompleteRecordsEmptyFields()
        {
            maxFieldsSize = GetMaxFieldsSize();
            int maxCount = maxFieldsSize.Count;
            StringBuilder emptyField = new StringBuilder();
            foreach (Record rec in records)
            {
                for (int i = 0; i < maxCount; i++)
                {
                    if (i < rec.Count)
                    {
                        if (rec[i].Length < maxFieldsSize[i])
                        {
                            rec[i].Append(SPACE, maxFieldsSize[i] - rec[i].Length);
                        }
                    }
                    else
                    {
                        emptyField.Append(SPACE, maxFieldsSize[i]);
                        rec.Add(emptyField);
                        emptyField = new StringBuilder();
                    }
                }
            }
        }

        private void CompleteByteListNulls(List<byte> list, int n)
        {
            for (int i = 0; i < n; i++)
            {
                list.Add(Ox00);
            }
        }

        private void PrepareFileHeader()
        {
            header.Clear();
            header.Add(SIMPLE_TABLE);
            DateTime date = DateTime.Now;
            int year = date.Year % 100;
            header.Add((byte)year);
            header.Add((byte)date.Month);
            header.Add((byte)date.Day);
            byte[] bytes = BitConverter.GetBytes(records.Count);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            header.AddRange(bytes);
            bytes = BitConverter.GetBytes((short)GetHeaderSize());
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            header.AddRange(bytes);
            bytes = BitConverter.GetBytes((short)(GetRecordSize()));
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            header.AddRange(bytes);
            CompleteByteListNulls(header, HEADER_NULL_FILL);
        }

        private void PrepareFieldNames()
        {
            if (FLoadFieldNamesFromFile)
            {
                StreamReader reader = File.OpenText(FFieldNamesFileName);
                StringBuilder name;
                for (int i = 0; i < maxFieldsSize.Count; i++)
                {
                    string line = reader.ReadLine();
                    if (line != null)
                    {
                        name = new StringBuilder(line);
                        name.Replace(SPACE, UNDERSCORE);
                        if (name.Length > Record.MAX_FIELD_NAME_CHARS)
                        {
                            name.Remove(Record.MAX_FIELD_NAME_CHARS, name.Length - Record.MAX_FIELD_NAME_CHARS);
                        }
                    }
                    else
                    {
                        name = new StringBuilder(DEFAULT_FIELD_NAME);
                        name.Append(i.ToString());
                    }
                    fieldNames.Add(name);
                }
                reader.Close();
            }
            else
            {
                for (int i = 0; i < maxFieldsSize.Count; i++)
                {
                    StringBuilder name = new StringBuilder(DEFAULT_FIELD_NAME);
                    name.Append(i.ToString());
                    fieldNames.Add(name);
                }
            }
        }

        private void PrepareFieldHeaders()
        {
            StringBuilder fieldName = new StringBuilder();
            for (int i = 0; i < maxFieldsSize.Count; i++)
            {
                fieldName = fieldNames[i];
                byte[] bytes = FEncoding.GetBytes(fieldName.ToString());
                header.AddRange(bytes);
                CompleteByteListNulls(header, Record.MAX_FIELD_NAME_CHARS - fieldName.Length + 1);
                header.Add((byte)FIELD_TYPE);
                CompleteByteListNulls(header, 4);
                header.Add((byte)maxFieldsSize[i]);
                CompleteByteListNulls(header, FIELD_NULL_FILL);
            }
            header.Add(FILE_HEADER_END);
        }

        private void WriteHeader(Stream stream)
        {
            byte[] bytes = header.ToArray();
            stream.Write(bytes, 0, header.Count);
        }

        private void WriteRecords(Stream stream)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Record rec in records)
            {
                for (int i = 0; i < rec.Count; i++)
                {
                    builder.Append(rec[i]);
                }
                stream.WriteByte(RECORD_NOT_REMOVED);
                byte[] bytes = FEncoding.GetBytes(builder.ToString());
                stream.Write(bytes, 0, bytes.Length);
                builder.Remove(0, builder.Length);
            }
        }

        private void Write(Stream stream)
        {
            PrepareRecords();
            CompleteRecordsEmptyFields();
            PrepareFileHeader();
            PrepareFieldNames();
            PrepareFieldHeaders();
            WriteHeader(Stream);
            WriteRecords(Stream);
            Stream.WriteByte(DBF_FILE_END);
        }

        #endregion // Private Methods

        #region Protected Methods

        /// <inheritdoc/>
        protected override void Start()
        {
            FMatrix = new ExportMatrix();
            FMatrix.Inaccuracy = 0.5f;
            FMatrix.PlainRich = true;
            FMatrix.AreaFill = false;
            FMatrix.CropAreaFill = true;
            FMatrix.Report = Report;
            FMatrix.Images = false;
            FMatrix.WrapText = false;
            FMatrix.DataOnly = FDataOnly;
            FMatrix.ShowProgress = ShowProgress;
        }

        /// <inheritdoc/>
        protected override void ExportPage(int pageNo)
        {
            using (ReportPage page = GetPage(pageNo))
            {
                FMatrix.AddPage(page);
            }
        }

        /// <inheritdoc/>
        protected override void Finish()
        {
            FMatrix.Prepare();
            Write(Stream);
        }

        /// <inheritdoc/>
        protected override string GetFileFilter()
        {
            return new MyRes("FileFilters").Get("DbfFile");
        }

        #endregion // Protected Methods

        #region Public Methods

        /// <inheritdoc/>
        public override bool ShowDialog()
        {
            using (DbfExportForm form = new DbfExportForm())
            {
                form.Init(this);
                return form.ShowDialog() == DialogResult.OK;
            }
        }

        #endregion // Public Methods

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DBFExport"/> class.
        /// </summary>
        public DBFExport()
        {
            FEncoding = Encoding.Default;
            FDataOnly = false;
            header = new List<byte>();
            field = new StringBuilder("");
            record = new Record();
            records = new List<Record>();
            maxFieldsSize = new List<int>();
            fieldNames = new List<StringBuilder>();
            FLoadFieldNamesFromFile = false;
            FFieldNamesFileName = "";
        }

        #endregion // Constructors
    }
}