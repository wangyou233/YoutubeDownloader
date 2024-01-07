using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeDownloader.Utils
{
    /// <summary>
    /// 一个可动态调整信号量大小的类，实现异步获取和释放资源的功能。
    /// </summary>
    internal partial class ResizableSemaphore : IDisposable
    {
        private readonly object _syncLock = new();
        private readonly Queue<TaskCompletionSource> _waitersQueue = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private bool _disposed;
        private int _maxCount;
        private int _currentCount;

        public int MaxCount
        {
            get
            {
                lock (_syncLock)
                {
                    return _maxCount;
                }
            }
            set
            {
                lock (_syncLock)
                {
                    _maxCount = value;
                    Refresh();
                }
            }
        }

        private void Refresh()
        {
            lock (_syncLock)
            {
                while (_currentCount < _maxCount && _waitersQueue.TryDequeue(out var waiter))
                {
                    // 如果等待者未被取消，则增加计数并唤醒等待的任务
                    if (waiter.TrySetResult())
                        _currentCount++;
                }
            }
        }

        public async Task<IDisposable> AcquireAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ResizableSemaphore));

            var waiter = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            // 注册取消令牌回调以取消等待任务
            await using var registration1 = _cancellationTokenSource.Token.Register(() => waiter.TrySetCanceled(_cancellationTokenSource.Token));
            await using var registration2 = cancellationToken.Register(() => waiter.TrySetCanceled(cancellationToken));

            lock (_syncLock)
            {
                _waitersQueue.Enqueue(waiter);
                Refresh();
            }

            // 等待获取到信号量后返回一个表示已获取资源的对象
            await waiter.Task;
            return new AcquiredAccess(this);
        }

        private void Release()
        {
            lock (_syncLock)
            {
                _currentCount--;
                Refresh();
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        // 内部类，用于表示已经获取资源的状态，并在释放时减少信号量计数
        private sealed class AcquiredAccess : IDisposable
        {
            private readonly ResizableSemaphore _semaphore;

            public AcquiredAccess(ResizableSemaphore semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose() => _semaphore.Release();
        }
    }
}