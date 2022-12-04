using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCLNet
{
    public static class IntPtrExtend
    {
        /// <summary>
        /// 取指针
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static unsafe IntPtr ToIntPtr(this int[] obj)
        {
            IntPtr PtrA = IntPtr.Zero;
            fixed (int* Ap = obj) return new IntPtr(Ap);
        }
        public static unsafe IntPtr ToFloatPtr(this float[] obj)
        {
            IntPtr PtrA = IntPtr.Zero;
            fixed (float* Ap = obj) return new IntPtr(Ap);
        }
        public static unsafe IntPtr ToDoublePtr(this double[] obj)
        {
            IntPtr PtrA = IntPtr.Zero;
            fixed (double* Ap = obj) return new IntPtr(Ap);
        }
        public static unsafe IntPtr ToBoolPtr(this bool[] obj)
        {
            IntPtr PtrA = IntPtr.Zero;
            fixed (bool* Ap = obj) return new IntPtr(Ap);
        }
    }//public static class Extend
    static public class OpenCLObj
    {
        static public ulong GetGlobalMemorySize(Device device)
        {            
            return device.GlobalMemSize;
        }
        static public double GetGlobalMemoryMB(Device device)
        {
            return device.GlobalMemSize / 1024 /1024;
        }
        static public ulong GetLocalMemorySize(Device device)
        {
            return device.LocalMemSize;
        }
        static public double GetLocalMemoryMB(Device device)
        {
            return device.LocalMemSize / 1024 / 1024;
        }
        static public ulong GetMaxAllocMemorySize(Device device)
        {
            return device.MaxMemAllocSize;
        }
        static public double GetMaxAllocMemoryMB(Device device)
        {
            return device.MaxMemAllocSize / 1024 / 1024;
        }
       
        static public List<Device> GetDevices(DeviceType type = DeviceType.ALL)
        {
            //获取平台数量
            OpenCL.GetPlatformIDs(32, new IntPtr[32], out uint num_platforms);
            List<Device> devices = new List<Device>();
            Device[]dvs = null;
            //枚举所有平台下面的设备(CPU和GPU)
            for (int i = 0; i < num_platforms; i++)
            {
                dvs = OpenCL.GetPlatform(i).QueryDevices(type);
                //这里后面有个参数,是Enum,这里选择GPU,表示只枚举GPU,
                //在没有GPU的电脑上可以选CPU,也可以传ALL,会把所有设备枚举出来供选择
                for (int j = 0; j < dvs.Length; j++) devices.Add(dvs[j]);
            }
            return devices;
        }

        static public long GetMaxWorkGroupSizes(Device device)
        {
            return device.MaxWorkGroupSize;
        }
        static public int[] GetMaxWorkItemSizes(Device device)
        {
            IntPtr[] sizes = device.MaxWorkItemSizes;
            int[] workitemsizes = new int[3];
            for (int i = 0; i < 3; i++)
            {
                if (sizes.Length > 0) workitemsizes[0] = (int)sizes[0];
                if (sizes.Length > 1) workitemsizes[1] = (int)sizes[1];
                if (sizes.Length > 2) workitemsizes[2] = (int)sizes[2];
            }
            return workitemsizes;
        }
    }
    public class CLUnit
    {
        //GPU devices
        public Context oclContext = null;
        public CommandQueue oclCQ = null;
        public Dictionary<string, Kernel> Kernels = new Dictionary<string, Kernel>();
        public Device oclDevice = null;
        public string errMsg = "";

        public string Sources = "";     //代码
        public List<string> functions = new List<string>(); //函数

        public Semaphore Sema = null;
        public Semaphore externSema = null;
        public bool Initialized = false;

        public long taskNumbers = 0;//分配任务数
        public int workItemSize = 0;
        public int groupNumber = 0;    //分组数
        public long totalWorkSize = 0;

        public CLUnit(Device _device)
        {
            oclDevice = _device;
            Sema = new Semaphore(0, 1);
            externSema = new Semaphore(0, 1);
        }
        static public string formatTime(double second)
        {
            int hours = (int)(second / 3600);
            double sec1 = second - hours * 3600;
            int minutes = (int)(sec1 / 60);
            double sec2 = Math.Round(sec1 - 60 * minutes, 0);
            string tmstr = hours + ":" + minutes + ":" + sec2;
            return tmstr;
        }
        public virtual void ReleaseSiginal()
        {
            Sema.Release();
        }
        public virtual void WaitSiginal()
        {
            Sema.WaitOne();
        }
        public virtual int MaxWorkGroupSize
        {
            get
            {
                if (oclDevice == null) return 0;
                else return (int)oclDevice.MaxWorkGroupSize;
            }
        }
        //设置需要工作数，自动设置大小group 和 itemsize大小
        public virtual int SetWorkItemSize(int num)
        {
            taskNumbers = num;
            if (num <= MaxWorkGroupSize)
            {
                groupNumber = 1;
                workItemSize = (int)num;
            }
            else
            {
                workItemSize = MaxWorkGroupSize;
                groupNumber = (int)num / workItemSize;
                if (num % workItemSize != 0) groupNumber++;
            }

            totalWorkSize = groupNumber * workItemSize;//总共计算行数

            return groupNumber;
        }
        public virtual ulong MaxAllocMemorySize
        {
            get
            {
                if (oclDevice == null) return 0;
                else return oclDevice.MaxMemAllocSize;
            }
        }
        //根据当前内存大小，计算最大允许的任务数
        //tasksize任务单元字节
        public virtual ulong GetMaxTaskNumberOnMemory(long tasksize)
        {
            if (oclDevice == null) return 0;
            ulong size = oclDevice.MaxMemAllocSize;
            return (size / (ulong)tasksize);
        }
        public void AddFunction(string function)
        {
            functions.Add(function);
        }
        public bool Compile(string codes)
        {
            if (oclCQ == null || oclContext == null)
            {
                errMsg = "device not initialized.";
                return false;
            }
            if (functions.Count < 1)
            {
                errMsg = "no function added.";
                return false;
            }
            Sources = codes;
            var oclProgram = oclContext.CreateProgramWithSource(Sources);
            try
            {
                oclProgram.Build();
            }
            catch (OpenCLBuildException EEE)
            {
                errMsg = EEE.BuildLogs[0];
                return false;
            }

            Kernels.Clear();
            foreach (var item in functions)
            {
                Kernels.Add(item, oclProgram.CreateKernel(item));
            }

            oclProgram.Dispose();

            errMsg = "sources compiled successfully.";

            Initialized = true;

            return true;
        }
        public bool InitDevice()
        {
            if (oclDevice == null)
            {
                errMsg = "no available device.";
                return false;
            }

            //根据配置建立上下文
            oclContext = oclDevice.Platform.CreateContext(
                new[] { (IntPtr)ContextProperties.PLATFORM, oclDevice.Platform.PlatformID, IntPtr.Zero, IntPtr.Zero },
                new[] { oclDevice },
                (errInfo, privateInfo, cb, userData) => { },
                IntPtr.Zero
            );

            //创建命令队列
            oclCQ = oclContext.CreateCommandQueue(oclDevice, CommandQueueProperties.PROFILING_ENABLE);

            Initialized = true;

            return true;
        } //end of bool InitCL(Device oclDevice)
        public virtual void ReleaseMemory()
        {
        }
        public virtual void ReleaseCL()
        {
            if (oclDevice != null) oclDevice.Dispose();
            if (oclCQ != null) oclCQ.Dispose();
            if (oclContext != null) oclContext.Dispose();
            Kernels.Clear();
            functions.Clear();
        }
    }//class CLUnit
   
}
