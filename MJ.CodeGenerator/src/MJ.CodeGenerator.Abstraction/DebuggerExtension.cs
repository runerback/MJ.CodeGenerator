using System.Diagnostics;

namespace MJ.CodeGenerator
{
    public static class DebuggerExtension
    {
        [DebuggerStepThrough]
        public static void TryLaunchDebugger(this object _)
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
        }
    }
}