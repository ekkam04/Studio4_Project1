using System;

namespace Ekkam
{
    public class Job
    {
        public Action Execute { get; private set; }
        public Action OnComplete { get; private set; }

        public Job(Action execute, Action onComplete = null)
        {
            Execute = execute;
            OnComplete = onComplete;
        }
    }
}