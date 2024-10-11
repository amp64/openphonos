using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenPhonos.UPnP
{

    public class WaitableEvent
    {
        readonly AutoResetEvent RealEvent = new AutoResetEvent(false);

        public WaitableEvent() { }

        public void Set()
        {
            RealEvent.Set();
        }

        public async Task WaitAsync(int milliseconds)
        {
            await Task.Run(
                () => RealEvent.WaitOne(milliseconds)
                );
        }

        public async Task<bool> WaitAsync(TimeSpan timeout)
        {
            bool signalled = false;

            await Task.Run(
                () =>
                {
                    signalled = RealEvent.WaitOne((int)timeout.TotalMilliseconds);
                }
                );

            return signalled;
        }

    }
}
