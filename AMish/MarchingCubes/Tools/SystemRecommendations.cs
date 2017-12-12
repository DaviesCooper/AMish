using System;
using System.Diagnostics;
using System.Management;


namespace ServerSide.Tools
{
    public static class SystemRecommendations
    {

        private static int minimumHz = 1100;
        private static int maximumHz = 4700;
        private static int minRes = 500000;
        private static int maxRes = 5000000;
        private static double testMEM = 2294016800;

        private static PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private static PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");

        public static long getRAM()
        {
            ManagementScope oMs = new ManagementScope();
            ObjectQuery oQuery = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
            ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);
            ManagementObjectCollection oCollection = oSearcher.Get();

            long MemSize = 0;
            long mCap = 0;

            //each stick has its own obj
            foreach (ManagementObject obj in oCollection)
            {
                mCap = Convert.ToInt64(obj["Capacity"]);
                MemSize += mCap;
            }
            return MemSize;
        }

        public static int getCores()
        {

            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            return coreCount;
        }

        public static uint getHz()
        {
            uint maxClockSpeed = uint.MinValue;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select MaxClockSpeed from Win32_Processor");
            foreach (var item in searcher.Get())
            {
                uint clockSpeed;
                if((clockSpeed = (uint)item["MaxClockSpeed"]) > maxClockSpeed)
                {
                    maxClockSpeed = clockSpeed;
                }
            }
            return maxClockSpeed;
        }

        public static int getRecommendedResolution()
        {
            float perc = ((Convert.ToSingle(getHz()) - (float)minimumHz)/(float)maximumHz);
            return (int)((maxRes - minRes) * perc);
        }

        public static int getRecommendedThreads(long numberOfEntries)
        {
            return (int)Math.Floor(getRAM() / testMEM);
        }
    }
}
