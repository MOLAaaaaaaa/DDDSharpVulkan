using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCollection
{
    public class XYZV_DOUBLE_POINT
    {
        public double x, y, z, v;
        public double dist;
        public double weight;
        public XYZV_DOUBLE_POINT()
        {
            x = y = z = v = 0;
            dist = weight = 0;
        }
    }
    public class CLeastSquare
    {
        private List<XYZV_DOUBLE_POINT> pOrgPoints = new List<XYZV_DOUBLE_POINT>();
#pragma warning disable CS0414 // 字段“CLeastSquare.maxNum”已被赋值，但从未使用过它的值
        private int maxNum = 20;
#pragma warning restore CS0414 // 字段“CLeastSquare.maxNum”已被赋值，但从未使用过它的值
        private int m = 0;
        private int n = 0;
        private int p1 = 6;
        private int q1 = 5;
        private double[] alpha;
        private double[] zvalue;
        private double[] xvalue;
        private double[] yvalue;
        private double[] dt;
        private double x0, y0;
        public CLeastSquare()
        {
        }
        public void AddPoint(double x, double y, double z)
        {
            XYZV_DOUBLE_POINT p = new XYZV_DOUBLE_POINT();
            p.x = x;
            p.y = y;
            p.z = z;
            p.v = z;
            pOrgPoints.Add(p);
        }
        public int DataSize()
        {
            return pOrgPoints.Count;
        }
        public void Init(int _p = 6,int _q = 5)
        {
            p1 = _p;
            q1 = _q;

            if (DataSize() < 1) return;

            XYZV_DOUBLE_POINT p = new XYZV_DOUBLE_POINT();
            for (int i=0;i<pOrgPoints.Count;i++)
                for (int j = i+1; j < pOrgPoints.Count; j++)
            {
                //sort on x
                if( pOrgPoints[i].x > pOrgPoints[j].x )
                    {
                        p.x = pOrgPoints[i].x;
                        p.y = pOrgPoints[i].y;
                        p.z = pOrgPoints[i].z;
                        p.v = pOrgPoints[i].v;
                        pOrgPoints[i].x = pOrgPoints[j].x;
                        pOrgPoints[i].y = pOrgPoints[j].y;
                        pOrgPoints[i].z = pOrgPoints[j].z;
                        pOrgPoints[i].v = pOrgPoints[j].v;
                        pOrgPoints[j].x = p.x;
                        pOrgPoints[j].y = p.y;
                        pOrgPoints[j].z = p.z;
                        pOrgPoints[j].v = p.v;
                    }
            }

            List<double> px = new List<double>();
            x0 = pOrgPoints[0].x;
            px.Add(x0);            
            for (int k = 1; k < pOrgPoints.Count; k++)
            {
                if (pOrgPoints[k].x > x0)
                {
                    px.Add(pOrgPoints[k].x);
                    x0 = pOrgPoints[k].x;
                }
            }
            //sort on y
            for (int i = 0; i < pOrgPoints.Count; i++)
                for (int j = i + 1; j < pOrgPoints.Count; j++)
                {
                    if (pOrgPoints[i].y > pOrgPoints[j].y)
                    {
                        p.x = pOrgPoints[i].x;
                        p.y = pOrgPoints[i].y;
                        p.z = pOrgPoints[i].z;
                        p.v = pOrgPoints[i].v;
                        pOrgPoints[i].x = pOrgPoints[j].x;
                        pOrgPoints[i].y = pOrgPoints[j].y;
                        pOrgPoints[i].z = pOrgPoints[j].z;
                        pOrgPoints[i].v = pOrgPoints[j].v;
                        pOrgPoints[j].x = p.x;
                        pOrgPoints[j].y = p.y;
                        pOrgPoints[j].z = p.z;
                        pOrgPoints[j].v = p.v;
                    }
                }
            List<double> py = new List<double>();
            y0 = pOrgPoints[0].y;
            py.Add(y0);
            for (int k = 1; k < pOrgPoints.Count; k++)
            {
                if (pOrgPoints[k].y > y0)
                {
                    py.Add(pOrgPoints[k].y);
                    y0 = pOrgPoints[k].y;
                }
            }

            CInversePower ip = new CInversePower();
            for(int  i=0;i<pOrgPoints.Count;i++)
            {
                ip.AddPoint(pOrgPoints[i].x, pOrgPoints[i].y, 0, pOrgPoints[i].z);
            }
            n = px.Count;
            m = py.Count;            
            zvalue = new double[n * m];
            alpha = new double[p1 * q1];
            xvalue = new double[n];
            yvalue = new double[m];
            dt = new double[3];

            x0 = 0;
            for (int i = 0; i < n; i++)
            {
                xvalue[i] = px[i];
                x0 += xvalue[i];
            }
            x0 = x0 / n;           

            y0 = 0;
            for (int j = 0; j < m; j++)
            {
                yvalue[j] = py[j];
                y0 += yvalue[j];
            }
            y0 = y0 / m;            

            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    zvalue[i * m + j] = ip.GetInterValue(xvalue[i], yvalue[j], 0);

           // for (int i = 0; i < n; i++)
           //     xvalue[i] = xvalue[i] - x0;
           // for (int i = 0; i < m; i++)
            //    yvalue[i] = yvalue[i] - y0;

            pir2(xvalue, yvalue, zvalue, n, m, alpha, p1, q1, dt);
        }
        public double GetInterpolatedValue(double x1,double y1)
        {
            double v = 0, sum = 0;
            for (int i = 0; i < p1; i++)
                for (int j = 0; j < q1; j++)
                {
                    //v = Math.Pow( (x1 - x0), i) * Math.Pow( (y1 - y0), j );
                    v = Math.Pow(x1, i) * Math.Pow(y1, j);
                    if (v * alpha[i * q1 + j] >= 1000 || v * alpha[i * q1 + j] <= -1000)
                    {
#pragma warning disable CS0219 // 变量“dd”已被赋值，但从未使用过它的值
                        int dd = 0;
#pragma warning restore CS0219 // 变量“dd”已被赋值，但从未使用过它的值
                    }
                    v = v * alpha[i * q1 + j];
                    sum += v;
                }
            return sum;
        }
        public void Test()
        {
                        
        }
        /***************************************************************************
         * -----------------最小二乘插值--------------------
        形参与函数类型  参数意义
        double x[n] 存放给定数据点的n个X坐标
        double y[m] 存放给定数据的m个Y坐标
        double z[n*m]  存放矩形居于内n×m个网点上的函数值
        int n   X坐标个数
        int m   Y坐标个数
        Double a[p*q]  返回二元拟合多项式
        int q   拟合多项式中x的最高次数加1。要求p≤n，且p≤20，若不满足这个条件，本函数自动取p=min{n，20}
        Double dt[3]    
                dt[0]返回拟合多项式与数据点误差的平方和，
                dt[1]返回拟合多项式与数据点误差绝对值之和，
                dt[2]返回拟合多项式与数据点误差绝对值的最大值        
        *****************************************************************************/
        /*
        private void pir2(double[] x, double[] y, double[] z, int n, int m, double[] a, int p, int q, double[] dt)
        {
            int i, j, k, l, kk;
            double[] apx = new double[maxNum];
            double[] apy = new double[maxNum];
            double[] bx = new double[maxNum];
            double[] by = new double[maxNum];
            double[,] u = new double[maxNum, maxNum];
            double[] t = new double[maxNum];
            double[] t1 = new double[maxNum];
            double[] t2 = new double[maxNum];
            double xx, yy, d1, d2, g, g1, g2;
            double x2, dd, y1, x1;
            double[] v = new double[m*maxNum];
            for (i = 0; i < p ; i++)
                for (j = 0; j < q ; j++)
                    a[i*q + j] = 0.0;
            
            if (p > n) p = n;
            if (p > maxNum) p = maxNum;
            if (q > m) q = m;
            if (q > maxNum) q = maxNum;
            xx = 0.0;
            for (i = 0; i <= n - 1; i++) xx = xx + x[i] / (1.0 * n);
            yy = 0.0;
            for (i = 0; i <= m - 1; i++) yy = yy + y[i] / (1.0 * m);
            d1 = 1.0 * n; apx[0] = 0.0;
            for (i = 0; i <= n - 1; i++) apx[0] = apx[0] + x[i] - xx;
            apx[0] = apx[0] / d1;
            for (j = 0; j < m ; j++)
            {
                v[j] = 0.0;
                for (i = 0; i < n; i++)
                    v[j] = v[j] + z[i * m + j];
                v[j] = v[j] / d1;
            }
            if (p > 1)
            {
                d2 = 0.0; apx[1] = 0.0;
                for (i = 0; i <= n - 1; i++)
                {
                    g = x[i] - xx - apx[0];
                    d2 = d2 + g * g;
                    apx[1] = apx[1] + (x[i] - xx) * g * g;
                }
                apx[1] = apx[1] / d2;
                bx[1] = d2 / d1;
                for (j = 0; j <= m - 1; j++)
                {
                    v[m + j] = 0.0;
                    for (i = 0; i <= n - 1; i++)
                    {
                        g = x[i] - xx - apx[0];
                        v[m + j] = v[m + j] + z[i * m + j] * g;
                    }
                    v[m + j] = v[m + j] / d2;
                }
                d1 = d2;
            }
            for (k = 2; k <= p - 1; k++)
            {
                d2 = 0.0; apx[k] = 0.0;
                for (j = 0; j <= m - 1; j++) v[k * m + j] = 0.0;
                for (i = 0; i <= n - 1; i++)
                {
                    g1 = 1.0; g2 = x[i] - xx - apx[0];
                    g = 0;
                    for (j = 2; j <= k; j++)
                    {
                        g = (x[i] - xx - apx[j - 1]) * g2 - bx[j - 1] * g1;
                        g1 = g2; g2 = g;
                    }
                    d2 = d2 + g * g;
                    apx[k] = apx[k] + (x[i] - xx) * g * g;
                    for (j = 0; j <= m - 1; j++)
                        v[k * m + j] = v[k * m + j] + z[i * m + j] * g;
                }
                for (j = 0; j <= m - 1; j++) v[k * m + j] = v[k * m + j] / d2;
                apx[k] = apx[k] / d2;
                bx[k] = d2 / d1;
                d1 = d2;
            }
            d1 = m;
            apy[0] = 0.0;
            for (i = 0; i <= m - 1; i++) apy[0] = apy[0] + y[i] - yy;
            apy[0] = apy[0] / d1;
            for (j = 0; j <= p - 1; j++)
            {
                u[j, 0] = 0.0;
                for (i = 0; i <= m - 1; i++) u[j, 0] = u[j, 0] + v[j * m + i];
                u[j, 0] = u[j, 0] / d1;
            }
            if (q > 1)
            {
                d2 = 0.0; apy[1] = 0.0;
                for (i = 0; i <= m - 1; i++)
                {
                    g = y[i] - yy - apy[0];
                    d2 = d2 + g * g;
                    apy[1] = apy[1] + (y[i] - yy) * g * g;
                }
                apy[1] = apy[1] / d2;
                by[1] = d2 / d1;
                for (j = 0; j <= p - 1; j++)
                {
                    u[j, 1] = 0.0;
                    for (i = 0; i <= m - 1; i++)
                    {
                        g = y[i] - yy - apy[0];
                        u[j, 1] = u[j, 1] + v[j * m + i] * g;
                    }
                    u[j, 1] = u[j, 1] / d2;
                }
                d1 = d2;
            }
            for (k = 2; k <= q - 1; k++)
            {
                d2 = 0.0; apy[k] = 0.0;
                for (j = 0; j <= p - 1; j++) u[j, k] = 0.0;
                for (i = 0; i <= m - 1; i++)
                {
                    g1 = 1.0;
                    g2 = y[i] - yy - apy[0];
                    g = 0;
                    for (j = 2; j <= k; j++)
                    {
                        g = (y[i] - yy - apy[j - 1]) * g2 - by[j - 1] * g1;
                        g1 = g2; g2 = g;
                    }
                    d2 = d2 + g * g;
                    apy[k] = apy[k] + (y[i] - yy) * g * g;
                    for (j = 0; j <= p - 1; j++)
                        u[j, k] = u[j, k] + v[j * m + i] * g;
                }
                for (j = 0; j <= p - 1; j++) u[j, k] = u[j, k] / d2;
                apy[k] = apy[k] / d2;
                by[k] = d2 / d1;
                d1 = d2;
            }
            v[0] = 1.0;
            v[m] = -apy[0];
            v[m + 1] = 1.0;
            for (i = 0; i <= p - 1; i++)
                for (j = 0; j <= q - 1; j++)
                    a[i * q + j] = 0.0;
            for (i = 2; i <= q - 1; i++)
            {
                v[i * m + i] = v[(i - 1) * m + (i - 1)];
                v[i * m + i - 1] = -apy[i - 1] * v[(i - 1) * m + i - 1] + v[(i - 1) * m + i - 2];
                if (i >= 3)
                    for (k = i - 2; k >= 1; k--)
                        v[i * m + k] = -apy[i - 1] * v[(i - 1) * m + k] + v[(i - 1) * m + k - 1] - by[i - 1] * v[(i - 2) * m + k];

                v[i * m] = -apy[i - 1] * v[(i - 1) * m] - by[i - 1] * v[(i - 2) * m];
            }
            for (i = 0; i <= p - 1; i++)
            {
                if (i == 0) { t[0] = 1.0; t1[0] = 1.0; }
                else
                {
                    if (i == 1)
                    {
                        t[0] = -apx[0]; t[1] = 1.0;
                        t2[0] = t[0]; t2[1] = t[1];
                    }
                    else
                    {
                        t[i] = t2[i - 1];
                        t[i - 1] = -apx[i - 1] * t2[i - 1] + t2[i - 2];
                        if (i >= 3)
                            for (k = i - 2; k >= 1; k--)
                                t[k] = -apx[i - 1] * t2[k] + t2[k - 1] - bx[i - 1] * t1[k];

                        t[0] = -apx[i - 1] * t2[0] - bx[i - 1] * t1[0];
                        t2[i] = t[i];

                        for (k = i - 1; k >= 0; k--)
                        {
                            t1[k] = t2[k];
                            t2[k] = t[k];
                        }
                    }
                }
                for (j = 0; j <= q - 1; j++)
                    for (k = i; k >= 0; k--)
                        for (l = j; l >= 0; l--)
                            a[k * q + l] = a[k * q + l] + u[i, j] * t[k] * v[j * m + l];
            }
            dt[0] = dt[1] = dt[2] = 0.0;
            for (i = 0; i <= n - 1; i++)
            {
                x1 = x[i] - xx;
                for (j = 0; j <= m - 1; j++)
                {
                    y1 = y[j] - yy;
                    x2 = 1.0; dd = 0.0;
                    for (k = 0; k <= p - 1; k++)
                    {
                        g = a[k * q + q - 1];
                        for (kk = q - 2; kk >= 0; kk--) g = g * y1 + a[k * q + kk];
                        g = g * x2;
                        dd = dd + g;
                        x2 = x2 * x1;
                    }
                    dd = dd - z[i * m + j];
                    if (Math.Abs(dd) > dt[2]) dt[2] = Math.Abs(dd);
                    dt[0] = dt[0] + dd * dd;
                    dt[1] = dt[1] + Math.Abs(dd);
                }
            }
            return;
        }*/
        private void pir2(double[] x, double[] y, double[] z, int n, int m, double[] a, int p, int q, double[] dt)
        {
            int i, j, k, l, kk;
            double[] apx = new double[20];
            double[] apy = new double[20];
            double[] bx = new double[20];
            double[] by = new double[20];
            double[,] u = new double[20, 20];
            double[] t = new double[20];
            double[] t1 = new double[20];
            double[] t2 = new double[20];
            double xx, yy, d1, d2, g, g1, g2;
            double x2, dd, y1, x1;
            double[] v = new double[20 * m];
            for (i = 0; i <= p - 1; i++)
            {
                l = i * q;
                for (j = 0; j <= q - 1; j++) a[l + j] = 0.0;
            }
            if (p > n) p = n;
            if (p > 20) p = 20;
            if (q > m) q = m;
            if (q > 20) q = 20;
            xx = 0.0;
            for (i = 0; i <= n - 1; i++)
                xx = xx + x[i] / (1.0 * n);
            yy = 0.0;
            for (i = 0; i <= m - 1; i++)
                yy = yy + y[i] / (1.0 * m);
            d1 = 1.0 * n; apx[0] = 0.0;
            for (i = 0; i <= n - 1; i++)
                apx[0] = apx[0] + x[i] - xx;
            apx[0] = apx[0] / d1;
            for (j = 0; j <= m - 1; j++)
            {
                v[j] = 0.0;
                for (i = 0; i <= n - 1; i++)
                    v[j] = v[j] + z[i * m + j];
                v[j] = v[j] / d1;
            }
            if (p > 1)
            {
                d2 = 0.0; apx[1] = 0.0;
                for (i = 0; i <= n - 1; i++)
                {
                    g = x[i] - xx - apx[0];
                    d2 = d2 + g * g;
                    apx[1] = apx[1] + (x[i] - xx) * g * g;
                }
                apx[1] = apx[1] / d2;
                bx[1] = d2 / d1;
                for (j = 0; j <= m - 1; j++)
                {
                    v[m + j] = 0.0;
                    for (i = 0; i <= n - 1; i++)
                    {
                        g = x[i] - xx - apx[0];
                        v[m + j] = v[m + j] + z[i * m + j] * g;
                    }
                    v[m + j] = v[m + j] / d2;
                }
                d1 = d2;
            }
            for (k = 2; k <= p - 1; k++)
            {
                d2 = 0.0; apx[k] = 0.0;
                for (j = 0; j <= m - 1; j++) v[k * m + j] = 0.0;
                for (i = 0; i <= n - 1; i++)
                {
                    g1 = 1.0; g2 = x[i] - xx - apx[0];
                    g = 0;
                    for (j = 2; j <= k; j++)
                    {
                        g = (x[i] - xx - apx[j - 1]) * g2 - bx[j - 1] * g1;
                        g1 = g2; g2 = g;
                    }
                    d2 = d2 + g * g;
                    apx[k] = apx[k] + (x[i] - xx) * g * g;
                    for (j = 0; j <= m - 1; j++)
                        v[k * m + j] = v[k * m + j] + z[i * m + j] * g;
                }
                for (j = 0; j <= m - 1; j++)
                    v[k * m + j] = v[k * m + j] / d2;
                apx[k] = apx[k] / d2;
                bx[k] = d2 / d1;
                d1 = d2;
            }
            d1 = m; apy[0] = 0.0;
            for (i = 0; i <= m - 1; i++)
                apy[0] = apy[0] + y[i] - yy;
            apy[0] = apy[0] / d1;
            for (j = 0; j <= p - 1; j++)
            {
                u[j, 0] = 0.0;
                for (i = 0; i <= m - 1; i++)
                    u[j, 0] = u[j, 0] + v[j * m + i];
                u[j, 0] = u[j, 0] / d1;
            }
            if (q > 1)
            {
                d2 = 0.0; apy[1] = 0.0;
                for (i = 0; i <= m - 1; i++)
                {
                    g = y[i] - yy - apy[0];
                    d2 = d2 + g * g;
                    apy[1] = apy[1] + (y[i] - yy) * g * g;
                }
                apy[1] = apy[1] / d2;
                by[1] = d2 / d1;
                for (j = 0; j <= p - 1; j++)
                {
                    u[j, 1] = 0.0;
                    for (i = 0; i <= m - 1; i++)
                    {
                        g = y[i] - yy - apy[0];
                        u[j, 1] = u[j, 1] + v[j * m + i] * g;
                    }
                    u[j, 1] = u[j, 1] / d2;
                }
                d1 = d2;
            }
            for (k = 2; k <= q - 1; k++)
            {
                d2 = 0.0; apy[k] = 0.0;
                for (j = 0; j <= p - 1; j++) u[j, k] = 0.0;
                for (i = 0; i <= m - 1; i++)
                {
                    g1 = 1.0;
                    g2 = y[i] - yy - apy[0];
                    g = 0;
                    for (j = 2; j <= k; j++)
                    {
                        g = (y[i] - yy - apy[j - 1]) * g2 - by[j - 1] * g1;
                        g1 = g2; g2 = g;
                    }
                    d2 = d2 + g * g;
                    apy[k] = apy[k] + (y[i] - yy) * g * g;
                    for (j = 0; j <= p - 1; j++)
                        u[j, k] = u[j, k] + v[j * m + i] * g;
                }
                for (j = 0; j <= p - 1; j++)
                    u[j, k] = u[j, k] / d2;
                apy[k] = apy[k] / d2;
                by[k] = d2 / d1;
                d1 = d2;
            }
            v[0] = 1.0; v[m] = -apy[0]; v[m + 1] = 1.0;
            for (i = 0; i <= p - 1; i++)
                for (j = 0; j <= q - 1; j++)
                    a[i * q + j] = 0.0;
            for (i = 2; i <= q - 1; i++)
            {
                v[i * m + i] = v[(i - 1) * m + (i - 1)];
                v[i * m + i - 1] = -apy[i - 1] * v[(i - 1) * m + i - 1] + v[(i - 1) * m + i - 2];
                if (i >= 3)
                    for (k = i - 2; k >= 1; k--)
                        v[i * m + k] = -apy[i - 1] * v[(i - 1) * m + k] +
                                 v[(i - 1) * m + k - 1] - by[i - 1] * v[(i - 2) * m + k];
                v[i * m] = -apy[i - 1] * v[(i - 1) * m] - by[i - 1] * v[(i - 2) * m];
            }
            for (i = 0; i <= p - 1; i++)
            {
                if (i == 0) { t[0] = 1.0; t1[0] = 1.0; }
                else
                {
                    if (i == 1)
                    {
                        t[0] = -apx[0]; t[1] = 1.0;
                        t2[0] = t[0]; t2[1] = t[1];
                    }
                    else
                    {
                        t[i] = t2[i - 1];
                        t[i - 1] = -apx[i - 1] * t2[i - 1] + t2[i - 2];
                        if (i >= 3)
                            for (k = i - 2; k >= 1; k--)
                                t[k] = -apx[i - 1] * t2[k] + t2[k - 1]
                                     - bx[i - 1] * t1[k];
                        t[0] = -apx[i - 1] * t2[0] - bx[i - 1] * t1[0];
                        t2[i] = t[i];
                        for (k = i - 1; k >= 0; k--)
                        { t1[k] = t2[k]; t2[k] = t[k]; }
                    }
                }
                for (j = 0; j <= q - 1; j++)
                    for (k = i; k >= 0; k--)
                        for (l = j; l >= 0; l--)
                            a[k * q + l] = a[k * q + l] + u[i, j] * t[k] * v[j * m + l];
            }
            dt[0] = 0.0; dt[1] = 0.0; dt[2] = 0.0;
            for (i = 0; i <= n - 1; i++)
            {
                x1 = x[i] - xx;
                for (j = 0; j <= m - 1; j++)
                {
                    y1 = y[j] - yy;
                    x2 = 1.0; dd = 0.0;
                    for (k = 0; k <= p - 1; k++)
                    {
                        g = a[k * q + q - 1];
                        for (kk = q - 2; kk >= 0; kk--)
                            g = g * y1 + a[k * q + kk];
                        g = g * x2; dd = dd + g; x2 = x2 * x1;
                    }
                    dd = dd - z[i * m + j];
                    if (Math.Abs(dd) > dt[2]) dt[2] = Math.Abs(dd);
                    dt[0] = dt[0] + dd * dd;
                    dt[1] = dt[1] + Math.Abs(dd);
                }
            }

            return;
        }
    }
    public struct IndexedNode
    {
        public int index;
        public double distance;
        public IndexedNode(int id,double dist)
        {
            index = id;
            distance = dist;
        }
    }

    public class CSearchedList
    {
        //refer to the index divided by 2 angles
        int angleArea = 0;

        //points in this angle area
        public List<IndexedNode> points;

        public CSearchedList(int id)
        {
            angleArea = id;
            points = new List<IndexedNode>();
        }
        public void AddNode(int id,double dist)
        {
            IndexedNode node = new IndexedNode(id,dist);
            points.Add(node);
        }
        public void Clear()
        {
            points.Clear();
        }
        public int Size()
        {
            return points.Count;
        }
        //sort by distance
        public void Sort()
        {
            int n = points.Count;
            if (n <= 0) return;
            IndexedNode node1, node2;
            for (int i = 0; i < n; i++)
            {
                node1 = points[i];
                for (int j = i + 1; j < n; j++)
                {
                    node2 = points[j];
                    if (node1.distance > node2.distance)
                    {
                        IndexedNode node = new IndexedNode(node1.index, node1.distance);
                        points[i] = node2;
                        points[j] = node;
                    }
                }
            }
        }
    }

    public enum SearchMethod
    {
        none = 0,   // no search, use all data 
        Sphere = 1, // use a sphere
        Cube =2,     // use a cube
        MultiAngle = 3,  //search on multi-direction
    };

    public class CInversePower
    {
        public List<Vector64> pScatterPoint = new List<Vector64>();
        public double minx, maxx, miny, maxy, minz, maxz, minv, maxv;

        SearchMethod nSearchMode = SearchMethod.none;
        //distance Zero points
        private CSearchedList zeroList;
        //other points list
        private CSearchedList pointList;

        //search radius
        private double searchDist = -1;

        //search radius on x,y,z direction
        private double searchDistX = -1;
        private double searchDistY = -1;
        private double searchDistZ = -1;

        private int minNum = 4;        
        private double XRatio = 1;
        private double YRatio = 1;
        private double ZRatio = 1;

        //search in a cube
        private CSearchedList[] pAngledPoints;        
        private double angleStep = 10;
        private int angleNumHor = 36;
        private int angleNumVert = 18;
        
        public void SetAngleStep(double step)
        {
            angleStep = step;
            angleNumHor = (int)(360 / angleStep);
            angleNumVert = (int)(180 / angleStep);
        }
        
        //get the angle belong which strip
        private int GetAngleStripIndex(double _angleHor,double _angleVert)
        {
            //index 0 store zero distance            
            int ix = (int)(_angleHor / angleStep);
            int iy = (int)(_angleVert / angleStep);

            double xa = _angleHor - ix * angleStep;
            double ya = _angleVert - iy * angleStep;
            if( xa == 0 && _angleHor >0 ) ix--;
            if (ya == 0 && _angleVert > 0) iy--;

            return ix + iy * angleNumHor;
        }
        //get the 2 angles from strip index
        private void GetAngleFromIndex(int index, out double _angleHor, out double _angleVert)
        {
            int iy = (index-1) / angleNumHor;
            int ix = (index-1) - iy;

            _angleHor = angleStep*ix;
            _angleVert = angleStep * iy;
        }

        private void InitAngles()
        {
            if (pAngledPoints == null)
            {
                int id;
                pAngledPoints = new CSearchedList[angleNumHor * angleNumVert];                
                for (int i = 0; i < angleNumVert; i++)
                    for (int j = 0; j < angleNumHor; j++)
                    {
                        id = i * angleNumHor + j;
                        pAngledPoints[id] = new CSearchedList(id);
                    }
            }
            else
            {
                for (int i = 0; i < angleNumVert * angleNumHor; i++)
                    pAngledPoints[i].Clear();
            }
        }
        private double toAngle(double angle)
        {
            return 180 * angle / Math.PI;
        } 
        
        public CInversePower()
        {
            minx = maxx = 0;
            miny = maxy = 0;
            minz = maxz = 0;
            minv = maxv = 0;

            zeroList = new CSearchedList(0);
            pointList = new CSearchedList(1);

            Clear();            
        }
        public bool IsZero(double value)
        {
            if (value >= -1.0E-10 && value <= 1.0E-10)
                return true;
            return false;
        }
        public void Clear()
        {
            pScatterPoint.Clear();
            zeroList.Clear();
            pointList.Clear();
        }
        public void SetSearchNum(int min_point_no = 2)
        {
            minNum = min_point_no;
        }        
        public void SetSearchMode(SearchMethod mode)
        {
            nSearchMode = mode;
        }
        public void SetScale(double xscale,double yscale,double zscale)
        {
            XRatio = xscale;
            YRatio = yscale;
            ZRatio = zscale;
        }
        public void SetSearchDistPercent(double percent)
        {
            if( minx == maxx || miny == maxy || minz == maxz)
                UpdateDataRange();

            double maxlen = maxx - minx;
            if (maxy - miny > maxlen) maxlen = maxy - miny;
            if (maxz - minz > maxlen) maxlen = maxz - minz;

            searchDist = maxlen * percent;
        }
        public void SetSearchDist(double percent)
        {
            searchDist = percent;
        }
        public void SetSearchDistPercent(double percentx, double percenty, double percentz)
        {
            if (minx == maxx || miny == maxy || minz == maxz)
                UpdateDataRange();
            searchDistX = percentx * (maxx - minx);
            searchDistY = percenty * (maxy - miny);
            searchDistZ = percentz * (maxz - minz);
        }
        public void SetSearchDist(double searchx, double searchy, double searchz)
        {
            searchDistX = searchx;
            searchDistY = searchy;
            searchDistZ = searchz;
        }
        public void AddPoint(Vector32 p)
        {
            AddPoint(new Vector64(p.X,p.Y,p.Z,p.V));
        }
        public void AddPoint(Vector64 p)
        {        
            if(pScatterPoint.Count == 0)
            {
                minx = maxx = p.X;
                miny = maxy = p.Y;
                minz = maxz = p.Z;
                minv = maxv = p.V;
            }
            else
            {
                if (p.X < minx) minx = p.X;
                if (p.Y < miny) miny = p.Y;
                if (p.Z < minz) minz = p.Z;
                if (p.V < minv) minv = p.V;
                if (p.X > maxx) maxx = p.X;
                if (p.Y > maxy) maxy = p.Y;
                if (p.Z > maxz) maxz = p.Z;
                if (p.V > maxv) maxv = p.V;
            }
            pScatterPoint.Add(p);            
        }
        public void UpdateDataRange()
        {
            Vector64 p;
            for(int i=0; i<pScatterPoint.Count;i++)
            {
                p = pScatterPoint[i];
                if (i == 0)
                {
                    minx = maxx = p.X;
                    miny = maxy = p.Y;
                    minz = maxz = p.Z;
                    minv = maxv = p.V;
                }
                else
                {
                    if (p.X < minx) minx = p.X;
                    if (p.Y < miny) miny = p.Y;
                    if (p.Z < minz) minz = p.Z;
                    if (p.V < minv) minv = p.V;
                    if (p.X > maxx) maxx = p.X;
                    if (p.Y > maxy) maxy = p.Y;
                    if (p.Z > maxz) maxz = p.Z;
                    if (p.V > maxv) maxv = p.V;
                }
            }
        }
        public int DataSize()
        {
            return pScatterPoint.Count;
        }
        public void AddPoint(double x, double y, double z, double v)
        {
            AddPoint(new Vector64(x, y, z, v));
        }      
       
        private bool IsInSearchRange(double xl, double yl, double zl,out double dist )
        {
            dist = 0;
            switch (nSearchMode)
            {
                case SearchMethod.none:
                    dist = Math.Round(Math.Sqrt(xl * xl + yl * yl + zl * zl),10);
                    return true;
#pragma warning disable CS0162 // 检测到无法访问的代码
                    break;
#pragma warning restore CS0162 // 检测到无法访问的代码
                case SearchMethod.Sphere:
                case SearchMethod.MultiAngle:
                    if (xl > searchDist && searchDist > 0 ) return false;
                    if (yl > searchDist && searchDist > 0) return false;
                    if (zl > searchDist && searchDist > 0) return false;
                    dist = Math.Round(Math.Sqrt(xl * xl + yl * yl + zl * zl), 10);
                    if (dist > searchDist && searchDist > 0) return false;
                    return true;
#pragma warning disable CS0162 // 检测到无法访问的代码
                    break;
#pragma warning restore CS0162 // 检测到无法访问的代码
                case SearchMethod.Cube:
                    if (xl > searchDistX && searchDistX > 0) return false;
                    if (yl > searchDistY && searchDistY > 0) return false;
                    if (zl > searchDistZ && searchDistZ > 0) return false;
                    dist = Math.Round(Math.Sqrt(xl * xl + yl * yl + zl * zl), 10);
                    return true;
#pragma warning disable CS0162 // 检测到无法访问的代码
                    break;
#pragma warning restore CS0162 // 检测到无法访问的代码
            }
            return false;
        }
        
        private int DoSearchAll(double x0, double y0, double z0)
        {
            Vector64 p;
            double xl, yl, zl, dist;
            for (int i = 0; i < pScatterPoint.Count; i++)
            {
                p = pScatterPoint[i];
                xl = Math.Round(XRatio * (x0 - p.X), 10);
                yl = Math.Round(YRatio * (y0 - p.Y), 10);
                zl = Math.Round(ZRatio * (z0 - p.Z), 10);
                dist = Math.Sqrt(xl * xl + yl * yl + zl * zl);
                if (dist > searchDist) continue;
                if (IsZero(dist))
                {
                    zeroList.AddNode(i, dist);
                }
                else
                {
                    if (zeroList.Size() == 0)
                    {
                        pointList.AddNode(i, dist);
                    }
                }

            }//for (int i = 0; i < pScatterPoint.Count; i++)
            return pointList.Size();
        }

        private int DoSearchSphereAndCube(double x0,double y0,double z0)
        {
            Vector64 p;
            double xl, yl, zl, dist;            
            for (int i = 0; i < pScatterPoint.Count; i++)
            {
                p = pScatterPoint[i];

                xl = Math.Round(XRatio * (x0 - p.X), 10);
                yl = Math.Round(YRatio * (y0 - p.Y), 10);
                zl = Math.Round(ZRatio * (z0 - p.Z), 10);
                if (xl < 0) xl = -xl;
                if (yl < 0) yl = -yl;
                if (zl < 0) zl = -zl;

                if (!IsInSearchRange(xl, yl, zl, out dist)) continue;

                //add to list
                if ( IsZero( dist ) )
                {
                    zeroList.AddNode(i, dist);
                }
                else
                {
                    if( zeroList.Size() == 0 )
                    {
                        pointList.AddNode(i, dist);
                    }
                }
            }//for (int i = 0; i < pScatterPoint.Count; i++)
            return pointList.Size();
        }
       
        private int DoAngleSearch(double x0, double y0, double z0)
        {
            InitAngles();

            //select points in valid ranges to pSearchedPoints
            Vector64 p;
            
            //calculate direction and delivered to angle array
            double dist, dist1;
            double angle_hor, angle_vert;
            double x, y, z,xl,yl,zl;
            for (int i = 0; i < pScatterPoint.Count; i++)
            {
                p = pScatterPoint[i];

                x = Math.Round(XRatio * (x0 - p.X),10);
                y = Math.Round(YRatio * (y0 - p.Y),10);
                z = Math.Round(ZRatio * (z0 - p.Z),10);

                xl = x;yl = y;zl = z;
                if (xl < 0) xl = -xl;
                if (yl < 0) yl = -yl;
                if (zl < 0) zl = -zl;
                if (!IsInSearchRange(xl, yl, zl, out dist)) continue;
              
                if ( IsZero(dist) )
                {
                    zeroList.AddNode(i, dist);
                }
                else
                {
                    //zero array exist,ignore orther points
                    if (zeroList.Size() > 0) continue;

                    //-90  -  90
                    angle_vert = toAngle(Math.Asin(Math.Abs(y) / dist));
                    if (y < 0) angle_vert = 90 - angle_vert;
                    else if (y > 0) angle_vert = 90 + angle_vert;
                    else if (y == 0) angle_vert = 90;
                    //
                    if (x == 0 && z == 0) { angle_hor = 0; }
                    else if (x == 0 && z < 0) { angle_hor = 90; }
                    else if (x == 0 && z > 0) { angle_hor = 270; }
                    else if (x < 0 && z == 0) { angle_hor = 180; }
                    else
                    {
                        dist1 = Math.Round(Math.Sqrt(x * x + z * z),10);  //projection on XOZ      

                        if (dist1 == 0) { angle_hor = 0; }
                        else
                        {
                            angle_hor = toAngle(Math.Asin(Math.Abs(z) / dist1));
                            if (x > 0 && z < 0) { } // 1
                            else if (x < 0 && z < 0) { angle_hor = 180 - angle_hor; }//2
                            else if (x < 0 && z > 0) { angle_hor = angle_hor + 180; }//3
                            else if (x > 0 && z > 0) { angle_hor = 360 - angle_hor; }//4
                        }
                    }
                    int id = GetAngleStripIndex(angle_hor, angle_vert);
                    pAngledPoints[id].AddNode(i, dist);
                }
            }

            //zero array exist,ignore orther points
            if (zeroList.Size() > 0) return zeroList.Size();

            //sort point on distance
            for (int i = 0; i < pAngledPoints.Length; i++)
            {
                pAngledPoints[i].Sort();
            }

            //re-pick points,pick nearest 4 points on each direction            
            IndexedNode node;
            CSearchedList s;
            for (int i = 0; i < pAngledPoints.Length; i++)
            {
                s = pAngledPoints[i];
                for (int j = 0; j < s.points.Count && j <= minNum; j++)
                {
                    node = s.points[j];
                    pointList.points.Add(node);
                }
            }
            return pointList.Size();
        }
        private int DoSearch(double x0,double y0,double z0)
        {
            zeroList.Clear();
            pointList.Clear();
            switch(nSearchMode)
            {
                case SearchMethod.none:
                    return DoSearchAll(x0, y0, z0);
#pragma warning disable CS0162 // 检测到无法访问的代码
                    break;
#pragma warning restore CS0162 // 检测到无法访问的代码
                case SearchMethod.Sphere:                    
                case SearchMethod.Cube:
                    return DoSearchSphereAndCube(x0, y0, z0);
#pragma warning disable CS0162 // 检测到无法访问的代码
                    break;
#pragma warning restore CS0162 // 检测到无法访问的代码
                case SearchMethod.MultiAngle:
                    return DoAngleSearch(x0, y0, z0);
#pragma warning disable CS0162 // 检测到无法访问的代码
                    break;
#pragma warning restore CS0162 // 检测到无法访问的代码
            }
            return 0;
        }
        private double GetFenmu()
        {
            double fenmu = 0;
            if ( zeroList.Size() > 0) return zeroList.Size();
            else
            {
                for (int i = 0; i < pointList.Size(); i++)
                {
                    fenmu += (1.0 / pointList.points[i].distance);
                }
            }
            return fenmu;
        }
       
        public double GetInterValue(double x0, double y0, double z0)
        {
            int id;
            Vector64 p;
            double dist, a, v = 0;
            DoSearch(x0, y0, z0);

            double fenmu = GetFenmu();
            if (fenmu == 0) return 0;

            if (zeroList.Size() > 0)
            {
                for (int i = 0; i < zeroList.Size(); i++)
                {
                    id = zeroList.points[i].index;
                    p = pScatterPoint[id];
                    v += p.V;
                }
                v = v / fenmu;
            }
            else
            {
                for (int i = 0; i < pointList.Size(); i++)
                {
                    id = pointList.points[i].index;
                    dist = pointList.points[i].distance;
                    p = pScatterPoint[id];
                    a = 1.0 / dist;
                    v += (a * p.V / fenmu);
                }                
            }
            return v;
        }
    }
    public struct XYZPoint
    {
        public float x;
        public float y;
        public float z;
        public float v;
        public double d;    //该点距目标点的距离
        public double w;    //权重
        public XYZPoint(double _x, double _y, double _z, double _v)
        {
            x = (float)_x;
            y = (float)_y;
            z = (float)_z;
            v = (float)_v;
            d = w = 0;
        }
    }
    //夹角定权反距离加权插值
    public class AngledInversePower: InversePower
    {
        //Points64 所有样本点数组
        public override double GetInterpolatedValue(double x, double y, double z)
        {
            if (Count == 0) return double.NaN;

            int id;
            if ( !GetDistances(x, y, z, out id) )//存在距离为0的值
                return Points[id].v;
            else //距离不为0的值
            {
                double val = 0;
                double fenmu = GetFenmu();
                for (int i = 0; i < Points.Count; i++)
                    val += (Points[i].v * Distances[i] / fenmu);
                return val;
            }
        }

    }
    public class InversePower
    {        
        public int Power = 2;
        public double ZeroValue = 1.0E-20;
        
        public List<Vector32> Points = new List<Vector32>();
        public List<double[]> Values = new List<double[]>();

        public double[] Distances = null;
        public int Count 
        { 
            get 
            { 
                return Points.Count;
            }            
        }
        public int ValuesLength
        {
            get
            {
                if ( Values == null ) return 0;
                if ( Values.Count < 1) return 0;
                if (Values[0] == null) return 0;
                return Values[0].Length;
            }
        }
        public void AddPoint(double x, double y, double z, double[]values)
        {
            Points.Add(new Vector32(x, y, z));
            Values.Add(values);
        }
        public void AddPoint(double x, double y, double z, double v)
        {
            Points.Add(new Vector32(x, y, z, v));
        }
        public void AddPoint(Vector32 p)
        {
            Points.Add(p);
        }        
        public void AddPoints(List<Vector32> points)
        {
            Points = points;            
        }
        public virtual void Clear() 
        { 
            Points.Clear();
            Distances = null; 
        }

        /// <summary>
        /// 每一个离散点至目标点的距离
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="id">距离为0的点序号</param>
        /// <returns>true计算完毕，false存在距离为0的点</returns>
        public virtual bool GetDistances(double x, double y, double z, out int id)
        {
            Vector32 p;
            double rr;
            id = -1;            
            if( Distances == null ) Distances = new double[Count];
            for (int i = 0; i < Count; i++)
            {
                p = Points[i];
                rr = (x - p.x) * (x - p.x) + 
                     (y - p.y) * (y - p.y) + 
                     (z - p.z) * (z - p.z);

                if (rr <= ZeroValue ) 
                { 
                    id = i;
                    return false; 
                }

                Distances[i] = rr;
            }
            return true;
        }
        
        //获取分母，将距离变成权重
        public virtual double GetFenmu()
        {
            double fenmu = 0;
            if( Power == 1 )
            {
                //Distances[i]存放的是距离平方
                for (int i = 0; i < Distances.Length; i++)
                {
                    //距离变权值1/Dist
                    Distances[i] = 1.0 / Math.Sqrt(Distances[i]);
                    fenmu += Distances[i];
                }
            }
            else if (Power == 2)//Distances[i]存放的是距离平方
            {
                for (int i = 0; i < Distances.Length; i++)
                {
                    Distances[i] = (1.0 / Distances[i]);
                    fenmu += Distances[i];
                }   
            }
            else//Distances[i]存放的是距离平方
            {
                for (int i = 0; i < Distances.Length; i++)
                { 
                    Distances[i] = 1.0 / Math.Pow(Math.Sqrt(Distances[i]), Power);
                    fenmu += Distances[i];
                }
            }            
            return fenmu;
        }
        public virtual double GetInterpolatedValue(double x, double y, double z)
        {
            if ( Count == 0 ) return double.NaN;
            int id;
            //存在距离为0的点
            if ( !GetDistances(x, y, z, out id) ) return Points[id].v;
            else
            {
                double val = 0;
                double fenmu = GetFenmu();
                for (int i = 0; i < Points.Count; i++)
                    val += (Points[i].v * Distances[i] / fenmu);
                return val;
            }
        } //GetInterpolatedValue(double x, double y, double z)    
        public virtual double[]GetInterpolatedValues(double x, double y, double z)
        {
            if (Count == 0) return null;
            int id;
            //存在距离为0的点
            if ( !GetDistances(x, y, z, out id) ) return Values[id];
            else
            {
                double val;
                double fenmu = GetFenmu();

                double[] values = new double[ValuesLength];

                for (int k = 0; k < ValuesLength; k++)
                {
                    val = 0;                    
                    for (int i = 0; i < Points.Count; i++)
                    {
                        val += ( Values[i][k] * Distances[i] / fenmu );
                    }
                    values[k] = val;
                }
                return values;
            }
        } //GetInterpolatedValue(double x, double y, double z)   
        public virtual double GetInterpolatedValueByIndices(double x, double y, double z, List<int>indices,List<Vector64>_points)
        {            
            double rr;
            
            if(Distances == null )Distances = new double[indices.Count];

            Vector64 p;
            for (int i = 0; i < indices.Count; i++)
            {
                p = _points[indices[i]];
                rr = (x - p.x) * (x - p.x) + 
                     (y - p.y) * (y - p.y) + 
                     (z - p.z) * (z - p.z);
                if (rr <= ZeroValue ) { Distances = null; return p.V; }
                Distances[i] = rr;
            }

            double val = 0;
            double fenmu = GetFenmu();//计算分母并把距离变权重
            for (int i = 0; i < indices.Count; i++)
            {
                p = _points[indices[i]];
                val += ( p.V * Distances[i] / fenmu); 
            }
            return val;
        } //GetInterpolatedValueByIndices(double x, double y, double z)    

        /// <summary>
        /// 从数组中取某些点加权插值
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="indices">加权点索引号</param>
        /// <param name="_points">数组</param>
        /// <returns></returns>
        public virtual double GetInterpolatedValueByIndices(double x, double y, double z, 
                                                            List<int> indices, 
                                                            List<Vector32> _points)
        {
            if (indices.Count < 1) return double.NaN;
            double rr;            
            Distances = new double[indices.Count];
            Vector32 p;
            for (int i = 0; i < indices.Count; i++)
            {
                p = _points[indices[i]];
                rr = (x - p.x) * (x - p.x) + (y - p.y) * (y - p.y) + (z - p.z) * (z - p.z);
                if (rr == 0) 
                {
                    Distances = null; return p.V; 
                    //rr = 1.0E-6;
                }
                Distances[i] = rr;
            }
            double val = 0;
            double fenmu = GetFenmu();//计算分母并把距离变权重
            for (int i = 0; i < indices.Count; i++)
            {
                p = _points[indices[i]];
                val += (p.V * Distances[i] / fenmu);
            }
            Distances = null;
            return val;
        } //GetInterpolatedValueByIndices(double x, double y, double z)    
    }//public class InversePower

    public struct MultiValuesPoint
    {
        public double X;
        public double Y;
        public double Z;
        public List<double> Values;        
        public MultiValuesPoint(double x,double y,double z)
        {
            X = x;
            Y = y;
            Z = z;
            Values = new List<double>();
        }
        public MultiValuesPoint(double x, double y, double z,List<double>values)
        {
            X = x;
            Y = y;
            Z = z;
            Values = new List<double>();
            Values.AddRange(values);
        }
    }
    /// <summary>
    /// 多属性同时插值版本
    /// </summary>
    public class InversePowerMultiProperties
    {
        public int Power = 2;
        public List<MultiValuesPoint> Points = new List<MultiValuesPoint>();
        public double[] Distances = null;
        public int Count
        {
            get
            {
                return Points.Count;                
            }
        }

        public void AddPoint(double x, double y, double z, List<double> values)
        {
            MultiValuesPoint p = new MultiValuesPoint(x, y, z, values);
            Points.Add(p);            
        }
        public void AddPoint(double x, double y, double z, double[]values)
        {
            MultiValuesPoint p = new MultiValuesPoint(x, y, z, new List<double>(values) );
            Points.Add(p);
        }
        public void AddPoints(List<MultiValuesPoint> points)
        {
            Points = points;            
        }
        public int PropertiesCount 
        { 
            get 
            {
                if (Count < 1) return 0;
                else return Points[0].Values.Count;
            } 
        }
        public virtual void Clear()
        {
            Points.Clear();            
            Distances = null;
        }        
        public virtual bool GetDistances(double x, double y, double z, out int id)
        {
            MultiValuesPoint p;
            double rr;
            id = -1;
            if (Distances == null) Distances = new double[Count];
            for (int i = 0; i < Count; i++)
            {
                p = Points[i];
                rr = (x - p.X) * (x - p.X) + (y - p.Y) * (y - p.Y) + (z - p.Z) * (z - p.Z);
                if (rr == 0)
                {
                    id = i;
                    return false;
                }
                Distances[i] = rr;
            }
            return true;
        }
        //获取分母，将距离变成权重
        public virtual double GetFenmu()
        {
            double fenmu = 0;
            if (Power == 1)
            {
                //存放的是距离平方
                for (int i = 0; i < Distances.Length; i++)
                {
                    //距离变权值1/Dist
                    Distances[i] = 1.0 / Math.Sqrt(Distances[i]);
                    fenmu += Distances[i];
                }
            }
            else if (Power == 2)
            {
                for (int i = 0; i < Distances.Length; i++)
                {
                    Distances[i] = (1.0 / Distances[i]);
                    fenmu += Distances[i];
                }
            }
            else
            {
                for (int i = 0; i < Distances.Length; i++)
                {
                    Distances[i] = 1.0 / Math.Pow(Distances[i], Power * 0.5);
                    fenmu += Distances[i];
                }
            }
            return fenmu;
        }

        public virtual List<double> GetInterpolatedValues(double x, double y, double z)
        {
            if (Count == 0) return null;
            int id;
            //存在距离为0的值
            if ( !GetDistances(x, y, z, out id) )
            { 
                return Points[id].Values; 
            }
            else
            {
                double fenmu = GetFenmu();
                List<double> values = new List<double>();
                MultiValuesPoint p;
                int count = 0;
                for (int k = 0; k < PropertiesCount; k++)
                {
                    double sum = 0;
                    count = 0;
                    for (int i = 0; i < Points.Count; i++)
                    {
                        p = Points[i];
                        if ( p.Values == null ) continue;
                        if ( p.Values.Count != PropertiesCount) continue;
                        if ( !double.IsNaN(p.Values[k]) )
                        {
                            sum += (p.Values[k] * Distances[i] / fenmu);
                            count++;
                        }
                    }
                    if(count >0) values.Add(sum);
                    else values.Add(double.NaN);
                }
                return values;
            }
        } //GetInterpolatedValue(double x, double y, double z)    
        
    }
}
