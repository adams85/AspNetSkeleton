﻿using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

// Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx

namespace AspNetSkeleton.UI.Infrastructure.BackgroundWork
{
    /// <summary>
    /// An async-compatible countdown event.
    /// </summary>
    [DebuggerDisplay("CurrentCount = {" + nameof(_count) + "}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class CountdownEvent
    {
        /// <summary>
        /// The TCS used to signal this event.
        /// </summary>
        private readonly TaskCompletionSource<object> _tcs;

        /// <summary>
        /// The remaining count on this event.
        /// </summary>
        private int _count;

        /// <summary>
        /// Creates an async-compatible countdown event.
        /// </summary>
        /// <param name="count">The number of signals this event will need before it becomes set. Must be greater than zero.</param>
        public CountdownEvent(int count)
        {
            _tcs = new TaskCompletionSource<object>();
            _count = count;
        }

        /// <summary>
        /// Asynchronously waits for this event to be set.
        /// </summary>
        public Task WaitAsync()
        {
            return _tcs.Task;
        }

        /// <summary>
        /// Attempts to modify the current count by the specified amount. This method returns <c>false</c> if the new current count value would be invalid, or if the count has already reached zero.
        /// </summary>
        /// <param name="signalCount">The amount to change the current count. This must be +1 or -1.</param>
        private void ModifyCount(int signalCount)
        {
            if (Interlocked.Add(ref _count, signalCount) == 0)
                _tcs.TrySetResult(null);
        }

        /// <summary>
        /// Attempts to add one to the current count. This method throws <see cref="InvalidOperationException"/> if the count is already at zero or if the new count would be greater than <see cref="Int32.MaxValue"/>.
        /// </summary>
        public void Increase()
        {
            ModifyCount(1);
        }

        /// <summary>
        /// Attempts to subtract one from the current count. This method throws <see cref="InvalidOperationException"/> if the count is already at zero or if the new count would be less than zero.
        /// </summary>
        public void Decrease()
        {
            ModifyCount(-1);
        }

        // ReSharper disable UnusedMember.Local
        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly CountdownEvent _ce;

            public DebugView(CountdownEvent ce)
            {
                _ce = ce;
            }

            public int CurrentCount => _ce._count;

            public Task Task => _ce._tcs.Task;
        }
        // ReSharper restore UnusedMember.Local
    }
}