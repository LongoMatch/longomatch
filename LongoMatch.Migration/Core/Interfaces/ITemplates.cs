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
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Interfaces
{
	public interface ITemplate
	{
		void Save (string filename);

		string Name { get; set; }
	}

	public interface ITemplate<T>: ITemplate, IList<T>
	{
		T AddDefaultItem (int index);
	}

	public interface ITemplateProvider
	{
		void CheckDefaultTemplate ();

		List<string> TemplatesNames { get; }

		bool Exists (string name);

		void Copy (string orig, string copy);

		void Delete (string templateName);

		void Create (string templateName, params object[] list);
	}

	public interface ITemplateProvider<T, U>: ITemplateProvider where T: ITemplate<U>
	{
		List<T> Templates { get; }

		T Load (string name);

		void Save (ITemplate<U> template);

		void Update (ITemplate<U> template);
	}

	public interface ITemplateWidget<T, U> where T: ITemplate<U>
	{
		T Template { get; set; }

		bool Edited { get; set; }

		bool CanExport { get; set; }

		Project Project { get; set; }
	}

	public interface ICategoriesTemplatesProvider: ITemplateProvider<Categories, Category>
	{

	}

	public interface ITeamTemplatesProvider: ITemplateProvider<TeamTemplate, Player>
	{

	}

	public interface ISubcategoriesTemplatesProvider: ITemplateProvider<SubCategoryTemplate, string>
	{

	}

	public interface ICategoriesTemplatesEditor: ITemplateWidget<Categories, Category>
	{

	}

	public interface ITeamTemplatesEditor: ITemplateWidget<TeamTemplate, Player>
	{

	}
}

