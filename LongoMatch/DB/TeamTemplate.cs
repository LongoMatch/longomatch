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
using LongoMatch.TimeNodes;

namespace LongoMatch.DB
{
	
	
	public class TeamTemplate
	{
		private List<Player> playersList;
	
		
		
		public TeamTemplate()
		{
			playersList = new List<Player>();	
			
		}
		
		public int PlayersCount{
			get {return playersList.Count;}
		}
	
		public void CreateDefaultTemplate(int playersCount){
			for (int i=0; i<playersCount;i++){
				playersList.Add(new Player("Player "+i,"",i));
			}
		}
		
		public void SetPlayersList(List<player> playersList){
				this.playersList = playersList;
		}
		
		public Player GetPlayer(int index){
			if (index >= PlayersCount)
				throw new Exception("The actual team template doesn't have so many players. Requesting player {0} but players count is {1}",index, PlayersCount);
			return playersList[index];
		}
		
		public List<Player> GetPlayersList(){
			return playersList;
		}
	}
}
