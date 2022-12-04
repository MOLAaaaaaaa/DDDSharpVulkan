using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCollection;
namespace DDDSharp
{
    class CBezier
    {
        public List<XYZV_DOUBLE_POINT> ctrlPoints = new List<XYZV_DOUBLE_POINT>();        
        public CBezier() { }
        public void AddCtrlPoint(double x,double y,double z)
        {
            XYZV_DOUBLE_POINT p = new XYZV_DOUBLE_POINT();
            p.x = x;
            p.y = y;
            p.z = z;
            p.v = z;
            ctrlPoints.Add(p);
        }
        /*
         * -----------------最小二乘插值--------------------
        形参与函数类型  参数意义
        double x[n] 存放给定数据点的n个X坐标
        double y[m] 存放给定数据的m个Y坐标
        double z[n][m]  存放矩形居于内n×m个网点上的函数值
        int n   X坐标个数
        int m   Y坐标个数
        Double a[p][q]  返回二元拟合多项式
        int q   拟合多项式中x的最高次数加1。要求p≤n，且p≤20，若不满足这个条件，本函数自动取p=min{n，20}
        Double dt[3]    dt[0]返回拟合多项式与数据点误差的平方和，dt[1]返回拟合多项式与数据点误差绝对值之和，
        dt[2]返回拟合多项式与数据点误差绝对值的最大值        
        */
        public void pir2(double []x, double[] y, double[] z, int n, int m, double[] a, int p, int q, double[] dt)
        {
            int i, j, k, l, kk;
            double[] apx = new double[20];
            double[] apy = new double[20];
            double[] bx = new double[20];
            double[] by = new double[20];
            double[,] u = new double[20,20];
            double[] t = new double[20];
            double[] t1 = new double[20];
            double[] t2 = new double[20];
            double xx, yy, d1, d2, g=0, g1, g2;
            double x2, dd, y1, x1;
            double[] v = new double[20];            
            for (i=0; i<=p-1; i++)
            {
                l =i* q;
                for (j=0; j<=q-1; j++)
                    a[l + j]=0.0;
            }
            if (p>n) p=n;
            if (p>20) p=20;
            if (q>m) q=m;
            if (q>20) q=20;
            xx=0.0;
            for (i=0; i<=n-1; i++) xx=xx+x[i]/(1.0* n);
            yy=0.0;
            for (i=0; i<=m-1; i++) yy=yy+y[i]/(1.0* m);
            d1=1.0* n; apx[0]=0.0;
            for (i=0; i<=n-1; i++) apx[0]=apx[0]+x[i]-xx;
            apx[0]=apx[0]/d1;
            for (j=0; j<=m-1; j++)
            {
                v[j]=0.0;
                for (i=0; i<=n-1; i++)v[j]=v[j]+z[i * m + j];
                v[j]=v[j]/d1;
            }
            if (p>1)
            {
                d2 =0.0; apx[1]=0.0;
                for (i=0; i<=n-1; i++)
                {
                    g =x[i]-xx-apx[0];
                    d2=d2+g* g;
                    apx[1]=apx[1]+(x[i]-xx)* g* g;
                }
                apx[1]=apx[1]/d2;
                bx[1]=d2/d1;
                for (j=0; j<=m-1; j++)
                {
                    v[m + j]=0.0;
                    for (i=0; i<=n-1; i++)
                    {
                        g =x[i]-xx-apx[0];
                        v[m + j]=v[m + j]+z[i * m + j]* g;
                    }
                    v[m + j]=v[m + j]/d2;
                }
                d1=d2;
            }
            for (k=2; k<=p-1; k++)
            {
                d2 =0.0; apx[k]=0.0;
                for (j=0; j<=m-1; j++) v[k * m + j]=0.0;
                for (i=0; i<=n-1; i++)
                {
                    g1 =1.0; g2=x[i]-xx-apx[0];
                    for (j=2; j<=k; j++)
                    {
                        g =(x[i]-xx-apx[j - 1])* g2-bx[j - 1]* g1;
                        g1=g2; g2=g;
                    }
                    d2=d2 + g*g;
                    apx[k]=apx[k]+(x[i]-xx)* g*g;
                    for (j=0; j<=m-1; j++)
                        v[k * m + j]=v[k * m + j]+z[i * m + j]* g;
                }
                for (j=0; j<=m-1; j++)v[k * m + j]=v[k * m + j]/d2;
                apx[k]=apx[k]/d2;
                bx[k]=d2/d1;
                d1=d2;
            }
            d1=m;
            apy[0]=0.0;
            for (i=0; i<=m-1; i++) apy[0]=apy[0]+y[i]-yy;
            apy[0]=apy[0]/d1;
            for (j=0; j<=p-1; j++)
            {
                u[j,0]=0.0;
                for (i=0; i<=m-1; i++) u[j,0]=u[j,0]+v[j * m + i];
	            u[j,0]=u[j,0] / d1;
            }
            if (q>1)
            {
                d2 =0.0; apy[1]=0.0;
                for (i=0; i<=m-1; i++)
                {
                    g =y[i]-yy-apy[0];
                    d2=d2+g* g;
                    apy[1]=apy[1]+(y[i]-yy)* g* g;
                }
                apy[1]=apy[1]/d2;
                by[1]=d2/d1;
                for (j=0; j<=p-1; j++)
	            {
                    u[j,1]=0.0;
                    for (i=0; i<=m-1; i++)
                    {
                        g =y[i]-yy-apy[0];
		                u[j,1]=u[j,1]+v[j * m + i]* g;
                    }
	                u[j,1]=u[j,1]/d2;
                 }
                d1=d2;
            }
            for (k=2; k<=q-1; k++)
            {
                d2 =0.0; apy[k]=0.0;
	            for (j=0; j<=p-1; j++) u[j,k]=0.0;
                for (i=0; i<=m-1; i++)
                {
                    g1 =1.0;
                    g2=y[i]-yy-apy[0];
                    for (j=2; j<=k; j++)
                    {
                        g =(y[i]-yy-apy[j - 1])* g2-by[j - 1]* g1;
                        g1=g2; g2=g;
                    }
                    d2=d2+g* g;
                    apy[k]=apy[k]+(y[i]-yy)* g* g;
                    for (j=0; j<=p-1; j++)
	                    u[j,k] = u[j,k]+v[j * m + i]* g;
                }
                for (j=0; j<=p-1; j++) u[j,k] = u[j,k] / d2;
                apy[k]=apy[k]/d2;
                by[k]=d2/d1;
                d1=d2;
            }
            v[0]=1.0;
            v[m]=-apy[0];
            v[m + 1]=1.0;
            for (i=0; i<=p-1; i++)
                for (j=0; j<=q-1; j++)
                    a[i * q + j]=0.0;
            for (i=2; i<=q-1; i++)
            {
                v[i * m + i]=v[(i - 1) * m + (i - 1)];
                v[i * m + i - 1]=-apy[i - 1]* v[(i - 1) * m + i - 1]+v[(i - 1) * m + i - 2];
                if (i>=3)
                    for (k=i-2; k>=1; k--)
                        v[i * m + k]=-apy[i - 1]* v[(i - 1) * m + k]+ v[(i - 1) * m + k - 1]-by[i - 1]* v[(i - 2) * m + k];

                v[i * m]=-apy[i - 1]* v[(i - 1) * m]-by[i - 1]* v[(i - 2) * m];
            }
            for (i=0; i<=p-1; i++)
            {
                if (i==0) { t[0]=1.0; t1[0]=1.0;}
                else
                {
                    if (i==1)
                    {
                        t[0]=-apx[0]; t[1]=1.0;
                        t2[0]=t[0]; t2[1]=t[1];
                    }
                    else
                    {
                        t[i]=t2[i - 1];
                        t[i - 1]=-apx[i - 1]* t2[i - 1]+t2[i - 2];
                        if (i>=3)
                        for (k=i-2; k>=1; k--)
                            t[k]=-apx[i - 1]* t2[k]+t2[k - 1] - bx[i - 1]* t1[k];

                        t[0]=-apx[i - 1]* t2[0]-bx[i - 1]* t1[0];
                        t2[i]=t[i];

                        for (k=i-1; k>=0; k--)
                        {
                            t1[k]=t2[k];
                            t2[k]=t[k];
                        }
                    }
                }
                for (j=0; j<=q-1; j++)
                    for (k=i; k>=0; k--)
                        for (l=j; l>=0; l--)
	                        a[k * q + l] = a[k * q + l]+u[i,j]* t[k]* v[j * m + l];
            }
            dt[0] = dt[1] = dt[2] = 0.0;
            for (i=0; i<=n-1; i++)
            {
                x1 =x[i]-xx;
                for (j=0; j<=m-1; j++)
                {
                    y1 =y[j]-yy;
                    x2=1.0; dd=0.0;
                    for (k=0; k<=p-1; k++)
                    {
                        g =a[k * q + q - 1];
                        for (kk=q-2; kk>=0; kk--) g = g * y1 + a[k * q + kk];
                        g=g* x2;
                        dd =dd+g;
                        x2 =x2* x1;
                    }
                    dd=dd-z[i * m + j];
                    if(Math.Abs(dd)>dt[2]) dt[2] = Math.Abs(dd);
                    dt[0]=dt[0]+dd* dd;
                    dt[1]=dt[1]+ Math.Abs(dd);
                }
            }    
            return;
        }
    };

