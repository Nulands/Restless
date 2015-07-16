using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Nulands.Restless;

namespace ClientServerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            RestServer<HttpListener> server = RestServerUtil.CreateFromHttpListener("http://127.0.0.1:9000/");
            
            // Add a session to a listener context if the request contains a cookie
            server.ServerContext.NeedsSession = rlContext => rlContext.Request.Cookies != null && rlContext.Request.Cookies.Count > 0;
            // The session key is the first cookie
            server.ServerContext.GetSessionKey = rlContext => rlContext.Request.Cookies[1];
            // Set the session lifetime to 15 minutes
            server.ServerContext.GetValidUntil = creationDate => creationDate.AddMinutes(15.0);


            server.Get["", writeAsLine:true] = _ => "Hello world";

            //string data = "Hello World from server##########################################################################################################################################################################################################################################################################################################################################################################################################################################################################################################################################";
            string data = "Hello World";
            server.Get["/", writeAsLine: true] = _ => {
                Console.WriteLine("Get[/]");
                return data;
            };

            server.Get["/index/", writeAsLine: true] = _ =>{
                //Console.WriteLine("Get[/index/]");
                return "Hello World from server##########################################################################################################################################################################################################################################################################################################################################################################################################################################################################################################################################";
            };

            Console.WriteLine("Starting server");
            server.Start().GetAwaiter().GetResult();

            Console.ReadLine();
        }
    }
}
