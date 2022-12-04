using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataCollection;

namespace OpenCLNet
{
    public class CLInterpolation
    {
        #region 变量定义
        public string errMessage = "";
        
        public int xGrid = 0;
        public int yGrid = 0;
        public int zGrid = 0;
        public int threadGroupSize = 0;
        public float[] grid3d = null;

        public int pointNum  { get { return points.Count; } }
        public List<Vector32> points = new List<Vector32>();
        public double minx, miny, minz, minv,maxx, maxy, maxz,maxv;

        public double totalSeconds = 0;
        public double perSeconds = 0;

        
        public double[] A = null;//距离矩阵
        public double[] E = null;//扩展矩阵
        public double[] a = null;//系数矩阵

        #endregion 变量定义

        #region Add Point Function
        public CLInterpolation()
        {
            
        }

        public void AddPoint(double x, double y, double z, double v)
        {
            points.Add(new Vector32(x,y,z,v));
        }
        public void UpdateRange()
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0)
                {
                    minx = maxx = points[i].x;
                    miny = maxy = points[i].y;
                    minz = maxz = points[i].z;
                    minv = maxv = points[i].v;
                }
                else
                {
                    if (points[i].x > maxx) maxx = points[i].x;
                    if (points[i].x < minx) minx = points[i].x;
                    if (points[i].y > maxy) maxy = points[i].y;
                    if (points[i].y < miny) miny = points[i].y;
                    if (points[i].z > maxz) maxz = points[i].z;
                    if (points[i].z < minz) minz = points[i].z;
                    if (points[i].v > maxv) maxv = points[i].v;
                    if (points[i].v < minv) minv = points[i].v;
                }
            }
        }
        public void Clear()
        {
            A = null;
            E = null;
            a = null;
            points.Clear();
            //grid3d = null;            
        }
        #endregion
        public void Inverse()
        {
#pragma warning disable CS0168 // 声明了变量“id0”，但从未使用过
            long id, id0;
#pragma warning restore CS0168 // 声明了变量“id0”，但从未使用过
            double bs;

            int row = pointNum;

            E = new double[pointNum * pointNum];
            for (int i = 0; i < row * row; i++) E[i] = 0;
            for (int i = 0; i < row; i++) E[i * row + i] = 1;

            for (int k=0;k<row;k++)
            {
                id = k * row + k;            
                bs = A[id];
                A[id] = 1;
                for (int j = k + 1; j < row; j++)
                {
                    id = k * row + j;
                    A[id] = A[id] / bs;
                }
                for (int j = 0; j < row; j++)
                {
                    id = k * row + j;
                    E[id] = E[id] / bs;
                }

                for(int i=0;i<row;i++)
                {
                    if (i != k)
                    {
                        bs = A[i * row + k];
                        for (int j = k; j < row; j++)
                        {
                            A[i * row + j] -= bs * A[k * row + j];
                        }
                        for (int j = 0; j < row; j++)
                        {
                            E[i * row + j] -= bs * E[k * row + j];
                        }
                    }
                }
            }
            A = null;
        }
        public bool StartIDW(Device oclDevice,int nx,int ny,int nz)
        {
            #region 创建变量赋值
            if (pointNum<3)
            {
                errMessage = "no enough valid points.";
                return false;
            }

            grid3d = new float[nx * ny * nz];
            if(grid3d==null)
            {
                errMessage = "no enough memory.";
                return false;
            }

            threadGroupSize = nz;// (int)oclDevice.MaxWorkGroupSize;
            float[] dists = new float[pointNum * threadGroupSize];
            if(dists == null)
            {
                errMessage = "no enough memory.";
                return false;
            }

            float[] px = new float[points.Count];
            float[] py = new float[points.Count];
            float[] pz = new float[points.Count];
            float[] pv = new float[points.Count];
            if ( px == null || py == null|| pz == null|| pv == null)
            {
                dists = grid3d = null;
                errMessage = "no enough memory.";
                return false;
            }

            for (int i = 0; i < points.Count; i++)
            {
                px[i] = points[i].x;
                py[i] = points[i].y;
                pz[i] = points[i].z;
                pv[i] = points[i].v;
            }
            #endregion 创建变量赋值

            //根据配置建立上下文
            var oclContext = oclDevice.Platform.CreateContext(
                new[] { (IntPtr)ContextProperties.PLATFORM, oclDevice.Platform.PlatformID, IntPtr.Zero, IntPtr.Zero },
                new[] { oclDevice },
                (errInfo, privateInfo, cb, userData) => { },
                IntPtr.Zero
            );

            //创建命令队列
            var oclCQ = oclContext.CreateCommandQueue(oclDevice, CommandQueueProperties.PROFILING_ENABLE);

            #region 编译代码并导出核

            //定义一个字典用来存放所有核
            var Kernels = new Dictionary<string, Kernel>();

            var oclProgram = oclContext.CreateProgramWithSource(IDWSoruce);
            try
            {
                oclProgram.Build();
            }
            catch (OpenCLBuildException EEE)
            {
                MessageBox.Show(EEE.BuildLogs[0]);
                return false;
            }

            foreach (var item in new[] { "gpuThread" })//, "gpuThread"
            {
                Kernels.Add(item, oclProgram.CreateKernel(item));
            }
            oclProgram.Dispose();
            #endregion

            #region 调用gpuThread核
            {

                //在显存创建缓冲区并把HOST的数据拷贝过去 
                var px1 = oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, px.Length * sizeof(float), px.ToFloatPtr());
                var py1 = oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, py.Length * sizeof(float), py.ToFloatPtr());
                var pz1 = oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, pz.Length * sizeof(float), pz.ToFloatPtr());
                var pv1 = oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, pv.Length * sizeof(float), pv.ToFloatPtr());

                var dist1 = oclContext.CreateBuffer(MemFlags.READ_WRITE | MemFlags.COPY_HOST_PTR, dists.Length * sizeof(float), dists.ToFloatPtr());

                //还有一个缓冲区用来接收回参
                var grid1 = oclContext.CreateBuffer(MemFlags.READ_WRITE | MemFlags.COPY_HOST_PTR, grid3d.Length * sizeof(float), grid3d.ToFloatPtr());
                //var ret = oclContext.CreateBuffer(MemFlags.READ_WRITE, res.Length * sizeof(double), IntPtr.Zero);
                //把参数填进Kernel里
                int id = 0;
                Kernels["gpuThread"].SetArg(id++, px1);
                Kernels["gpuThread"].SetArg(id++, py1);
                Kernels["gpuThread"].SetArg(id++, pz1);
                Kernels["gpuThread"].SetArg(id++, pv1);
                Kernels["gpuThread"].SetArg(id++, pointNum);
                Kernels["gpuThread"].SetArg(id++, dist1);
                Kernels["gpuThread"].SetArg(id++, nx);
                Kernels["gpuThread"].SetArg(id++, ny);
                Kernels["gpuThread"].SetArg(id++, nz);
                Kernels["gpuThread"].SetArg(id++, minx);
                Kernels["gpuThread"].SetArg(id++, miny);
                Kernels["gpuThread"].SetArg(id++, minz);
                Kernels["gpuThread"].SetArg(id++, maxx);
                Kernels["gpuThread"].SetArg(id++, maxy);
                Kernels["gpuThread"].SetArg(id++, maxz);
                Kernels["gpuThread"].SetArg(id++, grid1);

                //把调用请求添加到队列里,参数分别是:Kernel,数据的维度1,每个维度的全局工作项ID偏移0,每个维度工作项数量4,每个维度的工作组长度(这里设为每4个一组)
                oclCQ.EnqueueNDRangeKernel(Kernels["gpuThread"], 1, new[] { 0 }, new[] { nz }, new[] { nz });
                //设置栅栏强制要求上面的命令执行完才继续下面的命令.
                oclCQ.EnqueueBarrier();
                //添加一个读取数据命令到队列里,用来读取运算结果
                oclCQ.EnqueueReadBuffer(grid1, true, 0, grid3d.Length * sizeof(float), grid3d.ToFloatPtr());
                //开始执行
                oclCQ.Finish();

                px = py = pz = pv = null;
                dists = null;
                px1.Dispose();
                py1.Dispose();
                pz1.Dispose();
                pv1.Dispose();
                dist1.Dispose();
                grid1.Dispose();
            }

            #endregion

            //按顺序释放之前构造的对象
            oclCQ.Dispose();
            oclContext.Dispose();
            oclDevice.Dispose();

            return true;
        }

        private Context oclContext = null;
        private CommandQueue oclCQ = null;        
        private Dictionary <string, Kernel> Kernels = new Dictionary<string, Kernel>();

        public bool InitCL(Device oclDevice)
        {
            //根据配置建立上下文
            oclContext = oclDevice.Platform.CreateContext(
                new[] { (IntPtr)ContextProperties.PLATFORM, oclDevice.Platform.PlatformID, IntPtr.Zero, IntPtr.Zero },
                new[] { oclDevice },
                (errInfo, privateInfo, cb, userData) => { },
                IntPtr.Zero
            );

            //创建命令队列
            oclCQ = oclContext.CreateCommandQueue(oclDevice, CommandQueueProperties.PROFILING_ENABLE);

            #region 编译代码并导出核

            var oclProgram = oclContext.CreateProgramWithSource(InterpolateSource);
            try
            {
                oclProgram.Build();
            }
            catch (OpenCLBuildException EEE)
            {
                errMessage = EEE.BuildLogs[0];
                return false;
            }
            string[] functions = new string[]
            {
                "distCalculate",
                "MatrixInverse",
                "weightCalculate",
                "gridInterpolate"
            };
            foreach (var item in functions )
            {
                Kernels.Add(item, oclProgram.CreateKernel(item));
            }

            oclProgram.Dispose();

            return true;
        } //end of bool InitCL(Device oclDevice)

        public void ReleaseCL()
        {
            oclCQ.Dispose();
            oclContext.Dispose();
            Kernels.Clear();            
        }

        public bool StartDistMatrixCalculate(Device oclDevice )
        {
            #region 创建变量赋值            

            bool ret = false;

            int[] workItemSizes = new int[2];
            int[] groupItemSizes = new int[2];
            int[] globalWorkItemSizes = new int[2];

            int itemSize = (int) Math.Sqrt(oclDevice.MaxWorkGroupSize);

            workItemSizes[0] = itemSize;
            workItemSizes[1] = itemSize;

            if (pointNum <= itemSize)
            {
                workItemSizes[0] = workItemSizes[1] = pointNum;
                groupItemSizes[0] = groupItemSizes[1] = 1;
            }
            else
            {
                int np = pointNum / itemSize;
                if (pointNum % itemSize > 0) np++;
                workItemSizes[0] = workItemSizes[1] = itemSize;
                groupItemSizes[0] = groupItemSizes[1] = np;
            }

            globalWorkItemSizes[0] = globalWorkItemSizes[1] = groupItemSizes[0] * workItemSizes[0];

            float[] px;
            float[] py;
            float[] pz;
            try
            {
                A = new double[pointNum * pointNum];
                px = new float[points.Count];
                py = new float[points.Count];
                pz = new float[points.Count];
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                return ret;
            }

            for (int i = 0; i < points.Count; i++)
            {
                px[i] = (float)points[i].x;
                py[i] = (float)points[i].y;
                pz[i] = (float)points[i].z;
            }

            #endregion          

            long workgroup = oclDevice.MaxWorkGroupSize;

            #region 调用gpuThread核

            DateTime t1 = DateTime.Now;
            //在显存创建缓冲区并把HOST的数据拷贝过去                
            try
            {
                var A1 = oclContext.CreateBuffer(MemFlags.READ_WRITE | MemFlags.COPY_HOST_PTR, A.Length * sizeof(double), A.ToDoublePtr());
                var px1 = oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, px.Length * sizeof(float), px.ToFloatPtr());
                var py1 = oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, py.Length * sizeof(float), py.ToFloatPtr());
                var pz1 = oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, pz.Length * sizeof(float), pz.ToFloatPtr());

                Kernels["distCalculate"].SetArg(0, A1);
                Kernels["distCalculate"].SetArg(1, px1);
                Kernels["distCalculate"].SetArg(2, py1);
                Kernels["distCalculate"].SetArg(3, pz1);
                Kernels["distCalculate"].SetArg(4, pointNum);

                //把调用请求添加到队列里,参数分别是:Kernel,数据的维度1,每个维度的全局工作项ID偏移0,每个维度工作项数量4,每个维度的工作组长度(这里设为每4个一组)
                oclCQ.EnqueueNDRangeKernel(Kernels["distCalculate"], 2, new[] { 0, 0 }, globalWorkItemSizes, workItemSizes);
                //设置栅栏强制要求上面的命令执行完才继续下面的命令.
                oclCQ.EnqueueBarrier();
                //添加一个读取数据命令到队列里,用来读取运算结果
                oclCQ.EnqueueReadBuffer(A1, true, 0, A.Length * sizeof(double), A.ToDoublePtr());

                oclCQ.Finish();

                A1.Dispose();
                px1.Dispose();
                py1.Dispose();
                pz1.Dispose();

                ret = true;
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                ret = false;
            }
            
            px = py = pz = null;
            
            
            #endregion

            //按顺序释放之前构造的对象
           // oclCQ.Dispose();
           // oclContext.Dispose();
           // oclDevice.Dispose();

            return ret;
        }
        //计算系数矩阵
        public bool StartWeightMatrixCalculate(Device oclDevice)
        {
            #region 创建变量赋值            

            bool ret = false;

            int[] workItemSizes = new int[1];
            int[] groupItemSizes = new int[1];
            int[] globalWorkItemSizes = new int[1];

            int itemSize = (int)oclDevice.MaxWorkGroupSize;
            workItemSizes[0] = itemSize;
            if (pointNum <= itemSize)
            {
                workItemSizes[0] = pointNum;
                groupItemSizes[0] = 1;
            }
            else
            {
                int np = pointNum / itemSize;
                if (pointNum % itemSize > 0) np++;
                workItemSizes[0] = itemSize;
                groupItemSizes[0] = np;
            }

            globalWorkItemSizes[0] = groupItemSizes[0] * workItemSizes[0];

            float[] pv;
            try
            {
                a = new double[points.Count];
                pv = new float[points.Count];                
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return ret;
            }

            for (int i = 0; i < points.Count; i++)
            {
                pv[i] = (float)points[i].v;                
            }

            #endregion            

            long workgroup = oclDevice.MaxWorkGroupSize;

            #region 调用gpuThread核

            DateTime t1 = DateTime.Now;
            //在显存创建缓冲区并把HOST的数据拷贝过去                
            try
            {
                var E1 = oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR,E.Length * sizeof(double), E.ToDoublePtr());
                var pv1 = oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, pv.Length * sizeof(float), pv.ToFloatPtr());
                var a1 = oclContext.CreateBuffer(MemFlags.READ_WRITE | MemFlags.COPY_HOST_PTR, a.Length * sizeof(double), a.ToDoublePtr());

                Kernels["weightCalculate"].SetArg(0, E1);
                Kernels["weightCalculate"].SetArg(1, pv1);
                Kernels["weightCalculate"].SetArg(2, a1);
                Kernels["weightCalculate"].SetArg(3, pointNum);

                //把调用请求添加到队列里,参数分别是:Kernel,数据的维度1,每个维度的全局工作项ID偏移0,每个维度工作项数量4,每个维度的工作组长度(这里设为每4个一组)
                oclCQ.EnqueueNDRangeKernel(Kernels["weightCalculate"], 1, new[] { 0}, globalWorkItemSizes, workItemSizes);
                //设置栅栏强制要求上面的命令执行完才继续下面的命令.
                oclCQ.EnqueueBarrier();
                //添加一个读取数据命令到队列里,用来读取运算结果
                oclCQ.EnqueueReadBuffer(a1, true, 0, a.Length * sizeof(double), a.ToDoublePtr());

                oclCQ.Finish();

                E1.Dispose();
                pv1.Dispose();
                a1.Dispose();

                ret = true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                ret = false;
            }

            pv = null;
            A = null;            
           
            #endregion

            //按顺序释放之前构造的对象
           // oclCQ.Dispose();
           // oclContext.Dispose();
           // oclDevice.Dispose();

            return ret;
        }
        public bool StartMatrixInverse(Device oclDevice)
        {
            #region 创建变量赋值            
            int row = pointNum;
            E = new double[pointNum * pointNum];
            for (int i = 0; i < row * row; i++) E[i] = 0;
            for (int i = 0; i < row; i++) E[i * row + i] = 1;

            #endregion           

            int[] workItemSizes = new int[1];
            int[] groupItemSizes = new int[1];
            int[] globalWorkItemSizes = new int[1];

            int itemSize = (int)oclDevice.MaxWorkGroupSize;
            workItemSizes[0] = itemSize;
            if (pointNum <= itemSize)
            {
                workItemSizes[0] = row;
                groupItemSizes[0] = 1;
            }
            else
            {
                int np = row / itemSize;
                if (row % itemSize > 0) np++;
                workItemSizes[0] = itemSize;
                groupItemSizes[0] = np;
            }

            globalWorkItemSizes[0] = groupItemSizes[0] * workItemSizes[0];
            #endregion

            #region 调用gpuThread核

            int[] currow = new int[1];

            double bs;
            long id;
            DateTime t1 = DateTime.Now;
            for ( int k = 0; k < row; k++ )
            {
                currow[0] = k;
                id = k * row + k;

                if ( A[id] != 1 )
                {
                    bs = A[id];
                    A[id] = 1;
                    for (int j = k+1; j < row; j++)
                    {
                        id = k * row + j;
                        A[id] = A[id] / bs;
                    }
                    for (int j = 0; j < row; j++)
                    {
                        id = k * row + j;
                        E[id] = E[id] / bs;
                    }
                }
                //在显存创建缓冲区并把HOST的数据拷贝过去                
                var A1 = oclContext.CreateBuffer(MemFlags.READ_WRITE | MemFlags.COPY_HOST_PTR, A.Length * sizeof(double), A.ToDoublePtr());
                var E1 = oclContext.CreateBuffer(MemFlags.READ_WRITE | MemFlags.COPY_HOST_PTR, E.Length * sizeof(double), E.ToDoublePtr());
                var currow1 = oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, currow.Length * sizeof(int), currow.ToIntPtr());

                Kernels["MatrixInverse"].SetArg(0, A1);
                Kernels["MatrixInverse"].SetArg(1, E1);
                Kernels["MatrixInverse"].SetArg(2, currow1);
                Kernels["MatrixInverse"].SetArg(3, row);

                //把调用请求添加到队列里,参数分别是:Kernel,数据的维度1,每个维度的全局工作项ID偏移0,每个维度工作项数量4,每个维度的工作组长度(这里设为每4个一组)
                oclCQ.EnqueueNDRangeKernel(Kernels["MatrixInverse"], 1, new[] { 0 }, globalWorkItemSizes, workItemSizes );
                //设置栅栏强制要求上面的命令执行完才继续下面的命令.
                oclCQ.EnqueueBarrier();
                //添加一个读取数据命令到队列里,用来读取运算结果
                oclCQ.EnqueueReadBuffer(E1, true, 0, E.Length * sizeof(double), E.ToDoublePtr());
                oclCQ.EnqueueReadBuffer(A1, true, 0, A.Length * sizeof(double), A.ToDoublePtr());
                A1.Dispose();
                E1.Dispose();

                oclCQ.Finish();

                if (k == 0)
                {
                    perSeconds = (DateTime.Now - t1).TotalSeconds;
                    totalSeconds = perSeconds * perSeconds;
                }                
            }

            #endregion

            //按顺序释放之前构造的对象
            //oclCQ.Dispose();
            //oclContext.Dispose();
            //oclDevice.Dispose();
            A = null;
            return true;
        }

        public bool StartGridInterpolation( Device oclDevice, 
                                            double _minx,double _miny,double _minz,
                                            double _xstep, double _ystep, double _zstep,
                                            int nx,int ny,int nz )
        {
            #region 创建变量赋值            
            bool ret = false;
            xGrid = nx;
            yGrid = ny;
            zGrid = nz;          
            
            #endregion 

            #region 线程配置------------------

            int[] workItemSizes = new int[3];
            int[] groupItemSizes = new int[3];
            int[] globalWorkItemSizes = new int[3];
            int[] nums = new int[] { nx, ny, nz };

            int itemSize = (int)Math.Pow(oclDevice.MaxWorkGroupSize,1.0/3.0);

            workItemSizes[0] = itemSize;
            workItemSizes[1] = itemSize;
            workItemSizes[2] = itemSize;
            for (int i = 0; i < 3; i++)
            {
                if ( nums[i] <= itemSize )
                {
                    workItemSizes[i] = nums[i];
                    groupItemSizes[i] = 1;
                }
                else
                {
                    workItemSizes[i] = itemSize;
                    groupItemSizes[i] = nums[i] / itemSize;
                    if (nums[i] % itemSize > 0) groupItemSizes[i]++; 
                }
                globalWorkItemSizes[i] = groupItemSizes[i] * workItemSizes[i];
            }             

            #endregion 线程配置
            
            #region 调用gpuThread核

            DateTime t1 = DateTime.Now;

            try
            {
                grid3d = new float[nx * ny * nz];
                float[]px = new float[points.Count];
                float[] py = new float[points.Count];
                float[] pz = new float[points.Count];
                for (int i = 0; i < points.Count; i++)
                {
                    px[i] = (float)points[i].x;
                    py[i] = (float)points[i].y;
                    pz[i] = (float)points[i].z;
                }

                //在显存创建缓冲区并把HOST的数据拷贝过去                
                var grid1 = oclContext.CreateBuffer(MemFlags.READ_WRITE | MemFlags.COPY_HOST_PTR, grid3d.Length * sizeof(float), grid3d.ToFloatPtr());
                var a1 = oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, a.Length * sizeof(double), a.ToDoublePtr());
                var px1 = oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, px.Length * sizeof(float), px.ToFloatPtr());
                var py1 = oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, py.Length * sizeof(float), py.ToFloatPtr());
                var pz1 = oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, pz.Length * sizeof(float), pz.ToFloatPtr());

                int k = 0;
                Kernels["gridInterpolate"].SetArg(k++, grid1);
                Kernels["gridInterpolate"].SetArg(k++, a1);
                Kernels["gridInterpolate"].SetArg(k++, px1);
                Kernels["gridInterpolate"].SetArg(k++, py1);
                Kernels["gridInterpolate"].SetArg(k++, pz1);
                Kernels["gridInterpolate"].SetArg(k++, _minx);
                Kernels["gridInterpolate"].SetArg(k++, _miny);
                Kernels["gridInterpolate"].SetArg(k++, _minz);
                Kernels["gridInterpolate"].SetArg(k++, _xstep);
                Kernels["gridInterpolate"].SetArg(k++, _ystep);
                Kernels["gridInterpolate"].SetArg(k++, _zstep);
                Kernels["gridInterpolate"].SetArg(k++, nx);
                Kernels["gridInterpolate"].SetArg(k++, ny);
                Kernels["gridInterpolate"].SetArg(k++, nz);
                Kernels["gridInterpolate"].SetArg(k++, pointNum);

                //把调用请求添加到队列里,参数分别是:Kernel,数据的维度1,每个维度的全局工作项ID偏移0,每个维度工作项数量4,每个维度的工作组长度(这里设为每4个一组)
                oclCQ.EnqueueNDRangeKernel(Kernels["gridInterpolate"], 3, new[] { 0, 0, 0 }, globalWorkItemSizes, workItemSizes);
                //设置栅栏强制要求上面的命令执行完才继续下面的命令.
                oclCQ.EnqueueBarrier();
                //添加一个读取数据命令到队列里,用来读取运算结果
                oclCQ.EnqueueReadBuffer(grid1, true, 0, grid3d.Length * sizeof(float), grid3d.ToFloatPtr());

                oclCQ.Finish();

                grid1.Dispose();
                a1.Dispose();
                px1.Dispose();
                py1.Dispose();
                pz1.Dispose();
                px = py = pz = null;
                
                ret = true;
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                ret = false;
            }
            
            #endregion

            return ret;
        }

        #region 插值加权Kernel Source
        private static string InterpolateSource = @"#pragma OPENCL EXTENSION cl_khr_fp64 : enable 

        //计算距离矩阵，2维
        __kernel void distCalculate(__global double* A, __global float *px,__global float *py, __global float *pz, int np )
        {
            int irow = get_global_id(0); //row id
            int icol = get_global_id(1); //col id
            
            if( irow >= np || icol >= np ) return;
            
            int id1 = irow * np + icol;
            int id2 = icol * np + irow;
            if(irow == icol)
            {
                A[id1] = 5e-4;
            }

            if( A[id2] > 0 ) A[id1] = A[id2];
            else 
               A[id1] = sqrt( ( px[irow] - px[icol] )*( px[irow] - px[icol] ) + 
                              ( py[irow] - py[icol] )*( py[irow] - py[icol] ) + 
                              ( pz[irow] - pz[icol] )*( pz[irow] - pz[icol] ) + 25e-8 );
        }        

       __kernel void MatrixInverse(__global double* A, __global double* E, __global int *currow, int row )
        {
            
            int i, j;
            long id, id0;
            double bs;

            int threadid = get_global_id(0);

            if( threadid >= row )return ;

            int k = currow[0];
            
            //id = k * row + k;
            
            
            //barrier(CLK_GLOBAL_MEM_FENCE);

            i = threadid;
            if( i != k )
            {                
                bs = A[i * row + k];
                for (j = k; j < row; j++ )
                {   
                    id0 = k * row + j;
                    id = i * row + j;
                    A[id] = A[id] - bs * A[id0];
                }
                for (j = 0; j < row; j++ )
                {
                    id0 = k * row + j;
                    id = i * row + j;
                    E[id] = E[id] - bs * E[id0];
                }
            }
        }

        __kernel void weightCalculate( __global double* A, __global float *pv, __global double *a, int np )
        {
            int i,j,id;
            i = get_global_id(0);
            if( i >= np ) return;            
            a[i] = 0;
            for ( j = 0; j < np; j++ )
            {
                id = i * np + j;
                a[i] += A[id] * pv[j];
            }                           
        }
        
        __kernel void gridInterpolate(  __global float* grid3d, __global double* a,
                                        __global float*px, __global float* py, __global float* pz, 
                                        double minx, double miny, double minz,
                                        double xstep, double ystep, double zstep,
                                        int nx, int ny, int nz, int np  )
        {
            int ix,iy,iz,id,i,j;
            double x,y,z,sum;

            ix = get_global_id(0);
            iy = get_global_id(1);
            iz = get_global_id(2);
            
            //无效的线程
            if( ix>= nx || iy>= ny || iz>= nz )return;

            //当前插值点
            x = minx + xstep * ix;
            y = miny + ystep * iy;
            z = minz + zstep * iz;

            sum = 0;            
            for (j = 0; j < np; j++)
            {
                sum += a[j] * sqrt( (x - px[j]) * (x - px[j]) +
                                    (y - py[j]) * (y - py[j]) +
                                    (z - pz[j]) * (z - pz[j]) + 25E-8 );
                
            }
            id = ix + iy * nx + iz * nx * ny;
            grid3d[id] = sum;
        }
        
        "; //end of InterpolationSource
        #endregion InterpolationSource
      
       
        #region 反距离加权插值Soruce
        //反距离加权插值
        private static string IDWSoruce = @"#pragma OPENCL EXTENSION cl_khr_fp64 : enable
            __kernel void gpuThread(__global float * px,__global float * py,__global float * pz,__global float * pv, int np,
                                         __global float * dists, 
                                          int nx, int ny, int nz,
                                          double minx, double miny, double minz,
                                          double maxx, double maxy, double maxz,
                                         __global float* grid3d )
                            {
                                    int i,iy,ix,id;
                                    double x,y,z,value,dist,distinv;    
                                    const int iz = get_global_id(0); //cur iz position
    
                                    double sum = 0;
    
                                    double err = 1.0E-20;
                                    double xstep = ( maxx - minx ) / (nx - 1);
                                    double ystep = ( maxy - miny ) / (ny - 1);
                                    double zstep = ( maxz - minz ) / (nz - 1); 
                                    z = minz + (iz * zstep);

                                    for(iy=0;iy<ny;iy++)
                                    {  
                                        for(ix=0;ix<nx;ix++)
                                        {                                            
                                            x = minx + (ix * xstep);
                                            y = miny + (iy * ystep);
                                            value = 0;
                                            sum = 0;
                                            for( i = 0;i < np; i++ )
                                            {
                                                dist  = (float)sqrt(  (x - px[i])*(x - px[i]) + 
                                                                      (y - py[i])*(y - py[i]) + 
                                                                      (z - pz[i])*(z - pz[i]) );
                                               
                                                if( dist >= -err && dist <= err )
                                                {                                                    
                                                    sum = 0;
                                                    value = pv[i];
                                                    break;
                                                }
                                                else 
                                                {   
                                                    distinv = 1.0 / dist;
                                                    sum = sum + distinv;
                                                    dists[ iz * np + i ] = distinv;
                                                }
                                            }//for( i = 0;i < np; i++ )
                        
                                            ////////////////////////////////////////////////////
                                            if( sum > 0.0 )
                                            {    
                                                value = 0;
                                                for( i = 0;i < np; i++ )                                                
                                                {   
                                                    value = value + pv[i] * dists[iz * np + i] / sum ;
                                                }
                                            }

                                            id = ix + iy*nx + iz * nx * ny;
                                            grid3d[id] = (float)value;

                                        }//for(ix=0;ix<nx;ix++)
                                    }//for(iy=0;iy<ny;iy++)
                            } ";
        #endregion
        #region 反距离加权插值Soruce
        //反距离加权插值
