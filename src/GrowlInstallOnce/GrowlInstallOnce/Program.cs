using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrowlInstallOnce
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length >= 2)
                GrowlInstallHelper.GrowlInstallHelper.CheckAndRun(args[0], (args[1] == "/Q"));
            else if (args.Length == 1)
                GrowlInstallHelper.GrowlInstallHelper.CheckAndRun(args[0], false);
            else
                GrowlInstallHelper.GrowlInstallHelper.CheckAndRun();
        }
    }
}
