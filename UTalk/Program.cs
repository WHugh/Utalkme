using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace UTalk
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("请输入服务器IP地址...");
            TalkToPerson.ServerIP = Console.ReadLine();
            //TalkToPerson.ServerIP = Dns.GetHostAddresses("hellowhugh.oicp.io:30462")[0].ToString().Split(':')[0];
            TalkToPerson.StartClient();
        }
    }
}
