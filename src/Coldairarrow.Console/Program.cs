﻿using Coldairarrow.Business.Base_SysManage;
using Coldairarrow.Entity.Base_SysManage;
using Coldairarrow.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Dynamic;
using System.Collections;
using static Coldairarrow.Entity.Base_SysManage.EnumType;
using Coldairarrow.Util.RPC;
using System.Diagnostics;
using Coldairarrow.Util.Wcf;
using System.ServiceModel;
using Coldairarrow.Util.Sockets;

namespace Coldairarrow.Console1
{
    [ServiceContract]
    public interface IHello
    {
        [OperationContract]
        string SayHello(string msg);
    }
    public class Hello : IHello
    {
        public string SayHello(string msg)
        {
            return msg;
        }
    }

    class Program
    {
        static void RpcTest()
        {
            int port = 9999;
            int count = 1;
            int errorCount = 0;
            RPCServer rPCServer = new RPCServer(port);
            rPCServer.HandleException = ex =>
            {
                Console.WriteLine(ExceptionHelper.GetExceptionAllMsg(ex));
            };
            rPCServer.RegisterService<IHello, Hello>();
            rPCServer.Start();
            IHello client = null;
            client = RPCClientFactory.GetClient<IHello>("127.0.0.1", port);
            client.SayHello("aaa");
            Stopwatch watch = new Stopwatch();
            List<Task> tasks = new List<Task>();
            watch.Start();
            LoopHelper.Loop(1, () =>
            {
                tasks.Add(Task.Run(() =>
                {
                    LoopHelper.Loop(count, () =>
                    {
                        string msg = string.Empty;
                        try
                        {
                            msg = client.SayHello("Hello");
                            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")}:{msg}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ExceptionHelper.GetExceptionAllMsg(ex));
                        }
                    });
                }));
            });
            Task.WaitAll(tasks.ToArray());
            watch.Stop();
            Console.WriteLine($"每次耗时:{(double)watch.ElapsedMilliseconds / count}ms");
            Console.WriteLine($"错误次数：{errorCount}");
        }
        static void WcfTest()
        {
            int count = 10000;

            WcfHost<IHello, Hello> wcfHost = new WcfHost<IHello, Hello>();
            wcfHost.StartHost();
            IHello client = WcfClient.GetService<IHello>("http://127.0.0.1:14725");
            client.SayHello("Hello");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            LoopHelper.Loop(10, () =>
            {
                Task.Run(() =>
                {
                    LoopHelper.Loop(count, index =>
                    {
                        var msg= client.SayHello("Hello"+index);
                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss:fff")}:{msg}");
                    });
                });
            });
            watch.Stop();
            Console.WriteLine($"每次耗时:{(double)watch.ElapsedMilliseconds / count}ms");
        }

        static void SocketTest()
        {
            int count = 1000;
            int port = 11111;
            Stopwatch watch = new Stopwatch();
            TcpSocketServer server = new TcpSocketServer(port);
            server.HandleRecMsg = (a, b, c) =>
            {
                b.Send(c);
            };
            server.StartServer();
            watch.Start();
            LoopHelper.Loop(count, () =>
            {
                AutoResetEvent waitEvent = new AutoResetEvent(false);
                TcpSocketClient tcpSocketClient = new TcpSocketClient(port);
                tcpSocketClient.HandleRecMsg = (a, b) =>
                {
                    waitEvent.Set();
                };
                tcpSocketClient.StartClient();
                tcpSocketClient.Send(new byte[] { 0X01 });
                waitEvent.WaitOne();
            });
            watch.Stop();
            Console.WriteLine($"每次耗时:{(double)watch.ElapsedMilliseconds / count}ms");
        }
        static void Main(string[] args)
        {
            SocketTest();
            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}
