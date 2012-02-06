// 
//  Copyright (C) 2012 Andoni Morales Alastruey
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
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
// 
using System;
using System.Collections.Generic;
using System.Linq;

using LongoMatch.Store;

namespace LongoMatch.Stats
{
	public class GameUnitsStats
	{
		TimelineNode game;
		GameUnitStatsNode gameNode;
		Dictionary<GameUnit, GameUnitStatsNode> gameUnitNodes;
		
		const float MAX_DIFF = (float) 80 / 100; 
		
		public GameUnitsStats (GameUnitsList guList, int duration)
		{
			game = new TimelineNode{Start=new Time(0), Stop=new Time(duration)};
			gameNode = new GameUnitStatsNode(game);
			gameUnitNodes = new Dictionary<GameUnit, GameUnitStatsNode>();
			GroupGameStats(guList);
		}
		
		public Dictionary<GameUnit, GameUnitStatsNode> GameUnitNodes {
			get {
				return gameUnitNodes;
			}
		}
		
		public GameUnitStatsNode GameNode {
			get {
				return gameNode;
			}
		}
		
		void GroupGameStats (GameUnitsList guList)
		{
			List<GameUnitStatsNode> parents = new List<GameUnitStatsNode>();
			parents.Add(gameNode);
			
			foreach (GameUnit gu in guList) {
				List<GameUnitStatsNode> nextParents = new List<GameUnitStatsNode>();
				foreach (TimelineNode tn in gu) {
					GameUnitStatsNode node = new GameUnitStatsNode(tn);
					nextParents.Add(node);
					FindParent(node, parents);
				}
				gameUnitNodes.Add(gu, new GameUnitStatsNode(game, nextParents));
				parents = nextParents;
			}
		}
		
		void FindParent (GameUnitStatsNode node, List<GameUnitStatsNode> parents) {
			List <GameUnitStatsNode> candidates = parents.Where(p => Contained(node.Node, p.Node)).ToList();
			if (candidates.Count != 1) {
				Log.Warning(String.Format("Found {0} candidates for node {1}", candidates.Count, node));
			}
				
			foreach (GameUnitStatsNode parent in candidates)
				parent.Add(node);
		}
			
		bool Contained (TimelineNode node, TimelineNode parent) {
			if (node.Start > parent.Start && node.Stop < parent.Stop) {
				return true;
			} else if (node.Start > parent.Start && node.Stop > parent.Stop) {
				return (node.Stop - parent.Stop) < node.Duration * MAX_DIFF;
			} else if (node.Start < parent.Start && node.Stop < parent.Stop) {
				return (parent.Start - node.Start) < node.Duration * MAX_DIFF;
			} else {
				return false;
			}
		}
	}
}

