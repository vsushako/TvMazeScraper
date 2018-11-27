using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TvMazeScraper.Source.Model;
using System.Net;
using System.Threading;

namespace TvMazeScraper.Source
{
    public class TvMazeScraperApi : ITvMazeScraperApi
    {
        private IRequestSender RequestSender { get; }

        private ICallManager CallManager { get; }

        public TvMazeScraperApi(IRequestSender requestSender, ICallManager callManager)
        {
            RequestSender = requestSender;
            CallManager = callManager;
        }

        public async Task<IEnumerable<CastModel>> GetCast(int showId)
        {
            return await GetResult<IEnumerable<CastModel>>($"/shows/{showId}/cast");
        }

        public async Task<ShowModel> GetShow(int id)
        {
            return await GetResult<ShowModel>($"/shows/{id}");
        }

        public async Task<IEnumerable<ShowModel>> GetShows(int page = 0)
        {
            return await GetResult<IEnumerable<ShowModel>>($"/shows?page={page}");
        }

        public async Task<IDictionary<string, int>> GetUpdates()
        {
            return await GetResult<IDictionary<string, int>>("/updates/shows");
        }

        private async Task<TResult> GetResult<TResult>(string address)
        {
            try
            {
                return await CallManager.Call(async () =>
                {
                    var resultStr = await RequestSender.Get(address);
                    return JsonConvert.DeserializeObject<TResult>(resultStr);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public interface ICallManager
    {
        int ElementsCount { get; set; }

        Task<TResult> Call<TResult>(Func<Task<TResult>> action);
    }

    public class CallManager : ICallManager
    {
        public int ElementsCount { get; set; }

        private static int _theLock;

        private static System.Timers.Timer Timer { get; set; }

        private static BackgroundWorker Worker { get; set; }

        private static EventWaitHandle EventWaitHandleFlag { get; set; } = new AutoResetEvent(false);

        private static readonly ConcurrentQueue<EventWaitHandle> Handles = new ConcurrentQueue<EventWaitHandle>();

        public CallManager(int waitPeriod)
        {
            if (Interlocked.CompareExchange(ref _theLock, 1, 0) == 1) return;

            Worker = new BackgroundWorker();
            Timer = new System.Timers.Timer(waitPeriod);

            Worker.DoWork += HandleCalls;
            Worker.RunWorkerCompleted += (sender, args) =>
            {
                if (!Timer.Enabled && Handles.Count != 0)
                    Timer.Start();
            };
            Timer.Elapsed += (sender, args) => {
                lock (Worker)
                {
                    if (!Worker.IsBusy && Handles.Count != 0)
                        Worker.RunWorkerAsync();
                    else
                        Timer.Stop();
                }
            };
            Timer.Start();
        }

        private void HandleCalls(object sender, DoWorkEventArgs e)
        {
            for (var i = 0; i < ElementsCount - 1; i++)
            {
                // Gets next element and enqueue it
                Handles.TryDequeue(out var ev);

                if (ev != null)
                    ev.Set();
                else
                {
                    EventWaitHandleFlag.WaitOne();
                }
            }
        }

        public async Task<TResult> Call<TResult>(Func<Task<TResult>> action)
        {
            var ev = new AutoResetEvent(false);
            Handles.Enqueue(ev);

            lock (Worker)
                if (!Worker.IsBusy && !Timer.Enabled)
                    Worker.RunWorkerAsync();

            EventWaitHandleFlag.Set();
            ev.WaitOne();

            

            return await action();
        }
    }
}
