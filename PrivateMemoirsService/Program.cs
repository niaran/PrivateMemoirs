using System.ServiceProcess;

namespace PrivateMemoirs
{
    static class Program
    {
        static void Main()
        {
            ServiceBase.Run(new PrivateMemoirsService());
        }
    }
}