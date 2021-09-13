﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Echo.Client
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;

    public class EchoClientHandler : ChannelHandlerAdapter
    {
        public static Stopwatch watch = new Stopwatch();
        //public override void ChannelActive(IChannelHandlerContext context) => context.WriteAndFlushAsync(this.initialMessage);
        int count = 0;
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            //Console.WriteLine($"{GetTime()}:收到服务器数据");
            watch.Stop();
            Console.WriteLine($"请求耗时:{(double)watch.ElapsedTicks / 10000}ms");
            string GetTime()
            {
                return $"{(double)DateTime.Now.Ticks / 10000}ms";
            }
        }
        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine("Exception: " + exception);
            context.CloseAsync();
        }
    }
}