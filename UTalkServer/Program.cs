using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace UTalkServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("请输入服务器IP地址...");
            TwoPersonTalk.ServerIP = Console.ReadLine();
            //TwoPersonTalk.ServerIP = Dns.GetHostAddresses("hellowhugh.oicp.io")[0].ToString().Split(':')[0];
            TwoPersonTalk.StartTalking();
            Console.ReadKey();
        }
    }
}
