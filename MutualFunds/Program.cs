using System;
using System.Linq;
using System.Collections;

namespace MutualFunds
{
    class Program
    {
        static void Main(string[] args)
        {
            var nothing = args.Length == 0;
            var display = args.Contains("--display");
            var save = args.Contains("--save");
            var mutualFunds = new MutualFunds();
            Hashtable info = null;

            if (nothing || display || save) {
                info = mutualFunds.getInfo();
            }

            if (nothing || display)
            {
                mutualFunds.displayInfo(info);
                Console.ReadLine();
            }

            if (save)
            {
                if (!mutualFunds.dateExist(info))
                {
                    mutualFunds.saveInfo(info);
                }
            }            
                   
        }
    }
}
