// File: HardwareInfo.cs
using System.Management; // Đừng quên dòng này

namespace BimcommandCAD
{
    public static class HardwareInfo
    {
        public static string GetCpuId()
        {
            try
            {
                string cpuInfo = string.Empty;
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win64_Processor");
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    cpuInfo = obj["ProcessorId"].ToString();
                    break;
                }
                return cpuInfo;
            }
            catch
            {
                return "ErrorGettingCPU_ID";
            }
        }
    }
}