#pragma warning disable CS0414 // 字段“CLInterpolation.IDWSoruce1”已被赋值，但从未使用过它的值
        private static string IDWSoruce1 = @"#pragma OPENCL EXTENSION cl_khr_fp64 : enable
            __kernel void gpuThread(__global float * px,__global float * py,__global float * pz,__global float * pv, int np,
                                         __global float * dists, 
                                          int nx, int ny, int nz,
                                          double minx, double miny, double minz,
                                          double maxx, double maxy, double maxz,
                                         __global float* grid3d )
                            {
                                    int i,iy,ix,id;
                                    double x,y,z,value,dist,distinv;    
                                    const int iz = get_global_id(0); //cur iz position
    
                                    double sum = 0;
    
                                    double err = 1.0E-20;
                                    double xstep = ( maxx - minx ) / (nx - 1);
                                    double ystep = ( maxy - miny ) / (ny - 1);
                                    double zstep = ( maxz - minz ) / (nz - 1); 
                                    z = minz + (iz * zstep);

                                    for(iy=0;iy<ny;iy++)
                                    {  
                                        for(ix=0;ix<nx;ix++)
                                        {                                            
                                            x = minx + (ix * xstep);
                                            y = miny + (iy * ystep);
                                            value = 0;
                                            sum = 0;
                                            for( i = 0;i < np; i++ )
                                            {
                                                dist  = (float)sqrt(  (x - px[i])*(x - px[i]) + 
                                                                      (y - py[i])*(y - py[i]) + 
                                                                      (z - pz[i])*(z - pz[i]) );
                                               
                                                if( dist >= -err && dist <= err )
                                                {                                                    
                                                    sum = 0;
                                                    value = pv[i];
                                                    break;
                                                }
                                                else 
                                                {   
                                                    distinv = 1.0 / dist;
                                                    sum = sum + distinv;
                                                    dists[ iz * np + i ] = distinv;
                                                }
                                            }//for( i = 0;i < np; i++ )
                        
                                            ////////////////////////////////////////////////////
                                            if( sum > 0.0 )
                                            {    
                                                value = 0;
                                                for( i = 0;i < np; i++ )                                                
                                                {   
                                                    value = value + pv[i] * dists[iz * np + i] / sum ;
                                                }
                                            }

                                            id = ix + iy*nx + iz * nx * ny;
                                            grid3d[id] = (float)value;

                                        }//for(ix=0;ix<nx;ix++)
                                    }//for(iy=0;iy<ny;iy++)
                            } "
#pragma warning restore CS0414 // 字段“CLInterpolation.IDWSoruce1”已被赋值，但从未使用过它的值
;
        #endregion
    }

   
}   

