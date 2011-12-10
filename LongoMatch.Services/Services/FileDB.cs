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
using System.IO;

using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Store;

namespace LongoMatch.Services
{
	public class FileDB: IDatabase
	{
		const string DESC = "desc";
		const string PROJECTS = "projects";
		
		string desc_path;
		string project_path;
		
		public FileDB (string filename)
		{
			desc_path = Path.Combine(filename, DESC);
			project_path = Path.Combine(filename, PROJECTS);
			
			if (!Directory.Exists(desc_path))
				Directory.CreateDirectory(desc_path);
			if (!Directory.Exists(project_path))
				Directory.CreateDirectory(project_path);
			
		}
		
		public List<ProjectDescription> GetAllProjects() {
			List<ProjectDescription> list = new List<ProjectDescription>();
			foreach (string path in Directory.GetFiles(desc_path)) {
				if (File.Exists(Path.Combine(project_path, Path.GetFileName(path))))
					list.Add(SerializableObject.Load<ProjectDescription>(path));
			}
			return list;
		}

		public Project GetProject(Guid id) {
			string path = Path.Combine(project_path, id.ToString());
			if (File.Exists(path))
				return SerializableObject.Load<Project>(path);
			return null;
		}
		
		public void AddProject(Project project){
			string path = Path.Combine(project_path, project.UUID.ToString());
			
			project.Description.LastModified = DateTime.Now;
			if (File.Exists(path))
				File.Delete(path);
			SerializableObject.Save(project, path);
			SerializableObject.Save(project.Description, Path.Combine(desc_path, project.UUID.ToString()));
		}
		
		public void RemoveProject(Guid id) {
			string path = Path.Combine(project_path, id.ToString());
			if (File.Exists(path))
				File.Delete(path);
				
			path = Path.Combine(desc_path, id.ToString());
			if (File.Exists(path))
				File.Delete(path);
		}
		
		public void UpdateProject(Project project) {
			project.Description.LastModified = DateTime.Now;
			AddProject(project);
		}
		
		public bool Exists(Project project) {
			return File.Exists(Path.Combine(desc_path, project.UUID.ToString())) &&
				File.Exists(Path.Combine(project_path, project.UUID.ToString()));
		}
	}
}

