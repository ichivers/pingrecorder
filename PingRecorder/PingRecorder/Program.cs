using System;
using System.Collections.Generic;
using System.Text;
using System.Net.NetworkInformation;
using System.IO;
using System.Threading;

namespace PingRecorder
{
    class Program
    {
        private static TextWriter writer;
        
        static void Main(string[] args)
        {            
            Console.TreatControlCAsInput = false;
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            Console.WriteLine("Press Ctrl + C to quit");

            Arguments commandLine = new Arguments(args);
            
            int delay = commandLine["delay"] == null ? 60000 : Convert.ToInt32(commandLine["delay"]);
            if (args.Length == 0)
            {
                Console.WriteLine("");
                Console.WriteLine("usage: pingrecorder [-timeout milliseconds] [-delay milliseconds]");
                Console.WriteLine("                    [-output output_filename] target_name");
                Console.WriteLine("");
                Console.WriteLine("Options:");
                Console.WriteLine("    -output filename         File to write output");
                Console.WriteLine("    -timeout milliseconds    Timeout in millseconds to wait for each reply");
                Console.WriteLine("    -delay milliseconds      Delay in milliseconds between each ping");
            }
            else
            {
                FileInfo output = new FileInfo(commandLine["output"]);
                writer = output.CreateText();
                while (true)
                {
                    Thread pingThread = new Thread(new ParameterizedThreadStart(ping));
                    pingThread.Start(args);
                    Thread.Sleep(delay);
                }
            }
        }


        static void ping(object param)
        {            
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            string[] args = (string[])param;
            Arguments commandLine = new Arguments((string[])param);
            options.DontFragment = true;

            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = commandLine["timeout"] == null ? 5000 : Convert.ToInt32(commandLine["timeout"]);
            PingReply reply = pingSender.Send(args[args.Length - 1], timeout, buffer, options);
            if (reply.Status == IPStatus.Success)
            {
                writer.WriteLine(DateTime.Now.ToString("hh:mm") + "," + reply.RoundtripTime.ToString());
                Console.WriteLine(DateTime.Now.ToString() + ": Reply from " + reply.Address + " bytes=32 time=" + reply.RoundtripTime + "ms");
            }
            else
            {
                writer.WriteLine(DateTime.Now.ToString("hh:mm") + "," + timeout.ToString());
                Console.WriteLine(DateTime.Now.ToString() + ": Request timed out");
            }
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            writer.Close();
        }
    }
}
