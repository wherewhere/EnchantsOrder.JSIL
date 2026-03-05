using JSIL.Meta;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EnchantsOrder.JSIL.Common
{
    public class Promise;
    public sealed class Promise<TResult> : Promise;

    public static class PromiseImplement
    {
        public static Promise Then<TSource>(this Promise<TSource> promise, Action<TSource> onFulfilled, Action<object>? onRejected = null)
        {
            return then(promise, onFulfilled, onRejected);
            [JSReplacement("$promise.then(onFulfilled, onRejected)")]
            static extern Promise then(Promise<TSource> promise, Action<TSource> onFulfilled, Action<object>? onRejected);
        }

        public static Promise<TResult> Then<TSource, TResult>(this Promise<TSource> promise, Func<TSource, Promise<TResult>> onFulfilled, Action<object>? onRejected = null)
        {
            return then(promise, onFulfilled, onRejected);
            [JSReplacement("$promise.then(onFulfilled)")]
            static extern Promise<TResult> then(Promise<TSource> promise, Func<TSource, Promise<TResult>> onFulfilled, Action<object>? onRejected);
        }

        public static PromiseWrapper<TResult>.PromiseAwaiter GetAwaiter<TResult>(this Promise<TResult> promise)
        {
            return new PromiseWrapper<TResult>(promise).GetAwaiter();
        }
    }

    public sealed class PromiseWrapper<TResult>(Promise<TResult> promise)
    {
        [JSImmutable]
        private readonly Promise<TResult> promise = promise;
        private Action? continuationActions;

        public bool IsCompleted { get; private set; }

        public TResult? Result { get; private set; }

        /// <inheritdoc/>
        public PromiseAwaiter GetAwaiter() => new(this);

        public Task<TResult> AsTask()
        {
            TaskCompletionSource<TResult> tcs = new();
            _ = promise.Then(
                value => _ = tcs.TrySetResult(value),
                reason => _ = tcs.TrySetException(new Exception(reason.ToString())));
            return tcs.Task;
        }

        [JSChangeName("then")]
        public Promise Then(Action<TResult> onFulfilled, Action<object>? onRejected = null) => promise.Then(onFulfilled, onRejected);

        /// <summary>
        /// Sets a continuation onto the <see cref="Promise{TResult}"/>.
        /// The continuation is scheduled to run in the current synchronization context is one exists,
        /// otherwise in the current task scheduler.
        /// </summary>
        /// <param name="continuationAction">The action to invoke when the <see cref="Promise{TResult}"/> has completed.</param>
        internal void SetContinuationForAwait(Action continuationAction)
        {
            if (IsCompleted)
            {
                continuationAction();
            }
            else if (continuationActions == null)
            {
                continuationActions = continuationAction;
                _ = promise.Then(
                    value =>
                    {
                        IsCompleted = true;
                        Result = value;
                        continuationActions?.Invoke();
                    },
                    error => Unsafe.Throw(error));
            }
            else
            {
                continuationActions += continuationAction;
            }
        }

        /// <summary>
        /// Provides an awaiter for awaiting a <see cref="Promise{TResult}"/>.
        /// </summary>
        [JSImmutable]
        public readonly struct PromiseAwaiter(PromiseWrapper<TResult> task) : ICriticalNotifyCompletion
        {
            /// <summary>
            /// The task being awaited.
            /// </summary>
            [JSImmutable]
            private readonly PromiseWrapper<TResult> m_task = task;

            /// <summary>
            /// Gets whether the task being awaited is completed.
            /// </summary>
            /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
            public bool IsCompleted => m_task.IsCompleted;

            /// <summary>
            /// Schedules the continuation onto the <see cref="Promise{TResult}"/> associated with this <see cref="PromiseAwaiter"/>.
            /// </summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            public void OnCompleted(Action continuation) => OnCompletedInternal(m_task, continuation);

            /// <summary>
            /// Schedules the continuation onto the <see cref="Promise{TResult}"/> associated with this <see cref="PromiseAwaiter"/>.
            /// </summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            public void UnsafeOnCompleted(Action continuation) => OnCompletedInternal(m_task, continuation);

            /// <summary>
            /// Ends the await on the completed <see cref="Promise{TResult}"/>.
            /// </summary>
            /// <returns>The result of the completed <see cref="Promise{TResult}"/>.</returns>
            [StackTraceHidden]
            public TResult GetResult() => m_task.Result!;

            /// <summary>
            /// Schedules the continuation onto the <see cref="Promise{TResult}"/> associated with this <see cref="PromiseAwaiter"/>.
            /// </summary>
            /// <param name="task">The task being awaited.</param>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            internal static void OnCompletedInternal(PromiseWrapper<TResult> task, Action continuation)
            {
                ArgumentNullException.ThrowIfNull(continuation);

                // Set the continuation onto the awaited task.
                task.SetContinuationForAwait(continuation);
            }
        }
    }
}
