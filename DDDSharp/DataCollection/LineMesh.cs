using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//mesh consist of a serials lines 
namespace DataCollection
{
    public class LineMesh: C3DObjectBase
    {
        public List<C3DLine> lines = new List<C3DLine>();
        public int lineNum { get { return lines.Count; } }              
        public int pointsNum
        {
            get
            {
                if (lines.Count < 1) return 0;
                else return lines[0].points.Count;
            }
        }

        public LineMesh()
        {
            type = ShapeEnum.LineMesh;
        }
        public LineMesh(string _name)
        {
            type = ShapeEnum.LineMesh;
            Name = _name;
        }
        public override bool Remove(C3DObjectBase obj)
        {
            return lines.Remove((C3DLine)obj);
        }
        public override void ClearRenderingBuffers()
        {
            foreach (C3DLine line in lines)
            {
                line.ClearRenderingBuffers();
            }
            RenderingBuffers.Clear();
        }
        public void Add(C3DLine line)
        {
            lines.Add(line);
        }
        int MinimumPointNum 
        {
            get 
            {
                int num = 9999999;
                foreach(C3DLine line in lines)
                {
                    if (line.points.Count < num) 
                        num = line.points.Count;
                }
                return num;
            }
        }
        //曲线方向校正，以第1条曲线为准
        private void ReviseLinesDirection()
        {
            C3DLine line1 = lines[0],line2;
            Vector64 p11, p12, p21,p22;
            for (int i = 1; i < lines.Count; i++ )
            {
                line2 = lines[i];
                p11 = line1.points[0];
                p12 = line1.points[line1.Count-1];
                p21 = line2.points[0];
                p22 = line2.points[line2.Count - 1];
                double dist1 = p11.Distance(p21) + p12.Distance(p22);
                double dist2 = p11.Distance(p22) + p12.Distance(p21);
                if ( dist1 > dist2 )
                {
                    line2.OrderInverse();
                    lines[i] = line2;
                }                
                line1 = line2;
            }
        }

        /// <summary>
        /// 创建垂直方向的LineMesh
        /// </summary>
        /// <param name="ptNum">曲线重采样点数</param>
        /// <param name="smoothed">是否要平滑</param>
        /// <returns></returns>
        //  1-2-3-4-5-6-7-8-9-  num  
        //  1-2-3-4-5-6-7-8-9-
        //  1-2-3-4-5-6-7-8-9-
        //  1-2-3-4-5-6-7-8-9-
        //  1-2-3-4-5-6-7-8-9-
        //ptNum曲线采样点数
        public LineMesh CreateVerticalLineMesh(int ptNum, bool smoothed = false)
        {
            LineMesh linemesh = new LineMesh();
            if (lines.Count < 2) return null;

            //ReviseLinesDirection();            
            int num = lines[0].Count;
            C3DLine line;
            for ( int ix = 0; ix < num; ix++ ) //
            {
                //纵向曲线
                C3DLine newline = new C3DLine(ix.ToString()) ;
                for (int i = 0; i < lines.Count; i++)//横向曲线
                {
                    line = lines[i];
                    if (line.Count != num) line = line.Resample(num);
                    newline.AddPoint(line[ix]);
                }
                
                if (smoothed) newline = newline.Smooth();
                linemesh.Add(newline.Resample(ptNum));//重采样
            }            
            linemesh.UpdateRange();
            return linemesh;
        }        
        
        /// <summary>
        /// 创建网格化的曲面
        /// </summary>
        /// <returns>horGridNum沿着线方向的网格数，垂直于线方向的网格数</returns>
        public CMesh CreateGridMesh(int horGridNum=101,int verGridNum=101,bool smoothed = false)
        {
            if ( lines.Count < 2 ) return null;

            //曲线方向校正
             ReviseLinesDirection();
            
             //先做横向曲线平滑
             LineMesh linemesh = new LineMesh();
            for (int i = 0; i < lines.Count; i++)
            {
                linemesh.Add(lines[i].Resample(horGridNum) );
            }

            //再做纵向抽取平滑
            LineMesh linemesh1 = linemesh.CreateVerticalLineMesh(verGridNum, smoothed);

            linemesh.Clear();

            //lines:0  1  2  3  4  5  6  7
            //      0--1--2--3--4--5--6--7-- hor
            //      |  |  |  |  |  |  |  | 
            //      0--1--2--3--4--5--6--7-----
            //      |  |  |  |  |  |  |  | 
            //      0--1--2--3--4--5--6--7-----
            //      |  |  |  |  |  |  |  | 
            //      0--1--2--3--4--5--6--7-----
            //创建Mesh网格
            C3DLine line;
            CMesh mesh = new CMesh(horGridNum,verGridNum);
            mesh.Name = Name + "_GridMesh";

            for (int i = 0; i < linemesh1.lines.Count; i++)//horGridNum
            {
                line = linemesh1.lines[i];
                for (int j = 0; j < verGridNum; j++)
                {
                    mesh.AddPoint(i, j, line[j]);
                }
            }            
            mesh.UpdateRange();
            linemesh1.Clear();
            return mesh;     
        }
        /*
        //对曲线进行插值，剖分网格数目，沿线方向剖分数lineDivNum,
        //沿测线走向剖分间隔数IntervalNum
        int Interpolate( int horNum = 100,int vertNum = 100 )
        {
            for(int i=0;i<lines.Count;i++)
            {
                lines[i].LineDivid(horNum);
            }

            List<C3DLine> outLine = new List<C3DLine>();

            Vector32[] pt = new Vector32[lines.Count];
            List<Vector64> vertline;
            for (int i = 0; i < horNum + 1;i++)
            {
                for (int j = 0; j < lines.Count; j++)
                    pt[j] = lines[j].points[i];

                Spline sp = new Spline(pt);
                vertline = sp.CreateSpline();

                C3DLine line1 = new C3DLine(vertline);
                line1.LineDivid( vertNum -1 );

                outLine.Add(line1);

                vertline.Clear();
                sp.Clear();
            }

            pt = null;

            lines.Clear();

            lines = outLine;

            return lines.Count;
        }
        */
        public override void UpdateRange()
        {
            minx = maxx = 0;
            miny = maxy = 0;
            minz = maxz = 0;
            minv = maxv = 0;
            C3DLine line;
            for(int i=0;i<lineNum;i++)
            {
                line = lines[i];
                if (i == 0)
                {
                    minx = line.minx;
                    maxx = line.maxx;
                    miny = line.miny;
                    maxy = line.maxy;
                    minz = line.minz;
                    maxz = line.maxz;
                    minv = line.minv;
                    maxv = line.maxv;
                }
                else
                {
                    if (line.minx < minx) minx = line.minx;
                    if (line.maxx > maxx) maxx = line.maxx;
                    if (line.miny < miny) miny = line.miny;
                    if (line.maxy > maxy) maxy = line.maxy;
                    if (line.minz < minz) minz = line.minz;
                    if (line.maxz > maxz) maxz = line.maxz;
                    if (line.minv < minv) minv = line.minv;
                    if (line.maxv > maxv) maxv = line.maxv;
                }
            }
        }
    }
}
