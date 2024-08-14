using System.Threading;

namespace Ekkam
{
    public class JobManager
    {
        private JobQueue jobQueue = new JobQueue();
        private Thread jobThread;
        private bool running = true;

        public static JobManager instance { get; } = new JobManager();

        private JobManager()
        {
            jobThread = new Thread(ProcessJobs);
            jobThread.Start();
        }

        public void EnqueueJob(Job job)
        {
            jobQueue.Enqueue(job);
        }

        private void ProcessJobs()
        {
            while (running)
            {
                if (jobQueue.TryDequeue(out Job job))
                {
                    job.Execute();
                    job.OnComplete?.Invoke();
                }

                Thread.Sleep(1);
            }
        }

        public void Stop()
        {
            running = false;
            jobThread.Join();
        }
    }
}