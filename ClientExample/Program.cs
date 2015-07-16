using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;

using System.Net;
using System.Net.Http;

using Nulands.Restless;
using Nulands.Restless.Extensions;
using Proactive.Threading;

namespace ClientExample
{
    class Program
    {
        static void Main(string[] args)
        {

            /*int t = 0;
            for (int i = 0; i < 10000; i++)
                t = i;

            int enqueueDequeueOperations = 1000;

            Ringbuffer<int> ringbuffer = new Ringbuffer<int>(enqueueDequeueOperations);

            //ringbuffer.Preallocate<int>();



            List<Task> tasks = new List<Task>();

            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
            Stopwatch watch = Stopwatch.StartNew();
            int tmp = 0;
            for (int i = 0; i < enqueueDequeueOperations; i++)
            {
                tasks.Add(Task.Run(() => queue.Enqueue(i)));
                tasks.Add(Task.Run(() => queue.TryDequeue(out tmp)));
            }

            //int tmp = 0;
            //for (int i = 0; i < enqueueDequeueOperations; i++)
            //    tasks.Add(Task.Run(() => queue.TryDequeue(out tmp)));

            Task.WaitAll(tasks.ToArray());
            watch.Stop();
            Console.WriteLine("Stopwatch: " + watch.ElapsedMilliseconds);
            Console.WriteLine("Per task: " + watch.ElapsedMilliseconds / (double)enqueueDequeueOperations);
            
            //System.Threading.Thread.Sleep(500);

            tasks = new List<Task>();
            Work.StartWorkerPool();
            watch = Stopwatch.StartNew();
            for (int i = 0; i < enqueueDequeueOperations; i++)
            {
                tasks.Add(Work.Run(() => ringbuffer.Enqueue(i)));
                tasks.Add(Work.Run(() => tmp = ringbuffer.Dequeue()));

                //tasks.Add(Task.Run(() => ringbuffer.Enqueue(i)));
                //tasks.Add(Task.Run(() => ringbuffer.Dequeue()));
            }
            //for (int i = 0; i < enqueueDequeueOperations; i++)
            //    tasks.Add(Task.Run(() => ringbuffer.Dequeue()));

            Task.WaitAll(tasks.ToArray());
            watch.Stop();
            Console.WriteLine("Stopwatch: " + watch.ElapsedMilliseconds);
            Console.WriteLine("Per task: " + watch.ElapsedMilliseconds / (double)enqueueDequeueOperations);



            */
            int numberOfRequests = 5000;

            Console.ReadLine();

            Rest.Get("http://127.0.0.1:9000/").Fetch(
                    resp => Console.WriteLine(resp.HttpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()),
                    resp => Console.WriteLine("Error: " + resp.Exception)).GetAwaiter().GetResult();

            List<Task<RestResponse<IVoid>>> responseTasks = new List<Task<RestResponse<IVoid>>>();

            Stopwatch stopWatch = Stopwatch.StartNew();

            for (int i = 0; i < numberOfRequests; i++)
            {
                var task = Rest.Get("http://127.0.0.1:9000/index/").Fetch(null,
                    resp => Console.WriteLine("Error: " + resp.Exception));
                responseTasks.Add(task);

                if (i % 4 == 0)
                {
                    Task.WaitAll(responseTasks.ToArray());
                    responseTasks.Clear();
                }
            }
            Task.WaitAll(responseTasks.ToArray());
            stopWatch.Stop();

            Console.WriteLine("Received " + numberOfRequests + " requests in " + stopWatch.ElapsedMilliseconds + " milli seconds");
            Console.WriteLine("Requests per second is " + (stopWatch.ElapsedMilliseconds / (double)numberOfRequests) * 1000.0);
            Console.ReadLine();
        }
    }
}
