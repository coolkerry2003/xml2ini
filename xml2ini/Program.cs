using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace xml2ini
{
    class Program
    {
        static void Main(string[] args)
        {
            string inipath = @"C:\Users\User\Desktop\地籍圖處理\landgen.ini";
            IniManager inimanager = new IniManager(inipath);

            string xmlpath = @"C:\Users\User\Desktop\地籍圖處理\110.10.15_以全方位轉出都市計畫圖為主套合地籍圖之轉換參數CD_SECT.xml";
            XMLRecord xr = new XMLRecord(xmlpath);
            List<string> datas = xr.Read("DATA");

            foreach(string data in datas)
            {
                string[] values = data.Split(',');
                string paintCode = values[0];
                string P4S = values[2];
                string P4R = values[3];
                string P4X = values[4];
                string P4Y = values[7];
                string P4C_X = values[13];
                string P4C_Y = values[14];


                string sectname = $"PAINT-CD{paintCode}";
                string[] keys = inimanager.GetKeys(sectname);
                if(keys.Length <= 0)
                {
                    Console.WriteLine($"{paintCode,5} > 略");
                }
                else
                {
                    inimanager.WriteIniFile(sectname, "P4S", P4S);
                    inimanager.WriteIniFile(sectname, "P4R", P4R);
                    inimanager.WriteIniFile(sectname, "P4X", P4X);
                    inimanager.WriteIniFile(sectname, "P4Y", P4Y);
                    inimanager.WriteIniFile(sectname, "P4C", $"{P4C_X},{P4C_Y}");
                    Console.WriteLine($"{paintCode,5} > P4S:[{P4S}], P4R:[{P4R}], P4X:[{P4X}], P4Y:[{P4Y}], P4C_X:[{P4C_X}], P4C_Y:[{P4C_Y}]");
                }
            }
            Console.WriteLine("完成！");
            Console.ReadKey();
        }
    }
    class XMLRecord
    {
        private string mPath = string.Empty;
        public XMLRecord(string aPath)
        {
            mPath = aPath;
        }
        public List<string> Read(string aName)
        {
            List<string> rtn = new List<string>();
            using (XmlReader reader = XmlReader.Create(mPath))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        if(reader.Name.ToString() == aName)
                        {
                            rtn.Add(reader.ReadString());
                        }
                    }
                }
            }
            return rtn;
        }
    }
}
