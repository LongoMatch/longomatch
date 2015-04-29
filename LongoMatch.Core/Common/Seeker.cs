using System;
using System.Threading;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Store;
using Timer = System.Threading.Timer;

namespace LongoMatch.Core.Common
{
	public class Seeker: IDisposable
	{
		public event SeekHandler SeekEvent;

		uint timeout;
		bool pendingSeek;
		bool waiting;
		bool disposed;
		Time start;
		float rate;
		SeekType seekType;
		readonly Timer timer;
		readonly ManualResetEvent TimerDisposed;

		public Seeker (uint timeoutMS = 80)
		{
			timeout = timeoutMS;
			pendingSeek = false;
			disposed = false;
			seekType = SeekType.None;
			timer = new Timer (HandleSeekTimeout);
			TimerDisposed = new ManualResetEvent (false);
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			if (!disposed) {
				timer.Dispose (TimerDisposed);
				TimerDisposed.WaitOne (200);
				TimerDisposed.Dispose ();
			}
			disposed = true;
		}

		#endregion

		public void Seek (SeekType seekType, Time start = null, float rate = 1)
		{
			this.seekType = seekType;
			this.start = start;
			this.rate = rate;

			pendingSeek = true;
			if (waiting) {
				return;
			}

			HandleSeekTimeout (this);
			waiting = true;
			timer.Change (timeout, Timeout.Infinite);
		}

		void HandleSeekTimeout (object state)
		{
			if (disposed) {
				return;
			}

			waiting = false;
			if (pendingSeek) {
				if (seekType != SeekType.None) {
					if (SeekEvent != null) {
						SeekEvent (seekType, start, rate);
					}
					seekType = SeekType.None;
				}
			}
			timer.Change (Timeout.Infinite, Timeout.Infinite);
		}
	}
}

