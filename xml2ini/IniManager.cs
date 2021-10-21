using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace xml2ini
{
    class IniSaver
    {
        List<IniSession> m_Sessions = new List<IniSession>();
        /// <summary>
        /// 加入區段
        /// </summary>
        /// <param name="_Session"></param>
        public void AddSession(IniSession _Session)
        {
            m_Sessions.Add(_Session);
        }
        /// <summary>
        /// 儲存ini檔
        /// </summary>
        /// <returns></returns>
        public bool Save(string _Path)
        {
            bool Rtn = false;
            string text;

            //清空
            File.WriteAllText(_Path, string.Empty, Encoding.Default);
            FileStream fs = new FileStream(_Path, FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);

            //開始寫入區段
            foreach (IniSession ses in m_Sessions)
            {
                //註解
                text = ses.GetRemark();
                if (!string.IsNullOrEmpty(text))
                {
                    sw.WriteLine(text);
                }

                //區段名稱
                text = string.Format("[{0}]", ses.GetName());
                sw.WriteLine(text);

                //KeyValue
                foreach (KeyValuePair kvp in ses.m_Kvps)
                {
                    text = string.Format("{0}={1}", kvp.Key, kvp.Value);
                    sw.WriteLine(text);
                }
                sw.WriteLine("");
            }
            sw.Close();

            Rtn = true;
            return Rtn;
        }
    }
    class IniSession
    {
        private string m_SectionName = string.Empty;
        private string m_Remark = string.Empty;
        public List<KeyValuePair> m_Kvps = new List<KeyValuePair>();
        /// <summary>
        /// 設定區段名稱
        /// </summary>
        /// <param name="_Name"></param>
        public void SetName(string _Name)
        {
            this.m_SectionName = _Name;
        }
        /// <summary>
        /// 取得區段名稱
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return m_SectionName;
        }
        /// <summary>
        /// 加入KeyValue
        /// </summary>
        /// <param name="_Key"></param>
        /// <param name="_Value"></param>
        public void AddData(string _Key, string _Value)
        {
            m_Kvps.Add(new KeyValuePair(_Key, _Value));
        }
        /// <summary>
        /// 設註解
        /// </summary>
        /// <param name="_Remark"></param>
        public void SetRemark(string _Remark)
        {
            m_Remark = _Remark;
        }
        /// <summary>
        /// 取註解
        /// </summary>
        /// <returns></returns>
        public string GetRemark()
        {
            return m_Remark;
        }
    }
    class IniManager
    {
        private string filePath;
        private StringBuilder lpReturnedString;
        private int bufferSize;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="iniPath"></param>
        public IniManager(string iniPath)
        {
            filePath = iniPath;
            bufferSize = 512;
            lpReturnedString = new StringBuilder(bufferSize);
        }
        /// <summary>
        /// 讀取Key
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string ReadIniFile(string section, string key, string defaultValue)
        {
            lpReturnedString.Clear();
            GetPrivateProfileString(section, key, defaultValue, lpReturnedString, bufferSize, filePath);
            return lpReturnedString.ToString();
        }
        /// <summary>
        /// 寫入Key
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void WriteIniFile(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, filePath);
        }
        /// <summary>
        /// 取得區段下所有Key值
        /// </summary>
        /// <param name="_section"></param>
        /// <returns></returns>
        public string[] GetKeys(string _section)
        {
            byte[] buffer = new byte[2048];

            GetPrivateProfileSection(_section, buffer, 2048, filePath);
            String[] tmp = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0');

            List<string> result = new List<string>();
            foreach (String entry in tmp)
            {
                //篩選標註#==
                if (!entry.Contains("#="))
                {
                    int index = entry.IndexOf("=");
                    if (index != -1)
                    {
                        string key = entry.Substring(0, index);
                        result.Add(key);
                    }
                }
            }
            return result.ToArray();
        }
        /// <summary>
        /// 取得KeyValuePairs
        /// </summary>
        /// <param name="_section"></param>
        /// <returns></returns>
        public List<KeyValuePair> GetKeyValuePairs(string _section)
        {
            byte[] buffer = new byte[2048];

            GetPrivateProfileSection(_section, buffer, 2048, filePath);
            String[] tmp = Encoding.Default.GetString(buffer).Trim('\0').Split('\0');

            List<KeyValuePair> Rtn = new List<KeyValuePair>();
            foreach (String entry in tmp)
            {
                //篩選標註#==
                if (!entry.Contains("#="))
                {
                    string[] kv = entry.Split('=');
                    if (kv.Length == 2)
                    {
                        Rtn.Add(new KeyValuePair(kv[0], kv[1]));
                    }
                }
            }
            return Rtn;
        }
        /// <summary>
        /// 寫入重複Key值
        /// </summary>
        /// <param name="_section"></param>
        /// <param name="_kps"></param>
        public void WriteIniFileDuplicate(string _section, List<KeyValuePair> _kps)
        {
            List<string> colStrs = new List<string>();
            List<KeyValuePair> orKps = GetKeyValuePairs(_section);
            StringBuilder values = new StringBuilder();

            //彙整原Key值，並篩掉重複的值
            foreach (KeyValuePair orKp in orKps)
            {
                colStrs.Add(orKp.Key + "/" + orKp.Value);
            }
            foreach (KeyValuePair _kp in _kps)
            {
                colStrs.Add(_kp.Key + "/" + _kp.Value);
            }
            colStrs = colStrs.Distinct().ToList();

            //串接字串
            int i = 0;
            foreach (string colStr in colStrs)
            {
                string[] KeyValue = colStr.Split('/');
                string key = KeyValue[0];
                string val = KeyValue[1];

                string value = (i == 0) ? val : string.Format("\n{0}={1}", key, val);
                values.Append(value);
                i++;
            }

            //預先清空Section
            WriteIniFile(_section, null, null);
            //寫入所有Key值
            if (colStrs.Count > 0)
            {
                string mainKey = colStrs[0].Split('/')[0];
                WriteIniFile(_section, mainKey, values.ToString());
            }
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string lpString, string lpFileName);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);
    }
    class KeyValuePair
    {
        public string Key = string.Empty;
        public string Value = string.Empty;
        public KeyValuePair(string _Key, string _Value)
        {
            this.Key = _Key;
            this.Value = _Value;
        }
    }
}
