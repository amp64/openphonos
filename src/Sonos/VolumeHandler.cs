using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OpenPhonos.Sonos
{
    //
    // handle the problem of Volume changes and Eventing, when connected to a live UI element
    // Works for Device and Group volumes
    //
    public class VolumeHandler
    {
        private ushort _LastSetVolume;
        private ushort _NextSetVolume;
        private volatile bool _PendingVolume;
        private volatile bool _SettingVolume;
        private AutoResetEvent _VolumeWaiter = new AutoResetEvent(false);
        private Func<ushort, ushort, Task<ushort>> _SetterAsync;

        public bool Logging { get; set; }
        public ushort LastValue { get; private set; }                 // the last value the hardware knows about

        /// <summary>
        /// Pass this the setter function, that takes the old and new volumes and sends it to the device. It needs to return what the new volume level is
        /// </summary>
        /// <param name="setterAsync"></param>
        public VolumeHandler(Func<ushort, ushort, Task<ushort>> setterAsync)
        {
            this._SetterAsync = setterAsync;
        }

        private void Log(string msg, int value)
        {
            if (this.Logging)
                Debug.WriteLine("{2} VolumeHandler {0} ({1})", msg, value, DateTime.Now.TimeOfDay.ToString("t"));
        }

        // sets the volume, then waits for an event to ensure it has gone
        private async Task RealSetZoneVolumeAsync(ushort newvol)
        {
            _SettingVolume = true;
            _LastSetVolume = newvol;
            Log("real", newvol);
            LastValue = await _SetterAsync(LastValue, newvol);
            await Task.Run(() =>
            {
                var ok = _VolumeWaiter.WaitOne(10 * 1000);
                if (!ok)
                    Log("waitfail", newvol);
            });
            _SettingVolume = false;
        }

        // an event came in, if it was the one we sent then ignore it (return false), else it was external so return true
        public bool EventReceived(int eventvol)
        {
            LastValue = (ushort)eventvol;

            if (eventvol != _LastSetVolume)
            {
                Log("event-change", eventvol);
                return true;
            }
            else
            {
                // ignore the event as a result of our own Set call
                Log("event-expected", eventvol);
                _LastSetVolume = 200;                    // impossible value in real-life
                _VolumeWaiter.Set();
                return false;
            }
        }

        /// <summary>
        /// Call this between setting the local property value, and sending the Changed event
        /// </summary>
        /// <param name="vol"></param>
        public void PropSet(ushort vol)
        {
            Log("propsetter", vol);
        }

        /// <summary>
        /// Call this to send the volume value to the device, do this after setting the local value and sending the Changed event
        /// It takes a while to set volume, and we mustn't overlap events, and we must ignore the events resulting from our Set call
        /// </summary>
        /// <param name="newvol"></param>
        /// <returns></returns>
        public async Task SetAsync(ushort newvol)
        {
            if (_SettingVolume)
            {
                Log("ignoring", newvol);
                _NextSetVolume = newvol;                                // send it when we can
                _PendingVolume = true;
                return;
            }

            // actually set the volume (and wait)
            await RealSetZoneVolumeAsync(newvol);

            // if we had some requests in the meantime, send that now
            while (_PendingVolume)
            {
                _PendingVolume = false;
                Log("catchup", _NextSetVolume);
                await RealSetZoneVolumeAsync(_NextSetVolume);
            }
        }
    }
}
