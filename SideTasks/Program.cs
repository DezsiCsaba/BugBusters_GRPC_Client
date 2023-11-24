namespace SideTasks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            double N = double.Parse(Console.ReadLine());
            double result = -2;
            char[] NArray = N.ToString().ToCharArray();
            while (result==-2)
            {
                N++;               
                double counter = 0;
                NArray = N.ToString().ToCharArray();
                if (NArray.Length>1)
                {
                    for (int i = 0; i < NArray.Length - 1; i++)
                    {
                        if (NArray[i + 1] >= NArray[i])
                        {
                            counter++;
                        }
                    }
                    if (counter == NArray.Length-1) { result = double.Parse(NArray); }                   
                }
                else
                {
                    result = double.Parse(NArray);
                }
            }
            Console.WriteLine(result);
        }
    }
}