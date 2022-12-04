using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DataCollection
{
    public class Curve
    {   
	    double Ax, Ay;
        double Bx, By;
        double Cx, Cy;
        int Ndiv;
        public double SPLINE_CURVE_FACTOR = 50;
        public double DIV_FACTOR = 50;
        public Curve(double ax, double ay, double bx, double by, double cx, double cy, int ndiv)
        {
            Ax = ax;
            Ay = ay;
            Bx = bx;
            By = by;
            Cx = cx;
            Cy = cy;
            Ndiv = ndiv;           
        }
        public Curve(double ax, double ay, double bx, double by, double cx, double cy)
        {
            Ax = ax;
            Ay = ay;
            Bx = bx;
            By = by;
            Cx = cx;
            Cy = cy;
            Ndiv = (int)(Math.Max(Math.Abs(Ax), Math.Abs(Ay)) / DIV_FACTOR);
        }
        public Curve()
        {
        }
        public void PutCurve(double ax, double ay, double bx, double by, double cx, double cy)
        {
            Ax = ax;
            Ay = ay;
            Bx = bx;
            By = by;
            Cx = cx;
            Cy = cy;
            Ndiv = (int)(Math.Max(Math.Abs(Ax), Math.Abs(Ay)) / DIV_FACTOR);
        }
        public int GetCount()
        {
            if (Ndiv == 0) Ndiv = 1;
            int PointCount = 1;
            for (int i = 1; i <= Ndiv; i++)
            {
                PointCount++;
            }
            return PointCount;
        }
        public int GetCurve(double x, double y, Vector64[] points, int count)
        {
            int PointCount = count;
            int X, Y;
            double t, f, g, h;
            if (Ndiv == 0)Ndiv = 1;
            X = (int)x;
            Y = (int)y;
            points[PointCount].x = X;
            points[PointCount].y = Y;
            PointCount++;
            for (int i = 1; i <= Ndiv; i++)
            {
                t = 1.0f / (double)Ndiv * (double)i;
                f = t * t * (3.0f - 2.0f * t);
                g = t * (t - 1.0f) * (t - 1.0f);
                h = t * t * (t - 1.0f);
                X = (int)(x + Ax * f + Bx * g + Cx * h);
                Y = (int)(y + Ay * f + By * g + Cy * h);
                points[PointCount].x = X;
                points[PointCount].y = Y;
                PointCount++;
            }
            return PointCount;
        }
    }//end of class curve

    public class Spline
    {
        double SPLINE_CURVE_FACTOR = 50;
        double[] Px = null;
        double[] Py = null;
        double[] Ax = null;
        double[] Ay = null;
        double[] Bx = null;
        double[] By = null;
        double[] Cx = null;
        double[] Cy = null;
        double[] k = null;
        double[] Mat0 = null;
        double[] Mat1 = null;
        double[] Mat2 = null;
        int NP = 0;
        double DIV_FACTOR = 50;
        public int[] gridIndics = null;
        // constructor
        public Spline(Vector64[] pt)
        {
            int np = pt.Length;
            NP = np;
            Px = new double[NP];
            Py = new double[NP];
            Ax = new double[NP];
            Ay = new double[NP];
            Bx = new double[NP];
            By = new double[NP];
            Cx = new double[NP];
            Cy = new double[NP];
            k = new double[NP];
            Mat0 = new double[NP];
            Mat1 = new double[NP];
            Mat2 = new double[NP];
            gridIndics = new int[NP];
            for (int i = 0; i < NP; i++)
            {
                Px[i] = pt[i].x;
                Py[i] = pt[i].y;
            }
            DIV_FACTOR = SPLINE_CURVE_FACTOR; //adjust this factor to adjust the curve smoothness
        }
        public Spline(Vector32[] pt)
        {
            int np = pt.Length;
            NP = np;
            Px = new double[NP];
            Py = new double[NP];
            Ax = new double[NP];
            Ay = new double[NP];
            Bx = new double[NP];
            By = new double[NP];
            Cx = new double[NP];
            Cy = new double[NP];
            k = new double[NP];
            Mat0 = new double[NP];
            Mat1 = new double[NP];
            Mat2 = new double[NP];
            gridIndics = new int[NP];
            for (int i = 0; i < NP; i++)
            {
                Px[i] = pt[i].x;
                Py[i] = pt[i].y;
            }
            DIV_FACTOR = SPLINE_CURVE_FACTOR; //adjust this factor to adjust the curve smoothness
        }
        public Spline(List<Vector64> pt)
        {
            int np = pt.Count;
            NP = np;
            Px = new double[NP];
            Py = new double[NP];
            Ax = new double[NP];
            Ay = new double[NP];
            Bx = new double[NP];
            By = new double[NP];
            Cx = new double[NP];
            Cy = new double[NP];
            k = new double[NP];
            Mat0 = new double[NP];
            Mat1 = new double[NP];
            Mat2 = new double[NP];
            gridIndics = new int[NP];
            for (int i = 0; i < NP; i++)
            {
                Px[i] = pt[i].x;
                Py[i] = pt[i].y;
            }
            DIV_FACTOR = SPLINE_CURVE_FACTOR;
        }
        public Spline(List<Vector32> pt)
        {
            int np = pt.Count;
            NP = np;
            Px = new double[NP];
            Py = new double[NP];
            Ax = new double[NP];
            Ay = new double[NP];
            Bx = new double[NP];
            By = new double[NP];
            Cx = new double[NP];
            Cy = new double[NP];
            k = new double[NP];
            Mat0 = new double[NP];
            Mat1 = new double[NP];
            Mat2 = new double[NP];
            gridIndics = new int[NP];
            for (int i = 0; i < NP; i++)
            {
                Px[i] = pt[i].x;
                Py[i] = pt[i].y;
            }
            DIV_FACTOR = SPLINE_CURVE_FACTOR;
        }
        Spline(double []px , double []py, int np)
        {
            NP = np;
            Px = new double[NP];
            Py = new double[NP];
            Ax = new double[NP];
            Ay = new double[NP];
            Bx = new double[NP];
            By = new double[NP];
            Cx = new double[NP];
            Cy = new double[NP];
            k = new double[NP];
            Mat0 = new double[NP];
            Mat1 = new double[NP];
            Mat2 = new double[NP];
            gridIndics = new int[NP];
            for (int i = 0; i < NP; i++)
            {
                Px[i] = px[i];
                Py[i] = py[i];
            }
            DIV_FACTOR = SPLINE_CURVE_FACTOR; ; //adjust this factor to adjust the curve smoothness
        }

        public void Clear()
        {
            Px = null;
            Py = null;
            Ax = null;
            Ay = null;
            Bx = null;
            By = null;
            Cx = null;
            Cy = null;
            k = null;
            Mat0 = null;
            Mat1 = null;
            Mat2 = null;
            gridIndics = null;
        }
        int GetCurveCount()
        {
            Curve c = new Curve();
            c.DIV_FACTOR = DIV_FACTOR;
            int count = 0;
            for (int i = 0; i < NP - 1; i++)
            {
                c.PutCurve(Ax[i], Ay[i], Bx[i], By[i], Cx[i], Cy[i]);
                count += c.GetCount();
            }
            return count;
        }
        int GetCurve(Vector64[]points,int count)
        {
            int PointCount = count;
            Curve c = new Curve();
            c.DIV_FACTOR = DIV_FACTOR;
            int i = 0;
            for ( i = 0; i < NP - 1; i++)
            {
                gridIndics[i] = PointCount;
                c.PutCurve(Ax[i], Ay[i], Bx[i], By[i], Cx[i], Cy[i]);
                PointCount = c.GetCurve(Px[i], Py[i], points, PointCount);
            }
            gridIndics[i] = PointCount-1;
            return PointCount;
        }

        public List<Vector64> CreateSpline(bool enclose = false)
        {
            //create a spline object	
            //generate a curve
            Generate();
            //get the curve points number
            int nPoint = GetCurveCount();
            int PointCount = 0;
            Vector64[] pp = new Vector64[nPoint];
            PointCount = GetCurve(pp, PointCount);

            List<Vector64> outPoints = new List<Vector64>();

            //多边形，首点和尾点重合了，少取1个点
            if ( enclose ) PointCount -= 2;
            for (int i = 0; i < PointCount; i++)
            {
                Vector64 p = new Vector64(pp[i].x, pp[i].y,0);                
                if (i > 0)
                {
                    if (p.x == pp[0].x && p.y == pp[0].y)
                    {
                        if (enclose) outPoints.Add(p);
                        break;
                    }
                }
                outPoints.Add(p);
            }
            return outPoints;
        }

        void Generate()
        {
            double AMag, AMagOld;
            // vector A
            for (int i = 0; i <= NP - 2; i++)
            {
                Ax[i] = Px[i + 1] - Px[i];
                Ay[i] = Py[i + 1] - Py[i];
            }
            // k
            AMagOld = Math.Sqrt( Ax[0] * Ax[0] + Ay[0] * Ay[0] );
            for (int i = 0; i <= NP - 3; i++)
            {
                AMag = Math.Sqrt(Ax[i + 1] * Ax[i + 1] + Ay[i + 1] * Ay[i + 1]);
                k[i] = AMagOld / AMag;
                AMagOld = AMag;
            }
            k[NP - 2] = 1.0f;

            // Matrix
            for (int i = 1; i <= NP - 2; i++)
            {
                Mat0[i] = 1.0f;
                Mat1[i] = 2.0f * k[i - 1] * (1.0f + k[i - 1]);
                Mat2[i] = k[i - 1] * k[i - 1] * k[i];
            }
            Mat1[0] = 2.0f;
            Mat2[0] = k[0];
            Mat0[NP - 1] = 1.0f;
            Mat1[NP - 1] = 2.0f * k[NP - 2];
            for (int i = 1; i <= NP - 2; i++)
            {
                Bx[i] = 3.0f * (Ax[i - 1] + k[i - 1] * k[i - 1] * Ax[i]);
                By[i] = 3.0f * (Ay[i - 1] + k[i - 1] * k[i - 1] * Ay[i]);
            }
            Bx[0] = 3.0f * Ax[0];
            By[0] = 3.0f * Ay[0];
            Bx[NP - 1] = 3.0f * Ax[NP - 2];
            By[NP - 1] = 3.0f * Ay[NP - 2];

            //
            MatrixSolve(Bx);
            MatrixSolve(By);

            for (int i = 0; i <= NP - 2; i++)
            {
                Cx[i] = k[i] * Bx[i + 1];
                Cy[i] = k[i] * By[i + 1];
            }
        }

        void MatrixSolve(double []B)
        {
            double[]Work = new double[NP];
            double[]WorkB = new double[NP];
            for (int i = 0; i <= NP - 1; i++)
            {
                Work[i] = B[i] / Mat1[i];
                WorkB[i] = Work[i];
            }

            for (int j = 0; j < 10; j++)
            { ///  need convergence judge
                Work[0] = (B[0] - Mat2[0] * WorkB[1]) / Mat1[0];
                for (int i = 1; i < NP - 1; i++)
                    Work[i] = (B[i] - Mat0[i] * WorkB[i - 1] - Mat2[i] * WorkB[i + 1]) / Mat1[i];                
                Work[NP - 1] = (B[NP - 1] - Mat0[NP - 1] * WorkB[NP - 2]) / Mat1[NP - 1];
                for (int i = 0; i <= NP - 1; i++)                
                    WorkB[i] = Work[i];
            }
            for (int i = 0; i <= NP - 1; i++)            
                B[i] = Work[i];
            
            Work = null;
            WorkB = null;
        }       
        
        //////////// closed cubic spline ////////////////////
        void GenClosed()
        {
            double AMag, AMagOld, AMag0;
            // vector A
            for (int i = 0; i <= NP - 2; i++)
            {
                Ax[i] = Px[i + 1] - Px[i];
                Ay[i] = Py[i + 1] - Py[i];
            }
            Ax[NP - 1] = Px[0] - Px[NP - 1];
            Ay[NP - 1] = Py[0] - Py[NP - 1];
            
            AMag0 = AMagOld = Math.Sqrt(Ax[0] * Ax[0] + Ay[0] * Ay[0]);
            for (int i = 0; i <= NP - 2; i++)
            {
                AMag = Math.Sqrt(Ax[i + 1] * Ax[i + 1] + Ay[i + 1] * Ay[i + 1]);
                k[i] = AMagOld / AMag;
                AMagOld = AMag;
            }
            k[NP - 1] = AMagOld / AMag0;

            // Matrix
            for (int i = 1; i <= NP - 1; i++)
            {
                Mat0[i] = 1.0f;
                Mat1[1] = 2.0f * k[i - 1] * (1.0f + k[i - 1]);
                Mat2[i] = k[i - 1] * k[i - 1] * k[i];
            }
            Mat0[0] = 1.0f;
            Mat1[0] = 2.0f * k[NP - 1] * (1.0f + k[NP - 1]);
            Mat2[0] = k[NP - 1] * k[NP - 1] * k[0];

            // 
            for (int i = 1; i <= NP - 1; i++)
            {
                Bx[i] = 3.0f * (Ax[i - 1] + k[i - 1] * k[i - 1] * Ax[i]);
                By[i] = 3.0f * (Ay[i - 1] + k[i - 1] * k[i - 1] * Ay[i]);
            }
            Bx[0] = 3.0f * (Ax[NP - 1] + k[NP - 1] * k[NP - 1] * Ax[0]);
            By[0] = 3.0f * (Ay[NP - 1] + k[NP - 1] * k[NP - 1] * Ay[0]);

            //
            MatrixSolveEX(Bx);
            MatrixSolveEX(By);

            for (int i = 0; i <= NP - 2; i++)
            {
                Cx[i] = k[i] * Bx[i + 1];
                Cy[i] = k[i] * By[i + 1];
            }
            Cx[NP - 1] = k[NP - 1] * Bx[0];
            Cy[NP - 1] = k[NP - 1] * By[0];
        }

        ///// tridiagonal matrix + elements of [0][0], [N-1][N-1] //// 
        void MatrixSolveEX(double []B)
        {
            double[]Work = new double[NP];
            double[]WorkB = new double[NP];

            for (int i = 0; i <= NP - 1; i++)
            {
                Work[i] = B[i] / Mat1[i];
                WorkB[i] = Work[i];
            }

            for (int j = 0; j < 10; j++)
            {  // need judge of convergence
                Work[0] = (B[0] - Mat0[0] * WorkB[NP - 1] - Mat2[0] * WorkB[1]) / Mat1[0];
                for (int i = 1; i < NP - 1; i++)
                {
                    Work[i] = (B[i] - Mat0[i] * WorkB[i - 1] - Mat2[i] * WorkB[i + 1]) / Mat1[i];
                }
                Work[NP - 1] = (B[NP - 1] - Mat0[NP - 1] * WorkB[NP - 2] - Mat2[NP - 1] * WorkB[0]) / Mat1[NP - 1];
                for (int i = 0; i <= NP - 1; i++)
                {
                    WorkB[i] = Work[i];
                }
            }
            for (int i = 0; i <= NP - 1; i++)
            {
                B[i] = Work[i];
            }
        }
    }//Spline Cure

    /// <summary>    
    /// 样条曲线。每根样条曲线包含4个控制点
    /// </summary>
    public class SplineCurve
    {
        /// <summary>
        /// 样点数。在点Pk和Pk+1之间，将会生成若干个样点。所以"u"将会从0.00F增长到0.05F.
        /// </summary>
        public static int SamplePointCount = 20;
        /// <summary>
        /// 在基数算法中的t
        /// </summary>
        public static float Tension = 0.5F;

        #region 属性
        private PointF _startControlPoint;

        /// <summary>
        /// "Pk-1"点(起始控制点)
        /// </summary>
        public PointF StartControlPoint
        {
            get
            {
                return this._startControlPoint;
            }
            set
            {
                this._startControlPoint = value;
            }
        }

        private PointF _startPoint;
        /// <summary>
        ///  "Pk"点(起始点)
        /// </summary>
        public PointF StartPoint
        {
            get
            {
                return this._startPoint;
            }
            set
            {
                this._startPoint = value;
            }
        }

        private PointF _endPoint;
        /// <summary>
        /// "Pk+1"点(结束点)
        /// </summary>
        public PointF EndPoint
        {
            get
            {
                return this._endPoint;
            }
            set
            {
                this._endPoint = value;
            }
        }


        private PointF _endControlPoint;
        /// <summary>
        /// "Pk+2"点(结束控制点)
        /// </summary>
        public PointF EndControlPoint
        {
            get
            {
                return this._endControlPoint;
            }
            set
            {
                this._endControlPoint = value;
            }
        }

        private PointF[] _ctrlPoints;
        /// <summary>
        /// 曲线点(控制点及模拟的样点)
        /// </summary>
        public PointF[] CtrlPoints
        {
            get
            {
                return this._ctrlPoints;
            }
            set 
            {
                this._ctrlPoints = value;
            }
        }

        private bool _isFirst = false;
        /// <summary>
        /// 标识当前样条曲线是否是第一条，如果是m_startControlPoint 和 m_startPoint将会相同。
        /// 因为在Pk和Pk+1之间需要4个点来决定样条曲线，所以我们需要在Pk-1点前手动添加一个点。
        /// 这样我们才能在Pk-1和Pk+1之间绘制样条曲线。
        /// 同样的，最后一根样条曲线的Pk+2点会与它的"Pk+1"点相同，
        /// 这样我们才能在Pk+1和Pk+2之间绘制样条曲线。
        /// </summary>
        public bool IsFirst
        {
            get
            {
                return this._isFirst;
            }
            set
            {
                this._isFirst = value;
            }
        }
        #endregion

        public SplineCurve()
        {
            _startControlPoint = new PointF();
            _startPoint = new PointF();
            _endPoint = new PointF();
            _endControlPoint = new PointF();
            _ctrlPoints = new PointF[SamplePointCount + 1];
            for (int i = 0; i < _ctrlPoints.Length; i++)
            {
                _ctrlPoints[i] = new PointF();
            }
        }

        /// <summary>
        ///添加关节,将新控制点添加到控制点列表中，并更新前面的样条曲线。
        /// </summary>
        /// <param name="prevSpline">前一根样条曲线</param>
        /// <param name="currentPoint">当前点</param>
        public void AddJoint(SplineCurve prevSpline, PointF currentPoint)
        {
            //前一根样条曲线(prevSpline)为null，说明控制点列表中只有一个点，所以4个控制点样同。
            //当第2个及之后的控制点添加到控制点列表中时，那第1根样条曲线的Pk+1和Pk+2点需要更新
            if (null == prevSpline)
            {
                this._startControlPoint = currentPoint;
                this._startPoint = currentPoint;
                this._endPoint = currentPoint;
                this._endControlPoint = currentPoint;
                this._isFirst = true;
            }
            else//前一根样条曲线不为null，所以更新前一根样条曲线的控制点列表，同时更新当前样条曲线的控制点列表。
            {
                //前一根样条曲线是第1根样条曲线，更新它的Pk+1和Pk+2点
                if (true == prevSpline._isFirst)
                {
                    this._startControlPoint = prevSpline.StartControlPoint;
                    this._startPoint = prevSpline.StartPoint;
                    this._endPoint = currentPoint;
                    this._endControlPoint = currentPoint;
                    GenerateSamplePoint();
                    return;
                }
                else///前一根样条曲线不是第1根样条曲线，仅更新它的Pk+2点
                {
                    prevSpline.EndControlPoint = currentPoint;
                    prevSpline.GenerateSamplePoint();

                    //模拟当前样条曲线的样点
                    this._startControlPoint = prevSpline._startPoint;
                    this._startPoint = prevSpline._endPoint;
                    this._endPoint = currentPoint;
                    this._endControlPoint = currentPoint;
                    GenerateSamplePoint();

                }
            }
        }

        /// <summary>
        /// 使用基数算法生成样点
        /// </summary>
        public void GenerateSamplePoint()
        {
            PointF startControlPoint = this.StartControlPoint;
            PointF startPoint = this.StartPoint;
            PointF endPoint = this.EndPoint;
            PointF endControlPoint = this.EndControlPoint;
            float step = 1.0F / (float)SamplePointCount;
            float uValue = 0.00F;
            for (int i = 0; i < SamplePointCount; i++)
            {
                PointF pointNew = GenerateSimulatePoint(uValue, startControlPoint, startPoint, endPoint, endControlPoint);
                this.CtrlPoints[i] = pointNew;
                uValue += step;
            }
            this.CtrlPoints[_ctrlPoints.Length - 1] = endPoint;
        }

        /// <summary>
        /// 绘制样条曲线
        /// </summary>
        /// <param name="g"></param>
        public void Draw(Graphics g, Pen pen)
        {
            for (int i = 0; i < _ctrlPoints.Length - 1; i++)
            {
                PointF lastPoint = _ctrlPoints[i];
                PointF nextPoint = _ctrlPoints[i + 1];
                g.DrawLine(pen, lastPoint, nextPoint);
            }
        }


        #region GenerateSimulatePoint
        /// <summary>
        /// 生成曲线模拟点，该点在startPoint和endPoint之间
        /// </summary>
        /// <param name="u">介于0和1之间的变量</param>
        /// <param name="startControlPoint">起始点startPoint之前的控制点, 协助确定曲线的外观</param>
        /// <param name="startPoint">目标曲线的起始点startPoint,当u=0时，返回结果为起始点startPoint</param>
        /// <param name="endPoint">目标曲线的结束点endPoint, 当u=1时,返回结果为结束点endPoint</param>
        /// <param name="endControlPoint">在起结点startPoint之后的控制点, 协助确定曲线的外观</param>
        /// <returns>返回介于startPoint和endPoint的点</returns>
        private PointF GenerateSimulatePoint(float u,
                                PointF startControlPoint,
                                PointF startPoint,
                                PointF endPoint,
                                PointF endControlPoint)
        {
            //float s = (1 - _tension) / 2;
            float s = Tension;
            PointF resultPoint = new PointF();
            resultPoint.X = CalculateAxisCoordinate(startControlPoint.X, startPoint.X, endPoint.X, endControlPoint.X, s, u);
            resultPoint.Y = CalculateAxisCoordinate(startControlPoint.Y, startPoint.Y, endPoint.Y, endControlPoint.Y, s, u);
            return resultPoint;
        }

        /// <summary>
        /// 计算轴坐标
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="s"></param>
        /// <param name="u"></param>
        /// <returns></returns>
        private float CalculateAxisCoordinate(float a, float b, float c, float d, float s, float u)
        {
            float result = 0.0F;
            result = a * (2 * s * u * u - s * u * u * u - s * u)
                   + b * ((2 - s) * u * u * u + (s - 3) * u * u + 1)
                   + c * ((s - 2) * u * u * u + (3 - 2 * s) * u * u + s * u)
                   + d * (s * u * u * u - s * u * u);
            return result;
        }
        #endregion

        /// <summary>
        /// 获取样条曲线上的点
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pen"></param>
        /// <param name="points"></param>
        public static List<PointF> FetchPoints(PointF[] points)
        {
            if (points == null || points.Length <= 0)
            {
                return null;
            }

            List<SplineCurve> _splines = new List<SplineCurve>();
            SplineCurve splineNew = null;
            SplineCurve lastNew = null;
            foreach (PointF nowPoint in points)
            {
                if (null == _splines || 0 == _splines.Count)
                {
                    splineNew = new SplineCurve();
                    splineNew.AddJoint(null, nowPoint);
                    _splines.Add(splineNew);
                }
                else
                {
                    splineNew = new SplineCurve();
                    lastNew = _splines[_splines.Count - 1] as SplineCurve;
                    splineNew.AddJoint(lastNew, nowPoint);
                    _splines.Add(splineNew);
                };
            }

            List<PointF> _points = new List<PointF>();
            foreach (SplineCurve spline in _splines)
            {
                if (spline.IsFirst)
                {
                    continue;
                }
                foreach (PointF point in spline.CtrlPoints)
                {
                    if (_points.Contains(point))
                    {
                        continue;
                    }

                    _points.Add(point);
                }
            }
            return _points;
        }
    }//SplineCurve

    public static class SplineMath
    {
        /// <summary>
        /// 三次样条插值
        /// </summary>
        /// <param name="points">排序好的数</param>
        /// <param name="xs">需要计算的插值点</param>
        /// <param name="chf">写1</param>
        /// <returns>返回计算好的数值</returns>

        public static double[] SplineInsertPoint(PointClass[] points, double[] xs, int chf)
        {
            int plength = points.Length;
            double[] h = new double[plength];
            double[] f = new double[plength];
            double[] l = new double[plength];
            double[] v = new double[plength];
            double[] g = new double[plength];
                       
            for (int i = 0; i < plength - 1; i++)
            {
                h[i] = points[i + 1].x - points[i].x;
                f[i] = (points[i + 1].y - points[i].y) / h[i];
            }

            for (int i = 1; i < plength - 1; i++)
            {

                l[i] = h[i] / (h[i - 1] + h[i]);
                v[i] = h[i - 1] / (h[i - 1] + h[i]);
                g[i] = 3 * (l[i] * f[i - 1] + v[i] * f[i]);
            }

            double[] b = new double[plength];
            double[] tem = new double[plength];
            double[] m = new double[plength];
            double f0 = (points[0].y - points[1].y) / (points[0].x - points[1].x);
            double fn = (points[plength - 1].y - points[plength - 2].y) / (points[plength - 1].x - points[plength - 2].x);
            b[1] = v[1] / 2;
            for (int i = 2; i < plength - 2; i++)
            {
                // Console.Write(" " + i);
                b[i] = v[i] / (2 - b[i - 1] * l[i]);
            }
            tem[1] = g[1] / 2;
            for (int i = 2; i < plength - 1; i++)
            {
                //Console.Write(" " + i);
                tem[i] = (g[i] - l[i] * tem[i - 1]) / (2 - l[i] * b[i - 1]);
            }

            m[plength - 2] = tem[plength - 2];
            for (int i = plength - 3; i > 0; i--)
            {
                //Console.Write(" " + i);
                m[i] = tem[i] - b[i] * m[i + 1];
            }

            m[0] = 3 * f[0] / 2.0;
            m[plength - 1] = fn;
            int xlength = xs.Length;
            double[] insertRes = new double[xlength];

            for (int i = 0; i < xlength; i++)
            {
                int j = 0;
                for (j = 0; j < plength; j++)
                {
                    if (xs[i] < points[j].x)
                        break;
                }

                j = j - 1;
                Console.WriteLine(j);
                if (j == -1 || j == points.Length - 1)
                {
                    if (j == -1)
                        throw new Exception("插值下边界超出");
                    if (j == points.Length - 1 && xs[i] == points[j].x)
                        insertRes[i] = points[j].y;
                    else
                        throw new Exception("插值下边界超出");
                }
                else
                {
                    double p1;
                    p1 = (xs[i] - points[j + 1].x) / (points[j].x - points[j + 1].x);
                    p1 = p1 * p1;
                    double p2; p2 = (xs[i] - points[j].x) / (points[j + 1].x - points[j].x);
                    p2 = p2 * p2;
                    double p3; p3 = p1 * (1 + 2 * (xs[i] - points[j].x) / (points[j + 1].x - points[j].x)) * points[j].y + p2 * (1 + 2 * (xs[i] - points[j + 1].x) / (points[j].x - points[j + 1].x)) * points[j + 1].y;                                     
                    double p4; p4 = p1 * (xs[i] - points[j].x) * m[j] + p2 * (xs[i] - points[j + 1].x) * m[j + 1];
                    //         Console.WriteLine(m[j] + " " + m[j + 1] + " " + j);

                    p4 = p4 + p3;
                    insertRes[i] = p4;
                    //Console.WriteLine("f(" + xs[i] + ")= " + p4);
                }
            }
            //Console.ReadLine();
            return insertRes;
        }

    }

    public class PointClass
    {
        public double x = 0;
        public double y = 0;
        public PointClass()
        {
            x = 0; y = 0;
        }

        //-------写一个排序函数，使得输入的点按顺序排列，是因为插值算法的要求是，x轴递增有序的---------
        public static PointClass[] DeSortX(PointClass[] points)
        {
            int length = points.Length;
            double temx, temy;
            for (int i = 0; i < length - 1; i++)
            {

                for (int j = 0; j < length - i - 1; j++)
                    if (points[j].x > points[j + 1].x)
                    {
                        temx = points[j + 1].x;
                        points[j + 1].x = points[j].x;
                        points[j].x = temx;
                        temy = points[j + 1].y;
                        points[j + 1].y = points[j].y;
                        points[j].y = temy;
                    }
            }
            return points;
        }
    }

}
