using JSIL.Meta;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EnchantsOrder.JSIL.Common
{
    public interface IPromise;
    public interface IPromise<TResult> : IPromise;

    public static class PromiseImplement
    {
        public static IPromise Then<TSource>(this IPromise<TSource> promise, Action<TSource> onFulfilled, Action<object>? onRejected = null)
        {
            return then(promise, onFulfilled, onRejected);
            [JSReplacement("$promise.then(onFulfilled, onRejected)")]
            static extern IPromise then(IPromise<TSource> promise, Action<TSource> onFulfilled, Action<object>? onRejected);
        }

        public static IPromise<TResult> Then<TSource, TResult>(this IPromise<TSource> promise, Func<TSource, IPromise<TResult>> onFulfilled, Action<object>? onRejected = null)
        {
            return then(promise, onFulfilled, onRejected);
            [JSReplacement("$promise.then(onFulfilled)")]
            static extern IPromise<TResult> then(IPromise<TSource> promise, Func<TSource, IPromise<TResult>> onFulfilled, Action<object>? onRejected);
        }

        public static PromiseWrapper<TResult>.PromiseAwaiter GetAwaiter<TResult>(this IPromise<TResult> promise) => new PromiseWrapper<TResult>(promise).GetAwaiter();
    }

    public sealed class PromiseWrapper<TResult>(IPromise<TResult> promise)
    {
        [JSImmutable]
        private readonly IPromise<TResult> promise = promise;
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
                reason => _ = tcs.TrySetException(reason.As<object, Exception>()));
            return tcs.Task;
        }

        [JSChangeName("then")]
        public IPromise Then(Action<TResult> onFulfilled, Action<object>? onRejected = null) => promise.Then(onFulfilled, onRejected);

        /// <summary>
        /// Sets a continuation onto the <see cref="IPromise{TResult}"/>.
        /// The continuation is scheduled to run in the current synchronization context is one exists,
        /// otherwise in the current task scheduler.
        /// </summary>
        /// <param name="continuationAction">The action to invoke when the <see cref="IPromise{TResult}"/> has completed.</param>
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
        /// Provides an awaiter for awaiting a <see cref="IPromise{TResult}"/>.
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
            /// Schedules the continuation onto the <see cref="IPromise{TResult}"/> associated with this <see cref="PromiseAwaiter"/>.
            /// </summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            public void OnCompleted(Action continuation) => OnCompletedInternal(m_task, continuation);

            /// <summary>
            /// Schedules the continuation onto the <see cref="IPromise{TResult}"/> associated with this <see cref="PromiseAwaiter"/>.
            /// </summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            public void UnsafeOnCompleted(Action continuation) => OnCompletedInternal(m_task, continuation);

            /// <summary>
            /// Ends the await on the completed <see cref="IPromise{TResult}"/>.
            /// </summary>
            /// <returns>The result of the completed <see cref="IPromise{TResult}"/>.</returns>
            [StackTraceHidden]
            public TResult GetResult() => m_task.Result!;

            /// <summary>
            /// Schedules the continuation onto the <see cref="IPromise{TResult}"/> associated with this <see cref="PromiseAwaiter"/>.
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
