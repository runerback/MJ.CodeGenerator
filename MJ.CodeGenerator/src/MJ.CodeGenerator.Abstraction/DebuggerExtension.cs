using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MJ.CodeGenerator
{
    public static class DebuggerExtension
    {
        public const int DefaultWaitTimeout = 30 * 1000;
        public const int MinWaitTimeout = 1000;

        [DebuggerStepThrough]
        public static void TryLaunchDebugger(this object? _)
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
        }

        /// <summary>
        /// Wait for any debugger to attach.
        /// </summary>
        /// <param name="_"></param>
        /// <param name="timeout">in Millisecond</param>
        [DebuggerStepThrough]
        public static async Task WaitForDebugger(this object? _, int timeout = DefaultWaitTimeout)
        {
            if (Debugger.IsAttached || timeout < MinWaitTimeout)
            {
                return;
            }

            await Task.WhenAny(
                ((Func<Task>)(async () =>
                {
                    while (true)
                    {
                        if (Debugger.IsAttached)
                        {
                            break;
                        }

                        await Task.Delay(1000);
                    }
                }))(),
                ((Func<Task>)(async () =>
                {
                    await Task.Delay(timeout);

                    throw new TimeoutException();
                }))());
        }
    }
}