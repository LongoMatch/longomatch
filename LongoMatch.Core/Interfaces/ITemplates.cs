// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.Store;

namespace LongoMatch.Core.Interfaces
{
	public interface ITemplate: IStorable
	{
		string Name { get; set; }

		bool Static  { get; set; }

	}

	public interface ITemplate<T>: ITemplate
	{
		T Copy (string newName);
	}

	public interface ITemplateProvider
	{
		bool Exists (string name);

		void Create (string templateName, params object[] list);
	}

	public interface ITemplateProvider<T>: ITemplateProvider where T: ITemplate
	{
		List<T> Templates { get; }

		T LoadFile (string filename);

		void Save (T template);

		void Delete (T template);

		void Register (T template);

		void Copy (T template, string copy);
	}

	public interface ICategoriesTemplatesProvider: ITemplateProvider<Dashboard>
	{

	}

	public interface ITeamTemplatesProvider: ITemplateProvider<Team>
	{

	}
}

