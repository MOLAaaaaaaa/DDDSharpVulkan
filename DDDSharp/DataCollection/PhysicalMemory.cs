using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace DataCollection
{
    static class MMPhysicalMemory
    {
        /// <summary>
        /// 获得已使用的物理内存的大小，单位 (Byte)，如果获取失败，返回 -1.
        /// </summary>
        /// <returns></returns>
        static public int DeviceNum = 0;
        static public  int GetDeveiceNum()
        {
            DeviceNum = 0;
            foreach (ManagementObject mo1 in new ManagementClass("Win32_PhysicalMemory").GetInstances())
            {
                DeviceNum++;
            }
            return DeviceNum;
        }

        public static ulong GetTotalMemorySize()
        {
            ulong capacity = 0;
            try
            {
                foreach (ManagementObject mo1 in new ManagementClass("Win32_PhysicalMemory").GetInstances())
                { 
                    capacity += Convert.ToUInt64(mo1.Properties["Capacity"].Value.ToString());
                    DeviceNum++;     
                }
            }
            catch (Exception ex)
            {
                capacity = 0;
            }
            return capacity;
        }

        /// <summary>
        /// 获得已使用的物理内存的大小，单位 (Byte)，如果获取失败，返回 -1.
        /// </summary>
        /// <returns></returns>
        public static ulong GetAvailableMemorySize()
        {
            ulong capacity = 0;
            try
            {
                foreach (ManagementObject mo1 in new ManagementClass("Win32_PerfFormattedData_PerfOS_Memory").GetInstances())
                    capacity += Convert.ToUInt64(mo1.Properties["AvailableBytes"].Value.ToString());
            }
            catch (Exception ex)
            {
                capacity = 0;
            }
            return capacity;
        }
        public static double GetTotalMemoryMB()
        {
            return GetTotalMemorySize() / 1024.0 / 1024.0;
        }
        public static double GetAvailableMemoryMB()
        {
            return GetAvailableMemorySize() / 1024.0 / 1024.0;
        }
    }

    static class PhysicalMemory
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_INFO
        {
            public uint dwLength;
            //dwMemoryLoad内存负载率(0-100)
            public uint dwMemoryLoad;//88%
            public uint dwTotalPhys;//
            public uint dwAvailPhys;//
            public uint dwTotalPageFile;//
            public uint dwAvailPageFile;
            public uint dwTotalVirtual;
            public uint dwAvailVirtual;
        }
        [DllImport("kernel32")]
        public static extern void GlobalMemoryStatus(ref MEMORY_INFO meminfo);
        public static uint memoryLoad = 0;
        public static ulong GetTotalMemorySize()
        {
            MEMORY_INFO MemInfo = new MEMORY_INFO();
            GlobalMemoryStatus(ref MemInfo);

            return Convert.ToUInt64(MemInfo.dwTotalPhys.ToString());
            /*
            memoryLoad = MemInfo.dwMemoryLoad;
            return (ulong)MemInfo.dwTotalPhys +
                   (ulong)MemInfo.dwTotalVirtual +
                    (ulong)MemInfo.dwTotalPageFile;
            */
        }
        public static double GetTotalMemoryMB()
        {
            return GetTotalMemorySize() / 1024 / 1024;
        }
        public static ulong GetAvailableMemorySize()
        {
            MEMORY_INFO MemInfo = new MEMORY_INFO();
            GlobalMemoryStatus(ref MemInfo);
            return Convert.ToUInt64(MemInfo.dwAvailPhys.ToString());            
            /*
            ulong totalsize = GetTotalMemorySize();
            double s = (100.0 - memoryLoad) / 100.0;
            return (ulong)(totalsize * s);
            */

        }
        public static double GetAvailableMemoryMB()
        {
            return (GetAvailableMemorySize() / 1024 / 1024);
        }
    }

}
