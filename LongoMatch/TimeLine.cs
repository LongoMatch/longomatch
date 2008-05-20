// TimeLine.cs created with MonoDevelop
// User: ando at 21:08 06/01/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace LongoMatch
{
	
	
	public partial class TimeLine : Gtk.Bin
	{
		
		// Porcentaje visible de la barra de tiempos

		private const int MS = 1000;
		
		private int startFrame;
		private int stopFrame;
		private int framerate;		
		private uint zoomValue=4;
		private TimeNode tNode;
		private bool enabled = false;
		
		public event PosValueChangedHandler PosValueChanged;
		public event StartValueChangedHandler StartValueChanged;
		public event StopValueChangedHandler StopValueChanged;		
		public event TimeNodeChangedHandler TimeNodeChanged;
		
		public TimeLine()
		{
			this.Build();
			
		}
		
		public void UpdateStartTime (long start){
			this.tNode.Start = start;
			this.startFrame = (int) (tNode.Start *framerate /MS);
			this.timescale1.AdjustPosition(startFrame,TimeScale.START);
			
		
		}
		
		public void UpdateStopTime (long stop){
			this.tNode.Stop = stop;
			this.stopFrame = (int) (tNode.Stop *framerate /MS);
			this.timescale1.AdjustPosition(stopFrame,TimeScale.STOP);
			
		
		}
		
		public void UpdateTimeNode(TimeNode tNode){
			if (this.tNode != null){
				this.tNode = tNode;
				this.startFrame = (int) (tNode.Start *framerate /MS);
				this.stopFrame = (int) (tNode.Stop * framerate /MS);
				this.timeprecisionadjustwidget1.SetTimeNode(tNode);
				this.timescale1.SetSegment(startFrame,stopFrame);

			}
		}
		
		public void SetTimeNode(TimeNode tNode,int framerate){

			//startFrame y stopFrame se actualizan al llamar a la función SetBounds
			//por lo tanto no se pueden usar las variables del objecto
			int startFrame = (int) (tNode.Start *framerate /MS);
			int stopFrame = (int) (tNode.Stop * framerate /MS);

			this.startFrame = startFrame;
			this.stopFrame = stopFrame;
			this.framerate = framerate;
			this.tNode = tNode;
			this.timeprecisionadjustwidget1.SetTimeNode(tNode);
			this.ApplyZoomValue(zoomValue);
			this.timescale1.SetSegment(startFrame,stopFrame);

		}
		
		public bool Enabled{
			set{
				this.enabled = value;
				if (enabled)
					this.Show();
				else
					this.Hide();					
			}
			get {
				return this.enabled;
			}
		}
		
		public void SetPosition(long pos){
			double framePos;
			framePos = pos * framerate / MS;
			this.timescale1.AdjustPosition(framePos,TimeScale.POS);
		}
			
		private void ApplyZoomValue(uint zoomValue){
			
			int lower,upper,gap;
			gap = stopFrame - startFrame;	
			lower =  startFrame-gap*(float)zoomValue/2 >= 0 ? (int)(startFrame-gap*(float)zoomValue/2) : 0;
			//Hay que poner un máximo con la longitud del clip
			upper = (int)(stopFrame+gap*(float)zoomValue/2);
			this.timescale1.SetBounds(lower,upper);	

			
		}

		protected virtual void OnTimescale1PosValueChanged (double pos)
		{
			if (PosValueChanged  != null && pos >startFrame && pos < stopFrame)
				this.PosValueChanged(pos);
		}

		protected virtual void OnTimescale1StartValueChanged (double pos)
		{
			if (tNode != null){
				this.startFrame = (int)pos;
				this.tNode.Start = (long)startFrame*MS/framerate;
				this.timeprecisionadjustwidget1.SetTimeNode(tNode);
				if (TimeNodeChanged != null)
					TimeNodeChanged(tNode,tNode.Start);
			}
			
		}

		protected virtual void OnTimescale1StopValueChanged (double pos)
		{
			if (tNode!= null){
				this.stopFrame = (int)pos;
				this.tNode.Stop = (long)stopFrame*MS/framerate;
				this.timeprecisionadjustwidget1.SetTimeNode(tNode);
				if (TimeNodeChanged != null)
					TimeNodeChanged(tNode,tNode.Stop);
			}
		}

		protected virtual void OnZoominbuttonClicked (object sender, System.EventArgs e)
		{
			if (zoomValue>0){
				this.zoomValue--;
				this.ApplyZoomValue(this.zoomValue);
			}
			
		}

		protected virtual void OnZoomoutbuttonClicked (object sender, System.EventArgs e)
		{
			if (zoomValue<20){
				zoomValue++;
				this.ApplyZoomValue(zoomValue);
			}
			
		}
	}
}
