using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCollection
{    
    //封闭多边形平滑
    public class BezierSmooth: C3DLine
    {
        //https://blog.csdn.net/microchenhong/article/details/6316332
        //原文地址：http://www.antigrain.com/research/ 
        //bezier_interpolation/index.html#PAGE_BEZIER_INTERPOLATION 
        public double Step = 0.05;
#pragma warning disable CS0108 // '“BezierSmooth.Smooth()”隐藏继承的成员“C3DLine.Smooth()”。如果是有意隐藏，请使用关键字 new。
        public C3DLine Smooth()
#pragma warning restore CS0108 // '“BezierSmooth.Smooth()”隐藏继承的成员“C3DLine.Smooth()”。如果是有意隐藏，请使用关键字 new。
        {
            C3DLine outLine = new C3DLine();
            //控制点收缩系数 ，经调试0.6较好
            double scale = 0.6;
            int nexti, backi, extraindex, extranexti;
            double offsetx, offsety, addx, addy;            
            Vector64 []midpoints = new Vector64[Count];
            //生成中点     
            for (int i = 0; i < Count; i++)
            {
                nexti = (i + 1) % Count; //1,2,3,4,5,6,...Count-1,0
                                         //0,1,2,3,4,5,....8    ,9
                midpoints[i].x = (points[i].x + points[nexti].x) / 2.0;
                midpoints[i].y = (points[i].y + points[nexti].y) / 2.0;
            }

            //平移中点
            Vector64 midinmid = new Vector64();
            Vector64[] extrapoints = new Vector64[2 * Count];
            for (int i = 0; i < Count; i++)
            {
                nexti = (i + 1) % Count;         //1,2,3,4,5,6,...9,0
                backi = (i + Count - 1) % Count; //9,0,1,2,3,4,...7,8               
                midinmid.x = (midpoints[i].x + midpoints[backi].x) / 2.0;
                midinmid.y = (midpoints[i].y + midpoints[backi].y) / 2.0;
                offsetx = points[i].x - midinmid.x;
                offsety = points[i].y - midinmid.y;
                extraindex = 2 * i;
                extrapoints[extraindex].x = midpoints[backi].x + offsetx;
                extrapoints[extraindex].y = midpoints[backi].y + offsety;
                //朝 points[i]方向收缩 
                addx = (extrapoints[extraindex].x - points[i].x) * scale;
                addy = (extrapoints[extraindex].y - points[i].y) * scale;
                extrapoints[extraindex].x = points[i].x + addx;
                extrapoints[extraindex].y = points[i].y + addy;
                extranexti = (extraindex + 1) % (2 * Count);
                extrapoints[extranexti].x = midpoints[i].x + offsetx;
                extrapoints[extranexti].y = midpoints[i].y + offsety;
                //朝 points[i]方向收缩 
                addx = (extrapoints[extranexti].x - points[i].x) * scale;
                addy = (extrapoints[extranexti].y - points[i].y) * scale;
                extrapoints[extranexti].x = points[i].x + addx;
                extrapoints[extranexti].y = points[i].y + addy;
            }

            Vector64 []controlPoint = new Vector64[4];
            //生成4控制点，产生贝塞尔曲线
            double u;
            for (int i = 0; i < Count; i++)
            {
                controlPoint[0] = points[i];
                extraindex = 2 * i;
                controlPoint[1] = extrapoints[extraindex + 1];
                extranexti = (extraindex + 2) % (2 * Count);
                controlPoint[2] = extrapoints[extranexti];
                nexti = (i + 1) % Count;
                controlPoint[3] = points[nexti];
                u = 1;                
                while (u >= 0)
                {
                    double px = bezier3funcX(u, controlPoint);
                    double py = bezier3funcY(u, controlPoint);
                    //u的步长决定曲线的疏密
                    u -= Step;                    
                    //存入曲线点 
                    outLine.AddPoint(px,py,points[i].z);                    
                }
            }
            midpoints = null;
            extrapoints = null;
            return outLine;
        }
        //三次贝塞尔曲线
        double bezier3funcX(double uu, Vector64 []controlP)
        {
            double part0 = controlP[0].x * uu * uu * uu;
            double part1 = 3 * controlP[1].x * uu * uu * (1 - uu);
            double part2 = 3 * controlP[2].x * uu * (1 - uu) * (1 - uu);
            double part3 = controlP[3].x * (1 - uu) * (1 - uu) * (1 - uu);
            return part0 + part1 + part2 + part3;
        }
        double bezier3funcY(double uu, Vector64[] controlP)
        {
            double part0 = controlP[0].y * uu * uu * uu;
            double part1 = 3 * controlP[1].y * uu * uu * (1 - uu);
            double part2 = 3 * controlP[2].y * uu * (1 - uu) * (1 - uu);
            double part3 = controlP[3].y * (1 - uu) * (1 - uu) * (1 - uu);
            return part0 + part1 + part2 + part3;
        }
    }
    public class CurveFit
    {
        //最小二乘拟合相关函数定义
        /*
        double sum(vector Vnum, int n);
        double MutilSum(vector Vx, vector Vy, int n);
        double RelatePow(vector Vx, int n, int ex);
        double RelateMutiXY(vector Vx, vector Vy, int n, int ex);
        void EMatrix(vector Vx, vector Vy, int n, int ex, double coefficient[]);
        void CalEquation(int exp, double coefficient[]);
        double F(double c[], int l, int m);
        */
        public CurveFit(List<Vector64> inPoints)
        {
            points = inPoints;
        }
        public CurveFit()
        {
            
        }
        List<Vector64> points = new List<Vector64>();
        double [,]Em = new double[6,5];
        public void AddPoint(double x,double y,double z = 0)
        {
            points.Add(new Vector64(x, y, z));
        }
        public void AddPoint(Vector64 p)
        {
            points.Add(p);
        }
        public void Clear()
        {
            points.Clear();
        }
        public List<Vector64> Fit()
        {
            double []coefficient = new double[points.Count];
            for (int i = 0; i < coefficient.Length; i++) 
                coefficient[i] = 0;
            EMatrix(3, coefficient);
            //y = c0 + c1*x + c2 * x^2
            double x,y,z = 0;
            List<Vector64> lists = new List<Vector64>();
            foreach(Vector64 p in points)
            {
                x = p.x;
                y = coefficient[0] + coefficient[1] * x + coefficient[2] * x * x;
                lists.Add(new Vector64(x,y,z));
            }
            coefficient = null;
            points.Clear();
            return lists;
        }
       
        //乘积和
        double MutilSum()
        {
            double dMultiSum = 0;
            double x, y;
            for (int i = 0; i < points.Count; i++)
            {
                x = points[i].x;
                y = points[i].y;
                dMultiSum += x*y;
            }
            return dMultiSum;
        }
        //ex次方和
        double RelatePow(int ex)
        {
            double ReSum = 0;
            foreach (Vector64 p in points)
            {
                ReSum += Math.Pow(p.x, ex);
            }
            return ReSum;
        }
        //x的ex次方与y的乘积的累加
        double RelateMutiXY(int ex)
        {
            double dReMultiSum = 0;
            foreach (Vector64 p in points)
            {
                dReMultiSum += Math.Pow( p.x, ex ) * p.y;
            }
            return dReMultiSum;
        }
        //计算方程组的增广矩阵
        void EMatrix(int ex, double []coefficient)
        {
            for (int i = 1; i <= ex; i++)
            {
                for (int j = 1; j <= ex; j++)
                {
                    Em[i,j] = RelatePow(i + j - 2);
                }
                Em[i,ex + 1] = RelateMutiXY(i - 1);
            }
            Em[1,1] = points.Count;
            CalEquation(ex, coefficient);
        }
        //求解方程
        void CalEquation(int exp, double []coefficient)
        {
            for (int k = 1; k < exp; k++) //消元过程
            {
                for (int i = k + 1; i < exp + 1; i++)
                {
                    double p1 = 0;
                    if (Em[k,k] != 0)
                        p1 = Em[i,k] / Em[k,k];
                    for (int j = k; j < exp + 2; j++)
                        Em[i,j] = Em[i,j] - Em[k,j] * p1;
                }
            }
            coefficient[exp] = Em[exp,exp + 1] / Em[exp,exp];
            for (int l = exp - 1; l >= 1; l--) //回代求解
                coefficient[l] = (Em[l,exp + 1] - F(coefficient, l + 1, exp)) / Em[l,l];
        }
        //供CalEquation函数调用
        double F(double []c, int l, int m)
        {
            double sum = 0;
            for (int i = l; i <= m; i++)
                sum += Em[l - 1,i] * c[i];
            return sum;
        }
    }
}
