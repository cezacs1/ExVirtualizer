using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExVirtualizer
{
    class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                MessageBox.Show("Please drag & drop to protect assembly.");
                Environment.Exit(0);
            }

            ExVirt.Protect(args[0]);
        }
    }
}
