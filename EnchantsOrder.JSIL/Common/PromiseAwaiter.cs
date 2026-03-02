using JSIL;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EnchantsOrder.JSIL.Common
{
    public sealed class Promise<TResult>(object promise)
    {
        private readonly dynamic promise = promise;
        private Action? continuationActions;

        public bool IsCompleted { get; private set; }

        public TResult? Result { get; private set; }

        /// <inheritdoc/>
        public PromiseAwaiter GetAwaiter() => new(this);

        public Task<TResult> AsTask()
        {
            TaskCompletionSource<TResult> tcs = new();
            promise.then(
                new Action<dynamic>(value => _ = tcs.TrySetResult(value)),
                new Action<dynamic>(reason => _ = tcs.TrySetException(new Exception(reason.ToString()))));
            return tcs.Task;
        }

        [SuppressMessage("Style", "IDE1006:命名样式", Justification = "<挂起>")]
        public void then(Action<TResult> onFulfilled, Action<object>? onRejected = null) => promise.then(onFulfilled, onRejected);

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
                promise.then(new Action<dynamic, object>((onFulfilled, onRejected) =>
                {
                    IsCompleted = true;
                    Result = onFulfilled;
                    if (Builtins.IsTruthy(onRejected))
                    {
                        Builtins.CreateNamedFunction<Action<object>>("Throw", ["error"], "throw error")(onRejected);
                    }
                    continuationActions?.Invoke();
                }));
            }
            else
            {
                continuationActions += continuationAction;
            }
        }

        /// <summary>
        /// Provides an awaiter for awaiting a <see cref="Promise{TResult}"/>.
        /// </summary>
        public readonly struct PromiseAwaiter(Promise<TResult> task) : ICriticalNotifyCompletion
        {
            /// <summary>
            /// The task being awaited.
            /// </summary>
            private readonly Promise<TResult> m_task = task;

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
            internal static void OnCompletedInternal(Promise<TResult> task, Action continuation)
            {
                ArgumentNullException.ThrowIfNull(continuation);

                // Set the continuation onto the awaited task.
                task.SetContinuationForAwait(continuation);
            }
        }
    }
}