    class CBezierSurface
    {
        public bool fitted = false;
        List<XYZV_DOUBLE_POINT> ctrlPoints;  //[4][4][3];
        List<XYZV_DOUBLE_POINT> ctrlPoints2; //[4][4][3];
	    CMatrix ctrlPMatrix;
        CMatrix ctrlPMatrix2;
        double[] dataPoints;//[10][10];							//100 data points
	    CMatrix dataPMatrix;
        double[] u;//[10][10];									//Êý¾Ýµã ²ÎÊý»¯ºóµÄ ²ÎÊý
        double[] v;//[10][10];
	    CMatrix paramMatrix;
        double Distance(double[] a, double[]b)
        {
            double xd = b[0] - a[0];
            double yd = b[1] - a[1];
            double zd = b[2] - a[2];
            double dd = xd * xd + yd * yd + zd * zd;
            return Math.Sqrt(dd);
        }
        double Bernstein(int j, int n, double t)
        {
            if (j < 0 || j > n || t < 0 || t > 1)
            {
                return 0.0;
            }

            int i;
            int comb, numerator, denominator;
            double result = 0.0;

            numerator = 1;
            for (i = n; i > n - j; i--)
            {
                numerator *= i;
            }
            denominator = 1;
            for (i = 1; i <= j; i++)
            {
                denominator *= i;
            }
            comb = numerator / denominator;

            result = comb;
            for (i = 0; i < j; i++)
            {
                result *= t;
            }
            for (; i < n; i++)
            {
                result *= (1.0 - t);
            }

            return result;
        }
        bool Fitting()
        {
            int i, j, k = 0;
            if ( Parametric() == false )
                return false;
            int m = 4, n = 4;
            CMatrix paramMatrixT = paramMatrix.transpose();
            CMatrix leftMatrix = new CMatrix(m*n, m*n);
            leftMatrix = paramMatrixT * paramMatrix;

            CMatrix rightMatrix = new CMatrix(m*n, 3);
            rightMatrix = paramMatrixT * dataPMatrix;
            ctrlPMatrix2 = rightMatrix.GaussJordan(leftMatrix, rightMatrix);

            int row, col;
            for (i = 0;i<ctrlPoints2.Count;i++)
            {
                row = i / m;
                col = i % n;
                ctrlPoints2[i].x = ctrlPMatrix2.get(row, 0);
                ctrlPoints2[i].y = ctrlPMatrix2.get(row, 1);
                ctrlPoints2[i].z = ctrlPMatrix2.get(row, 2);
            }            
            fitted = true;
            return fitted;
        }
        public bool Parametric()
        {
            /*
            int i, j, k = 0;
            double totalD, currentD;

            for (j = 0; j < 10; j++)
            {
                totalD = 0;                                                             //¼ÆËãjÁÐÊý¾ÝµãµÄ²î·ÖÊ¸Á¿Ä£Ö®ºÍ
                for (k = 0; k < 9; k++)
                {
                    totalD += Distance(dataPoints[k][j], dataPoints[k + 1][j]);
                }

                u[0][j] = 0.0;                                                          //µÚ1ÐÐÊý¾Ýµãu²ÎÊýÎª0

                for (i = 1; i < 10; i++)
                {
                    currentD = 0;
                    for (k = 0; k < i; k++)
                    {
                        currentD += Distance(dataPoints[k][j], dataPoints[k + 1][j]);   //¼ÆËãµ½jÁÐiÐÐÊý¾ÝµãµÄ²î·ÖÊ¸Á¿Ä£Ö®ºÍ
                    }

                    u[i][j] = currentD / totalD;                                        //¼ÆËã³öjÁÐiÐÐÊý¾Ýµã²ÎÊýÖµ
                }
            }

            for (i = 0; i < 10; i++)
            {
                totalD = 0;                                                             //¼ÆËãiÐÐÊý¾ÝµãµÄ²î·ÖÊ¸Á¿Ä£Ö®ºÍ
                for (k = 0; k < 9; k++)
                {
                    totalD += Distance(dataPoints[i][k], dataPoints[i][k + 1]);
                }

                v[i][0] = 0.0;                                                          //µÚ1ÁÐÊý¾Ýµãv²ÎÊýÖµÎª0
                for (j = 1; j < 10; j++)
                {
                    currentD = 0;
                    for (k = 0; k < j; k++)
                    {
                        currentD += Distance(dataPoints[i][k], dataPoints[i][k + 1]);   //¼ÆËãµ½iÐÐjÁÐÊý¾ÝµãµÄ²î·ÖÊ¸Á¿Ä£Ö®ºÍ
                    }

                    v[i][j] = currentD / totalD;                                        //¼ÆËã³öiÐÐjÁÐÊý¾Ýµã²ÎÊýÖµ
                }
            }


            double U[100], V[100];
            for (i = 0; i < 10; i++)
            {
                for (j = 0; j < 10; j++)
                {
                    U[i * 10 + j] = u[i][j];
                    V[i * 10 + j] = v[i][j];
                }
            }
            paramMatrix = GetMatrixA(U, V, 100);
         */
            return true;
        }
        CMatrix GetMatrixA(double[] u, double[] v, int num)
        {
            int i, j, k = 0;
            CMatrix matrix = new CMatrix(num, 16);
            for (; k < num; k++)
            {
                for (i = 0; i < 4; i++)
                {
                    for (j = 0; j < 4; j++)
                    {
                        matrix.set(k, i * 4 + j,Bernstein(i, 3, u[k]) * Bernstein(j, 3, v[k]) );
                    }
                }
            }
            return matrix;
        }
    };
}
