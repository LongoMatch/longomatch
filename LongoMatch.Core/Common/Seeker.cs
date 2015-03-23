using LongoMatch.Core.Store;
using LongoMatch.Core.Handlers;
using Timer = System.Threading.Timer;
using System.Threading;

namespace LongoMatch.Core.Common
{
	public class Seeker
	{
		public event SeekHandler SeekEvent;

		uint timeout;
		bool pendingSeek;
		bool waiting;
		Time start;
		float rate;
		SeekType seekType;
		Timer timer;

		public Seeker (uint timeoutMS = 80)
		{
			timeout = timeoutMS;
			pendingSeek = false;
			seekType = SeekType.None;
			timer = new Timer (HandleSeekTimeout);
		}

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

