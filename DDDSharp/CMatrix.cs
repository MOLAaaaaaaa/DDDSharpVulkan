using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDSharp
{
    class CMatrix
    {
        public	int m = 0;
        public int n = 0;
        double[,]arr;
        public CMatrix()
        {
        }
        public CMatrix(int m1, int n1)
        {
            m = m1;
            n = n1;
            arr = new double[m, n];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                    arr[i,j] = 0.0;
            }
        }
        
        private void swaprows(double[,] arr, int row0, int row1,int ncol)
        {
            double[] temp = new double[ncol];
            for (int i = 0; i < ncol; i++)            
                temp[i] = arr[row0, i];            
            for (int i = 0; i < ncol; i++)
                arr[row0,i] = arr[row1,i];
            for (int i = 0; i < ncol; i++)
                arr[row1,i] = temp[i];
        }
        public bool set(int row, int col, double x)
        {
            if (row < 0 || row >= m || col < 0 || col >= n)
                return false;            
            arr[row, col] = x;
            return true;
        }
        public double get(int row, int col)
        {
            return arr[row, col];
        }
        public int homotype(CMatrix x, CMatrix y)
        {
            if (x.m == y.m && x.n == y.n)
                  return 1;
            else  return 0;
        }
        public int multipliable(CMatrix x, CMatrix y)
        {
            if (x.n == y.m)
                return 1;
            else return 0;
        }

        static public CMatrix operator +(CMatrix x, CMatrix y)
        {   
            CMatrix z = new CMatrix(x.m, x.n);
            for (int i = 0; i < z.m; i++)
              for (int j = 0; j < z.n; j++)
                 z.arr[i, j] = x.arr[i, j] + y.arr[i, j];
            return z;            
        }
        static  public CMatrix operator -(CMatrix x, CMatrix y)
        {
            CMatrix z = new CMatrix(x.m, x.n);
            for (int i = 0; i < z.m; i++)
                for (int j = 0; j < z.n; j++)
                    z.arr[i, j] = x.arr[i, j] - y.arr[i, j];
            return z;
        }
        static public CMatrix operator *(CMatrix x, CMatrix y)
        {
            CMatrix z = new CMatrix(x.m,y.n);            
            for (int i = 0; i < x.m; i++)
            {
                for (int j = 0; j < y.n; j++)
                {
                    z.arr[i,j] = 0.0;
                    for (int k = 0; k < x.n; k++)
                        z.arr[i,j] += (x.arr[i,k] * y.arr[k,j]);
                }
            }            
            return z;
        }
	    public double det(CMatrix x)
        {
            if (!x.issquare())
            {
                //cout<<"²»ÊÇ·½Õó!"<<endl;
                return 9.99;
            }
            if (x.m == 0) return 0;
            else if (x.m == 1) return x.arr[0,0];
            else if (x.m == 2) return (x.arr[0,0] * x.arr[1,1] - x.arr[0,1] * x.arr[1,0]);
            else
            {
                double num = 0;
                int a = 1;
                for (int i = 0; i < x.m; i++)
                {
                    num = num + a * x.arr[0,i] * det(x.left(0, i));
                    //°´µÚ0ÐÐÕ¹¿ª
                    a = -a;
                }
                return num;
            }
        }

        public CMatrix GaussJordan(CMatrix lm, CMatrix rm)
        {
            int row, col, dindex;

            if (lm.m != rm.m || lm.m != lm.n)
                return null;
            int nrows = lm.m;
            int ncolsrhs = rm.n;

            double[,] arr = new double[nrows, nrows+ncolsrhs];            

            for (row = 0; row < nrows; ++row)
            {
                for (col = 0; col < nrows; ++col)
                {
                    arr[row,col] = lm.arr[row,col];
                }
                for (col = nrows; col < nrows + ncolsrhs; ++col)
                {
                    arr[row,col] = rm.arr[row,col - nrows];
                }
            }

            //	perform forward elimination to get arr in row-echelon form
            for (dindex = 0; dindex < nrows; ++dindex)
            {
                //	run along diagonal, swapping rows to move zeros in working position 
                //	(along the diagonal) downwards
                if ((dindex == (nrows - 1)) && (arr[dindex,dindex] == 0))
                {
                    return null; //  no solution
                }
                else if (arr[dindex,dindex] == 0)
                {
                    for (row = dindex + 1; row < nrows; ++row)
                    {
                        if (arr[row,dindex] != 0)
                            swaprows(arr, dindex, row, nrows + ncolsrhs);
                    }
                }
                //	divide working row by value of working position to get a 1 on the
                //	diagonal
                if (arr[dindex,dindex] == 0.0)
                {
                    return null;
                }
                else
                {
                    double tempval = arr[dindex,dindex];
                    for (col = 0; col < nrows + ncolsrhs; ++col)
                    {
                        arr[dindex,col] /= tempval;
                    }
                }

                //	eliminate value below working position by subtracting a multiple of 
                //	the current row
                for (row = dindex + 1; row < nrows; ++row)
                {
                    double wval = arr[row,dindex];
                    for (col = 0; col < nrows + ncolsrhs; ++col)
                    {
                        arr[row,col] -= wval * arr[dindex,col];
                    }
                }
            }

            //	backward substitution steps
            for (dindex = nrows - 1; dindex >= 0; --dindex)
            {
                //	eliminate value above working position by subtracting a multiple of 
                //	the current row
                for (row = dindex - 1; row >= 0; --row)
                {
                    double wval = arr[row,dindex];
                    for (col = 0; col < nrows + ncolsrhs; ++col)
                    {
                        arr[row,col] -= wval * arr[dindex,col];
                    }
                }
            }

            //	assign result
            CMatrix result = new CMatrix(nrows, ncolsrhs);
            for (row = 0; row < nrows; ++row)
            {
                for (col = 0; col < ncolsrhs; ++col)
                {
                    result.set(row, col, arr[row,col + nrows]);
                }
            } 
            return result;

        }
        public bool issquare()
        {
            return (m == n);
        }

        public CMatrix left(int x, int y)
        {   
            if ((x >= m) || (y >= n))
            {
                return null;
            }            
            CMatrix leftmatrix = new CMatrix(m-1, n-1);
            int testx = 0,testy = 0;
            for (int i = 0; i < leftmatrix.m; i++)
            {
                testy = 0;
                for (int j = 0; j < leftmatrix.n; j++)
                {
                    if (i == x)testx = 1;
                    if (j == y)testy = 1;
                    if ( testx==0 && testy ==0 )
                        leftmatrix.arr[i,j] = arr[i,j];
                    else if (testx==1 && testy==0 )
                        leftmatrix.arr[i,j] = arr[i + 1,j];
                    else if (testx==0 && testy==1)
                        leftmatrix.arr[i,j] = arr[i,j + 1];
                    else
                        leftmatrix.arr[i,j] = arr[i + 1,j + 1];
                }
            }
            return leftmatrix;
        }
        public CMatrix transpose()
        {
            CMatrix transposed = new CMatrix(n, m);
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    transposed.arr[j,i] = arr[i,j];
                }
            }
            return transposed;
        }        
    }
}
