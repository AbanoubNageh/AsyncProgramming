using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncPoc
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Using Task.Run()

            //var totalAfterTax = CalculateTotalAfterTaxAsync(70);
            //DoSomethingSynchronous();

            //totalAfterTax.Wait();
            //Console.ReadLine();

            #endregion

            #region Using Task.Run() with Task.WhenAll()

            var totalAfterTaxWhenAll = CalculateTotalAfterTaxAsync();
            DoSomethingSynchronous();

            totalAfterTaxWhenAll.Wait();
            Console.ReadLine();

            #endregion

            #region Using async and await

            //DoSyncWork();
            //var someTask = DoSomethingAsync();
            //DoSyncWorkAfterAwait();
            //someTask.Wait(); //this is a blocking call
            //Console.ReadLine();

            #endregion


        }

        #region Using Task.Run() with task continuation options
        static CancellationTokenSource cancellationTokenSource = null;

        private static void DoSomethingSynchronous()
        {
            Console.WriteLine("Doing some synchronous work");
        }

        static async Task<float> CalculateTotalAfterTaxAsync(float value)
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }

            cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.Token.Register(() =>
            {
                Console.WriteLine("Cancellation requested");
            });

            Console.WriteLine("Started CPU Bound asynchronous task on a background thread");
            var result = Task.Run(() => value * 1.2f, cancellationTokenSource.Token);
            var process = result.ContinueWith(t =>
            {
                Console.WriteLine($"Finished Task. Total of ${value} after tax of 20% is ${t.Result} ");
            }, cancellationTokenSource.Token,
                TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.Current

            );

            process = result.ContinueWith(t =>
           {
               Console.WriteLine(t.Exception.InnerException.Message);
           }, TaskContinuationOptions.OnlyOnFaulted);

            return await result;
        }

        #endregion

        #region Using Task.Run() with Task.WhenAll()
        static async Task<float[]> CalculateTotalAfterTaxAsync()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }

            cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.Token.Register(() =>
            {
                Console.WriteLine("Cancellation requested");
            });

            string [] values = "70|80|90".Split('|');
            var loadingTasks = new List<Task<float>>();
            foreach (var value in values)
            {
                Console.WriteLine("Started CPU Bound asynchronous task on a background thread");
                var result = Task.Run(() => Convert.ToSingle(value) * 1.2f, cancellationTokenSource.Token);

                loadingTasks.Add(result);
            }

            var allLoadingTasks = Task.WhenAll(loadingTasks);

            for (int i = 0; i < allLoadingTasks.Result.Length; i++)
            {
                Console.WriteLine(allLoadingTasks.Result[i]);
            }

            return await allLoadingTasks;
        }
        #endregion

        #region Using Async and await
        private const string URL = "https://docs.microsoft.com/en-us/dotnet/csharp/csharp";

        public static object Dispathcer { get; private set; }

        public static void DoSyncWork()
        {
            Console.WriteLine("1. Doing some work synchronously");
        }

        static async Task DoSomethingAsync()
        {
            Console.WriteLine("2. Async task has started");
            await GetStringAsync();
        }

        static async Task GetStringAsync()
        {
            using (var httpClient = new HttpClient())
            {
                Console.WriteLine("3. Awaiting the result of GetStringAsync of Http Client...");
                string result = await httpClient.GetStringAsync(URL);

                //The execution will resume once the above awaitable is done
                Console.WriteLine($"4. The length of http Get is : {result.Length} character");
            }
        }

        static void DoSyncWorkAfterAwait()
        {
            //This is the work we can do while waiting for the awaited Async Task to complete
            Console.WriteLine("7. While waiting for the async task to finish, we can do some unrelated work");
            for (var i = 0; i <= 5; i++)
            {
                for (var j = i; j <= 5; j++)
                {
                    Console.Write("*");
                }
                Console.WriteLine();
            }

        }
        #endregion

    }
}
