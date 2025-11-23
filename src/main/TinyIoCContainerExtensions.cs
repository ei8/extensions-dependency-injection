using Nancy.TinyIoc;
using neurUL.Common.Http;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Extensions.DependencyInjection
{
    public static class TinyIoCContainerExtensions
    {
        public static void AddRequestProvider(this TinyIoCContainer container)
        {
            var rp = new RequestProvider();
            rp.SetHttpClientHandler(new HttpClientHandler());
            container.Register<IRequestProvider>(rp);
        }

        /// <summary>
        /// Attempts and retries a dependency registration process.
        /// </summary>
        /// <param name="container">Container to be used for dependency resolution and registration.</param>
        /// <param name="retryCount">Number of times to retry.</param>
        /// <param name="retryDelay">Number of seconds to wait before next retry.</param>
        /// <param name="parentProcessDescription">Description of parent process</param>
        /// <param name="checkProcess">Function to be used to check process readiness.</param>
        /// <param name="checkProcessDescription">Description of check process.</param>
        /// <param name="coreProcess">Function to be invoked if check is successful.</param>
        /// <param name="coreProcessDescription">Description of core process.</param>
        public static void RetryWaitProcess(
            this TinyIoCContainer container, 
            int retryCount, 
            int retryDelay, 
            string parentProcessDescription, 
            Func<TinyIoCContainer, Task<bool>> checkProcess,
            string checkProcessDescription,
            Action<TinyIoCContainer> coreProcess,
            string coreProcessDescription
        )
        {
            for (var tries = 0; tries < retryCount; tries++)
            {
                var checkProcessResult = Task.Run(async() => await checkProcess(container)).Result;

                var log = $"'{checkProcessDescription}' check {(checkProcessResult ? "succeeded" : "failed")}.";

                if (checkProcessResult)
                    Trace.TraceInformation(log);
                else
                    Trace.TraceWarning(log);

                if (checkProcessResult)
                {
                    try
                    {
                        coreProcess.Invoke(container);
                        Trace.TraceInformation($"'{parentProcessDescription} : {coreProcessDescription}' succeeded. ");
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"'{parentProcessDescription} : {coreProcessDescription}' encountered an error: {ex.ToString()}");
                    }
                    break;
                }
                else
                {
                    var currentTry = tries + 1;
                    log = (
                            currentTry < retryCount ?
                                $"Waiting {retryDelay} sec(s) before next attempt... " :
                                $"Stopping attempts - {parentProcessDescription} failed. "
                        ) + $"(Tried {currentTry}/{retryCount})";

                    if (currentTry < retryCount)
                    {
                        Trace.TraceInformation(log);
                        Thread.Sleep(Convert.ToInt32(TimeSpan.FromSeconds(retryDelay).TotalMilliseconds));
                    }
                    else
                        Trace.TraceWarning(log);
                }
            }
        }
    }
}
