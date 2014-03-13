using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotBitNs
{
    public class FileLogger : TextWriter, IDisposable
    {

        private bool _Enabled;
        private bool _shouldTruncate;
        public bool Enabled
        {
          get { return _Enabled; }
          set 
          {
              if (_Enabled != value)
              {
                  if (value)
                  {
                      _Enabled = true;
                      using (StreamWriter writer = new StreamWriter(new FileStream(m_FileName, !_shouldTruncate ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read)))
                      {
                          writer.WriteLine(">>>Logging started on {0}.", DateTime.Now.ToString("f")); //f = Tuesday, April 10, 2001 3:51 PM 
                      }
                      m_NewLine = true;
                      _shouldTruncate = false;
                  }
                  else
                  {
                      using (StreamWriter writer = new StreamWriter(new FileStream(m_FileName, FileMode.Append, FileAccess.Write, FileShare.Read)))
                      {
                          writer.WriteLine(">>>Logging ended on {0}.", DateTime.Now.ToString("f")); //f = Tuesday, April 10, 2001 3:51 PM 
                      }
                      m_NewLine = true;
                      _Enabled = false;
                  }
              }
          }
        }

        private string m_FileName;
        private bool m_NewLine;
        public const string DateFormat = "[MMM d HH:mm:ss]: ";

        public string FileName { get { return m_FileName; } }

        public FileLogger(string file, string path = null, bool append = false, bool enabled = true)
        {
            if (!string.IsNullOrWhiteSpace(path))
                m_FileName = Path.Combine(path, file);
            else
                m_FileName = file;

            _shouldTruncate = !append;
            Enabled = enabled;
        }

        public override void Write(char ch)
        {
            if (Enabled)
                using (StreamWriter writer = new StreamWriter(new FileStream(m_FileName, FileMode.Append, FileAccess.Write, FileShare.Read)))
                {
                    if (m_NewLine)
                    {
                        writer.Write(DateTime.Now.ToString(DateFormat));
                        m_NewLine = false;
                    }
                    writer.Write(ch);
                }
        }

        public override void Write(string str)
        {
            if (Enabled)
                using (StreamWriter writer = new StreamWriter(new FileStream(m_FileName, FileMode.Append, FileAccess.Write, FileShare.Read)))
                {
                    if (m_NewLine)
                    {
                        writer.Write(DateTime.Now.ToString(DateFormat));
                        m_NewLine = false;
                    }
                    writer.Write(str);
                }
        }

        public override void WriteLine(string line)
        {
            if (Enabled)
                using (StreamWriter writer = new StreamWriter(new FileStream(m_FileName, FileMode.Append, FileAccess.Write, FileShare.Read)))
                {
                    if (m_NewLine)
                        writer.Write(DateTime.Now.ToString(DateFormat));
                    writer.WriteLine(line);
                    m_NewLine = true;
                }
        }

        public override System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.Default; }
        }
    }

    public class MultiTextWriter : TextWriter
    {
        private List<TextWriter> m_Streams;

        public MultiTextWriter(params TextWriter[] streams)
        {
            m_Streams = new List<TextWriter>(streams);

            if (m_Streams.Count < 0)
                throw new ArgumentException("You must specify at least one stream.");
        }

        public void Add(TextWriter tw)
        {
            m_Streams.Add(tw);
        }

        public void Remove(TextWriter tw)
        {
            m_Streams.Remove(tw);
        }

        public override void Write(char ch)
        {
            for (int i = 0; i < m_Streams.Count; i++)
                m_Streams[i].Write(ch);
        }

        public override void WriteLine(string line)
        {
            for (int i = 0; i < m_Streams.Count; i++)
                m_Streams[i].WriteLine(line);
        }

        public override void WriteLine(string line, params object[] args)
        {
            WriteLine(String.Format(line, args));
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }
}
