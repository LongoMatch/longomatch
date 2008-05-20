// PlayListNode.cs created with MonoDevelop
// User: ando at 15:01 10/12/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace LongoMatch
{
	
	
	public class PlayListNode
	{
		private string fileName;
		private string name;
		private long startTime;
		private long stopTime;
		
		public PlayListNode(string fileName, string name, long startTime, long stopTime)
		{
			this.fileName = fileName;
			this.name = name;
			this.stopTime = stopTime;
			this.startTime = startTime;
		}
		
		public PlayListNode(string fileName, TimeNode tNode){
			this.fileName = fileName;
			this.name = tNode.Name;
			this.stopTime = tNode.Stop;
			this.startTime = tNode.Start;
		}
		public string FileName{
			set{ this.fileName = value;}
			get{ return this.fileName;}
		}
		
		public string Name{
			set{ this.name = value;}
			get{ return this.name;}
		}
		
		public long StartTime{
			set{ this.startTime = value;}
			get{ return this.startTime;}
		}
		
		public long StopTime{
			set{ this.stopTime = value;}
			get{ return this.stopTime;}
		}
		
	}
}
