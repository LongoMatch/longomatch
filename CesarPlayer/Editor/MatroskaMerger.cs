// 
//  Copyright (C) 2009 Andoni Morales Alastruey 2009
// 
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 

using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace LongoMatch.Video.Editor
{
	
	
	public class MatroskaMerger:GenericMerger
	{
		
		
		
		public MatroskaMerger(): base ("mkvmerge")
		{			
		}
		
	
		
		protected override string CreateMergeCommandLineArgs(){
		 	int i=0;
			string appendTo="";
			//string args = String.Format("-o {0}  --language 1:eng --track-name 1:Video --default-track 1:yes --display-dimensions 1:{1}x{2} ",
			//                            outputFile, width, height);
			string args = String.Format("-o {0}  --language 1:eng --track-name 1:Video --default-track 1:yes ", outputFile);	
			
			foreach (String path in filesToMergeList){
				if (i==0){
					args += String.Format ("-d 1 -A -S {0} ", path);
				}
				if (i==1){
					args += String.Format ("-d 1 -A -S +{0} ", path);
					appendTo += String.Format(" --append-to {0}:1:{1}:1",i,i-1);
				}
				else if (i>1){
					args += String.Format ("-d 1 -A -S +{0} ", path);
					appendTo += String.Format(",{0}:1:{1}:1",i,i-1);
				}				
				i++;
			}
			
			args += String.Format("--track-order 0:1 {0}", appendTo);
			
			return args;
		}		
		
	}
}
