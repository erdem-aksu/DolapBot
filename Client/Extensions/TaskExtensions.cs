using System;
using System.Threading;
using System.Threading.Tasks;

namespace DolapBot.Client.Extensions
{
    public class TaskExtensions
    {
        public static async Task WaitWhile(Func<Task<bool>> condition, int frequency = 25, int timeout = -1)
        {
            var tokenSource2 = new CancellationTokenSource();
            var ct = tokenSource2.Token;
            
            var waitTask = Task.Run(async () =>
            {
                ct.ThrowIfCancellationRequested();

                while (await condition())
                {
                    if (ct.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                    
                    await Task.Delay(frequency, ct);
                }
            }, tokenSource2.Token);

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout, tokenSource2.Token)))
            {
                tokenSource2.Cancel();
                tokenSource2.Dispose();
                throw new TimeoutException();
            }
        }
    }
}