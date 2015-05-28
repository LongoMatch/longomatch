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
		static object lockObject = new Object ();
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

		/// <summary>
		/// Schedule a throttled seek of the specified seekType, start and rate. The seek won't happen right away, instead
		/// a timer will be put in place if none already exist and the seek value will be stored. When the timer is fired
		/// the latest seek value will be used to do the seek. This is mostly useful to avoid flooding the media engine with
		/// seek requests when the user is scrubbing the user interface.
		/// </summary>
		/// <param name="seekType">Seek type.</param>
		/// <param name="start">Start.</param>
		/// <param name="rate">Rate.</param>
		public void Seek (SeekType seekType, Time start = null, float rate = 1)
		{
			lock (lockObject) {
				this.seekType = seekType;
				this.start = start;
				this.rate = rate;

				// If a we are already waiting for the timer, return.
				if (waiting) {
					return;
				}

				// Schedule timer
				timer.Change (timeout, Timeout.Infinite);
				// And remember we are waiting
				waiting = true;
			}
		}


		/// <summary>
		/// Called when the timer is fired and will do the actual seek. Note that a lock is used to protect
		/// member variables as the timers are called from a different thread.
		/// </summary>
		/// <param name="state">State.</param>
		void HandleSeekTimeout (object state)
		{
			if (disposed) {
				return;
			}

			lock (lockObject) {
				if (seekType != SeekType.None) {
					if (SeekEvent != null) {
						SeekEvent (seekType, start, rate);
					}
				}
				// Unschedule timer
				timer.Change (Timeout.Infinite, Timeout.Infinite);
				// We are not going to be called anymore until reschedule
				waiting = false;
			}
		}
	}
}

