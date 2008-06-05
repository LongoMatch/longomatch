// TimeLine.cs created
//
//  Copyright (C) 2007 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
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
		
			
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event PositionChangedHandler PositionChanged;
		
		public TimeLine()
		{
			this.Build();
			
		}
		
		public void UpdateStartTime (Time start){
			this.tNode.Start = start;
			
			//El tiempo lo tenemos que obtener en ms para no perder  precisión
			//y volver a pasarlo a segundos para el calculo del framerate
			//cambiarlo con un nuevo Método
			this.startFrame = GetFrame(tNode.Start);
			this.timescale1.AdjustPosition(startFrame,TimeScale.START);
			
		
		}
		
		public void UpdateStopTime (Time stop){
			this.tNode.Stop = stop;
			this.stopFrame = GetFrame(tNode.Stop);
			this.timescale1.AdjustPosition(stopFrame,TimeScale.STOP);
			
		
		}
		
		public void UpdateTimeNode(TimeNode tNode){
			if (this.tNode != null){
				this.tNode = tNode;
				this.startFrame = GetFrame(tNode.Start);
				this.stopFrame = GetFrame(tNode.Stop);
				this.timeprecisionadjustwidget1.SetTimeNode(tNode);
				this.timescale1.SetSegment(startFrame,stopFrame);

			}
		}
		
		public void SetTimeNode(TimeNode tNode,int framerate){

			//startFrame y stopFrame se actualizan al llamar a la función SetBounds
			//por lo tanto no se pueden usar las variables del objecto
			int startFrame = GetFrame(tNode.Start);
			int stopFrame = GetFrame(tNode.Stop);

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
		
		public void SetPosition(Time pos){
			double framePos;
			framePos = GetFrame(pos);
			this.timescale1.AdjustPosition(framePos,TimeScale.POS);
		}
			
		private int GetFrame (Time time){
			return time.MSeconds * framerate /MS;
			
		}
		private void ApplyZoomValue(uint zoomValue){
			
			int lower,upper,gap;
			gap = stopFrame - startFrame;	
			lower =  startFrame-gap*(float)zoomValue/2 >= 0 ? (int)(startFrame-gap*(float)zoomValue/2) : 0;
			//Hay que poner un máximo con la longitud del clip
			upper = (int)(stopFrame+gap*(float)zoomValue/2);
			this.timescale1.SetBounds(lower,upper);	

			
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

		protected virtual void OnPosValueChanged (object o, LongoMatch.PosChangedArgs args)
		{
			double pos = args.Val;
		
			if (PositionChanged  != null && pos >startFrame && pos < stopFrame)
				this.PositionChanged(new Time ((int)pos*MS/framerate));
		}

		protected virtual void OnStopValueChanged (object o, LongoMatch.OutChangedArgs args)
		{
			double pos = args.Val;		
			
			if (tNode!= null){
				
				this.stopFrame = (int)pos;
				tNode.Stop.MSeconds = (int)(stopFrame*MS/framerate);
				
				this.timeprecisionadjustwidget1.SetTimeNode(tNode);
				if (TimeNodeChanged != null)
					TimeNodeChanged(tNode,tNode.Stop);
			}
		
		}

		protected virtual void OnStartValueChanged (object o, LongoMatch.InChangedArgs args)
		{
			double pos = args.Val;
			if (tNode != null){
				this.startFrame = (int)pos;
				this.tNode.Start.MSeconds = (int)startFrame*MS/framerate;
				this.timeprecisionadjustwidget1.SetTimeNode(tNode);
				if (TimeNodeChanged != null)
					TimeNodeChanged(tNode,tNode.Start);
			}
			
			
		
		}

	
	}
}
