using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OpenPhonos.UPnP
{
    public static class TaskExtension
    {
        // From https://blogs.msdn.microsoft.com/pfxteam/2011/11/10/crafting-a-task-timeoutafter-method/

        internal static async Task TimeoutAfter(this Task task, int millisecondsTimeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout)))
                await task;
            else
                throw new TimeoutException();
        }

        public static async void Later(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Later-exception: {0}", (object)e.Message);
            }
        }
    }
}
