using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide.Tools
{
    public static class PercentageClass
    {
        public static Dictionary<string, double> percentages = new Dictionary<string, double>();

        public static void UpdatePercentage(string name, double percentage)
        {
            if (!percentages.ContainsKey(name))
                percentages.Add(name, percentage);

            else
                percentages[name] = percentage;
        }
    }
}
