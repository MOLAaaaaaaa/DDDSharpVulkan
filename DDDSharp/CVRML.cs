using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DataCollection
{
    public class VRML
    {
        public VRML()
        {

        }
        static public bool ExportVRMLHeader(StreamWriter wr)
        {
            string line = "";
            line = "#VRML V2.0 utf8"; wr.WriteLine(line);
            line = "#-----------------"; wr.WriteLine(line);
            line = "# Geological VRML Model"; wr.WriteLine(line);
            line = "# Created by 3D Surfer"; wr.WriteLine(line);
            line = "# " + DateTime.Now.ToString(); wr.WriteLine(line);
            line = "#-----------------"; wr.WriteLine(line);
            return true;
        }
        static public bool ExportVRMLSky(StreamWriter wr)
        {
            string line = "";
            line = "Background{"; wr.WriteLine(line);
            line = "skyAngle[ 1.309 1.571 ]"; wr.WriteLine(line);
            line = "skyColor["; wr.WriteLine(line);
            line = "1.0 1.0 1.0"; wr.WriteLine(line);
            line = "0.2 0.2 1.0"; wr.WriteLine(line);
            line = "1.0 1.0 1.0]}"; wr.WriteLine(line);
            return true;
        }
        static public bool ExportVRML(string filename, List<C3DObjectBase> objects)
        {
            int n = objects.Count;
            if (n < 1) return false;

            FileStream fs = new FileStream(filename, FileMode.Create);
            StreamWriter wr = new StreamWriter(fs);

            ExportVRMLHeader(wr);
            ExportVRMLSky(wr);

            string line;
            line = "Group"; wr.WriteLine(line);
            line = "{"; wr.WriteLine(line);
            line = "    children["; wr.WriteLine(line);

            foreach (C3DObjectBase obj in objects)
            {
                obj.ExportVRML(wr);
            }

            line = "    ]#end children"; wr.WriteLine(line);
            line = "}#end Group"; wr.WriteLine(line);

            wr.Close();
            fs.Close();
            return true;
        }

        static public bool ExportGridIsosurface(C3DGridData data, string filename)
        {
            CISOSurfaceExt iso = data.m_MarchCubeExt.pISOSurfaceExt;
            int np = iso.pCoordArray.Count;
            int ip = iso.pTriangleIndex.Count;
            if (np < 3 || ip < 1) return false;

            string line;
            FileStream fs = new FileStream(filename, FileMode.Create);
            StreamWriter wr = new StreamWriter(fs);

            line = "# VRML V2.0 utf8"; wr.WriteLine(line);
            line = "#-----------------"; wr.WriteLine(line);
            line = "# Geological VRML Model"; wr.WriteLine(line);
            line = "# Created by Jiansir"; wr.WriteLine(line);
            line = "#-----------------"; wr.WriteLine(line);
            line = "Shape"; wr.WriteLine(line);
            line = "{"; wr.WriteLine(line);
            line = "    geometry IndexedFaceSet"; wr.WriteLine(line);
            line = "    { "; wr.WriteLine(line);
            line = "       coord Coordinate{"; wr.WriteLine(line);
            line = "          point["; wr.WriteLine(line);
            line = "          ";
            FLOAT_POINT_EXT p;
            for (int i = 0; i < np; i++)
            {
                p = iso.pCoordArray[i];
                if (i % 50 == 0)
                {
                    wr.WriteLine(line);
                    line = "          ";
                }
                line += (data.scale.x * p.x + " ");
                line += (data.scale.y * p.y + " ");
                line += data.scale.z * p.z;
                if (i < np - 1) line += ",";
            }
            //the last row
            wr.WriteLine(line);
            line = "          ]#end of points"; wr.WriteLine(line);
            line = "        }#end of Coordinate"; wr.WriteLine(line);
            line = "       # ----------begin coordIndex---------------"; wr.WriteLine(line);
            line = "       coordIndex["; wr.WriteLine(line);
            line = "         ";
            for (int i = 0; i < ip / 3; i++)
            {
                if (i % 50 == 0)
                {
                    wr.WriteLine(line);
                    line = "         ";
                }
                line += (iso.pTriangleIndex[3 * i] + ",");
                line += (iso.pTriangleIndex[3 * i + 1] + ",");
                line += (iso.pTriangleIndex[3 * i + 2] + ",");
                line += "-1";
                if (i < (ip / 3) - 1) line += ",";
            }
            //the last row
            wr.WriteLine(line);
            line = "       ]#end coordIndex"; wr.WriteLine(line);
            line = "       color Color"; wr.WriteLine(line);
            line = "       {"; wr.WriteLine(line);
            line = "          color["; wr.WriteLine(line);
            line = "                 ";
            ColorRGBA cc;
            double r, g, b;
            for (int i = 0; i < np; i++)
            {
                if (i % 50 == 0)
                {
                    wr.WriteLine(line);
                    line = "                 ";
                }
                p = iso.pCoordArray[i];
                cc = iso.pColor[p.icolor];
                r = Math.Round(cc.R / 255.0, 2);
                g = Math.Round(cc.G / 255.0, 2);
                b = Math.Round(cc.B / 255.0, 2);
                line += (r + " ");
                line += (g + " ");
                line += (b + " ");
                if (i < np - 1) line += ",";
            }
            wr.WriteLine(line);
            line = "               ]#end of color[]"; wr.WriteLine(line);
            line = "       }#end of color"; wr.WriteLine(line);
            line = "       normalPerVertex TRUE "; wr.WriteLine(line);
            line = "       colorPerVertex TRUE "; wr.WriteLine(line);
            line = "       solid TRUE "; wr.WriteLine(line);
            line = "     }#end of geometry"; wr.WriteLine(line);
            line = "}#end of Shape"; wr.WriteLine(line);

            wr.Close();
            fs.Close();
            return true;
        }
    }
}
