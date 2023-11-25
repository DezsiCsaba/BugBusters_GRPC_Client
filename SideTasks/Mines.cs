using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideTasks
{
    public class Mines
    {
        public string Digits()
        {
            string first = Console.ReadLine();
            string second = Console.ReadLine();
            string third = Console.ReadLine();
            string[] input = { first, second, third };

            Dictionary<string, char> segmentMap = new Dictionary<string, char>()
            {
                {" _ | ||_|", '0'},
                {"     |  |", '1'},
                {" _  _||_ ", '2'},
                {" _  _| _|", '3'},
                {"   |_|  |", '4'},
                {" _ |_  _|", '5'},
                {" _ |_ |_|", '6'},
                {" _   |  |", '7'},
                {" _ |_||_|", '8'},
                {" _ |_| _|", '9'}
            };

            int digitCount = input[0].Length / 3;
            char[] result = new char[digitCount];

            for (int i = 0; i < digitCount; i++)
            {
                string digit = "";
                for (int j = 0; j < 3; j++)
                {
                    digit += input[j].Substring(i * 3, 3);
                }

                if (segmentMap.ContainsKey(digit))
                {
                    result[i] = segmentMap[digit];
                }

            }
            return new string(result);
        }
    }
}
