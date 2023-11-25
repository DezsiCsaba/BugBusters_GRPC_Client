using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideTasks
{
    public class Money
    {
        public string BiggerIncreasingNum()
        {
            double N = double.Parse(Console.ReadLine());
            double result = -2;
            char[] NArray = N.ToString().ToCharArray();
            while (result == -2)
            {
                N++;
                double counter = 0;
                NArray = N.ToString().ToCharArray();
                if (NArray.Length > 1)
                {
                    for (int i = 0; i < NArray.Length - 1; i++)
                    {
                        if (NArray[i + 1] >= NArray[i])
                        {
                            counter++;
                        }
                    }
                    if (counter == NArray.Length - 1) { result = double.Parse(NArray); }
                }
                else
                {
                    result = double.Parse(NArray);
                }
            }
            return result.ToString();
        }
    }
}
