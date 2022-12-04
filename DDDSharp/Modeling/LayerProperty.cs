using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace DataCollection
{
    public class LayerProperty
    {
        public string LayerName { get; set; } = "";
        public double LayerValue { get; set; } = 0;
        public Color LayerColor { get; set; } = Color.Black;
        public LayerProperty(string name,double value = 0)
        {
            LayerName = name;
            LayerValue = value;
            LayerColor = Color.Black;
        }
        public static List<LayerProperty> Import(string filename)
        {
            List<LayerProperty> layers = new List<LayerProperty>();
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open);
                StreamReader sr = new StreamReader(fs);

                string header = "", line = "";                
                //seek header
                while ( (header = sr.ReadLine()) != null )
                {
                    header = header.Trim().ToUpper();                  
                    if (header.Length < 1) continue;
                    if (header[0] == '/' || header[0] == '!' || header[0] == '#') continue;
                    if (header != "LAYERS PROPERTY") continue;
                    break;
                }

                //read lines
                string[] ss;
                string name;
                double value;
                int r, g, b;

                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim(' ');
                    if (line.Length < 1) continue;
                    if (line[0] == '/' || line[0] == '!' || line[0] == '#') continue;

                    ss = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if ( ss.Length != 5 ) continue;

                    name = ss[0].Trim();                    
                    if (!double.TryParse(ss[1], out value)) continue;

                    if ( !int.TryParse(ss[2], out r) ) continue;
                    if ( !int.TryParse(ss[3], out g) ) continue;
                    if ( !int.TryParse(ss[4], out b) ) continue;
                    
                    LayerProperty layer = new LayerProperty(name, value);
                    layer.LayerColor = Color.FromArgb(r, g, b);
                    layers.Add(layer);

                    ss = null;
                }

                sr.Close();
                fs.Close();                
            }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
            catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
            {
                layers.Clear();
            }
            return layers;
        }
        static public bool Export(string file, List<LayerProperty>layers)
        {
            try
            {
                FileStream fs = new FileStream(file, FileMode.Create);
                StreamWriter wr = new StreamWriter(fs);

                string header = "LAYERS PROPERTY";
                wr.WriteLine(header);
                string line = "";
                foreach (LayerProperty layer in layers)
                {
                    line = layer.LayerName + "," + layer.LayerValue + ",";
                    line += layer.LayerColor.R + ",";
                    line += layer.LayerColor.G + ",";
                    line += layer.LayerColor.B;
                    wr.WriteLine(line);                    
                }
                
                wr.Close();
                fs.Close();
                return true;
            }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
            catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
            {
                return false;
            }
        }
    }
}
