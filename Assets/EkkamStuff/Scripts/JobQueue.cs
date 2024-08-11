using System.Collections.Concurrent;

namespace Ekkam
{
    public class JobQueue
    {
        private ConcurrentQueue<Job> jobQueue = new ConcurrentQueue<Job>();

        public void Enqueue(Job job)
        {
            jobQueue.Enqueue(job);
        }

        public bool TryDequeue(out Job job)
        {
            return jobQueue.TryDequeue(out job);
        }
    }
}