using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace VetClinicServer
{
    class Program
    {
        private static Server server;

        private const string IP_STRING = "0.0.0.0";
        private const int PORT = 1379;

        static void Main(string[] args)
        {
            server = new Server();
            server.Start(IPAddress.Parse(IP_STRING), PORT, 10);

        }

        ~Program()
        {
            if (server != null)
                server.Stop();
        }
    }
}
