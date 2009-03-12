// Handlers.cs
//
//  Copyright (C) 2008 Andoni Morales Alastruey
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
using LongoMatch;
using LongoMatch.TimeNodes;

namespace LongoMatch.Handlers
{
		
	//Manejador para el evento producido al seleccionar un nodo en el árbol
	public delegate void TimeNodeSelectedHandler (MediaTimeNode tNode);
	//Manejador para el evento producido al pulsar un botón de selección de nuava marca
	public delegate void NewMarkEventHandler (int i);	
	//Manejador para el evento producido al pulsar un botón de selección de nuava marca
	public delegate void NewMarkAtFrameEventHandler (int i,int frame);	
	//Manejador para el evento producido cuando se edita un nodo
	public delegate void TimeNodeChangedHandler (TimeNode tNode, object val);
	//Manejador para el evento producido al eliminar un MediaTimeNode
	public delegate void TimeNodeDeletedHandler (MediaTimeNode tNode);
	//Manejador para el evento producido al inserir un MediaTimeNode en la lista de reproducción
	public delegate void PlayListNodeAddedHandler(MediaTimeNode tNode);
	//Manejador para el evento producido al selecionar un nodo en la lista de reproducción
	public delegate void PlayListNodeSelectedHandler (PlayListTimeNode plNode, bool hasNext);
	//Manejador para el evento producido al ajustar la posición 
	public delegate void PositionChangedHandler (Time pos);	
	
	public delegate void ProgressHandler (float progress);	
	
	public delegate void DateSelectedHandler (DateTime selectedDate);
	
	public delegate void SnapshotSeriesHandler(MediaTimeNode tNode);
	
	public delegate void NewVersionHandler(Version version, string URL);
	
}
