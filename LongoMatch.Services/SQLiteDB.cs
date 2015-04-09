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
using System.Data;
using Mono.Data.Sqlite;

using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Services
{
	public class SQLiteDB: IDatabase
	{
		string dbFile;
		Version dbVersion;
		
		public SQLiteDB (string file)
		{
			dbFile = file;
			Init();
		}
		
		/// <value>
		/// The database version
		/// </value>
		public Version Version {
			get {
				return dbVersion;
			}
		}
		
	
		/// <summary>
		/// Initialize the Database
		/// </summary>
		public void Init() {
			/* Create a new DB if it doesn't exists yet */
			if(!System.IO.File.Exists(dbFile))
				CreateNewDB();

			Log.Information ("Using database file: " + dbFile);
			
		/*	GetDBVersion();
			GetBackupDate();
			CheckDB();*/
		}
		
		/// <summary>
		/// Retrieve all the projects from the database. This method don't return the
		/// the whole <see cref="LongoMatch.DB.Project"/> but the projects fields to
		/// create a <see cref="LongoMatch.DB.ProjectDescription"/> to make the seek
		/// faster.
		/// </summary>
		/// <returns>
		/// A <see cref="List"/>
		/// </returns>
		public List<ProjectDescription> GetAllProjects() {
			List<ProjectDescription> list = new List<ProjectDescription>();
			SqliteConnection conn = new SqliteConnection("URI=file:" + dbFile);
			
			using (var cmd = conn.CreateCommand())
			{
				conn.Open();
				cmd.CommandText = "SELECT * FROM projects_desc";
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						byte[] data = GetBytes(reader);
						using (MemoryStream stream = new MemoryStream(data)) {
							list.Add(SerializableObject.Load<ProjectDescription>(stream));
						}
					}
				}
			}
			conn.Close();
			return list;
		}

		/// <summary>
		/// Search and return a project in the database. Returns null if the
		/// project is not found
		/// </summary>
		/// <param name="filename">
		/// A <see cref="System.String"/> with the project's video file name
		/// </param>
		/// <returns>
		/// A <see cref="LongoMatch.DB.Project"/>
		/// </returns>
		public Project GetProject(Guid id) {
			return ReadObject<Project>("projects", id.ToString());
		}

		/// <summary>
		/// Add a project to the databse
		/// </summary>
		/// <param name="project">
		/// A <see cref="Project"/> to add
		/// </param>
		public void AddProject(Project project) {
			SaveObject("projects", project.UUID.ToString(), project, false);
			SaveObject("projects_desc", project.UUID.ToString(), project.Description, false);
		}

		/// <summary>
		/// Delete a project from the database
		/// </summary>
		/// <param name="filePath">
		/// A <see cref="System.String"/> with the project's video file path
		/// </param>
		public void RemoveProject(Guid id) {
			ExecNonQuery(String.Format("DELETE FROM {0} where ID={1}", "projects", id.ToString()));
			ExecNonQuery(String.Format("DELETE FROM {0} where ID={1}", "projects_desc", id.ToString()));
		
		}

		/// <summary>
		/// Updates a project in the database. Because a <see cref="LongoMatch.DB.Project"/> has
		/// many objects associated, a simple update would leave in the databse many orphaned objects.
		/// Therefore we need to delete the old project a replace it with the changed one.
		/// </summary>
		/// <param name="project">
		/// A <see cref="Project"/> to update
		/// </param>
		/// <param name="previousFileName">
		/// A <see cref="System.String"/> with the old file path
		/// </param>
		public void UpdateProject(Project project) {
			SaveObject("projects", project.UUID.ToString(), project, true);
			SaveObject("projects_desc", project.UUID.ToString(), project.Description, true);
		}

		/// <summary>
		/// Checks if a project already exists in the DataBase with the same file
		/// </summary>
		/// <param name="project">
		/// A <see cref="Project"/> to compare
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Exists(Project project) {
			bool exists;
			SqliteConnection conn = new SqliteConnection("URI=file:" + dbFile);
			
			using (var cmd = conn.CreateCommand())
			{
				conn.Open();
				cmd.CommandText ="SELECT ID FROM projects_desc";
				using (var reader = cmd.ExecuteReader()) {
					exists = reader.Read() ? true : false;
				}
			}
			conn.Close();
			return exists;
		}
		
		void ExecNonQuery(string sql) {
			SqliteConnection conn = new SqliteConnection("URI=file:" + dbFile);
			
			using (var cmd = conn.CreateCommand())
			{
				conn.Open();
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
				cmd.Dispose();
			}
			conn.Close();
		}
		
		T ReadObject<T>(string table, string id) {
			T obj = default(T);
			SqliteConnection conn = new SqliteConnection("URI=file:" + dbFile);
			
			using (var cmd = conn.CreateCommand())
			{
				conn.Open();
				cmd.CommandText = String.Format("SELECT Data FROM {0} WHERE ID= '{1}'", table, id);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						byte[] data = GetBytes(reader);
						using (MemoryStream stream = new MemoryStream(data)) {
							obj = SerializableObject.Load<T>(stream);
						}
					} else {
						Log.Error ("Project with ID " + id + "not found in table " + table);
					}
				}
			}
			conn.Close();
			return obj;
		}
		
		void SaveObject (string table, string id, object obj, bool update) {
			byte[] data;
			string sql = "";
			SqliteConnection conn = new SqliteConnection("URI=file:" + dbFile);
			conn.Open();
			
			using (var cmd = conn.CreateCommand())
			{
				using (MemoryStream stream = new MemoryStream()) {
					SerializableObject.Save(obj, stream);
					data = stream.ToArray();
				}
				if (update) {
					sql = String.Format("UPDATE {0} SET Data=@data WHERE id='{1}'", table, id);
				} else {
					sql = String.Format( "INSERT INTO {0} ([ID], [Data]) VALUES('{1}', @data)", table, id);
				}
				cmd.CommandText = sql;
				cmd.Parameters.Add("@data", DbType.Binary, data.Length).Value = data; 
				cmd.ExecuteNonQuery();
			}
			conn.Close();
		}

		byte[] GetBytes(SqliteDataReader reader)
		{
			const int CHUNK_SIZE = 2 * 1024;
			byte[] buffer = new byte[CHUNK_SIZE];
			long bytesRead;
			long fieldOffset = 0;
			using (MemoryStream stream = new MemoryStream())
			{
				while ((bytesRead = reader.GetBytes(0, fieldOffset, buffer, 0, buffer.Length)) > 0)
				{
					byte[] actualRead = new byte[bytesRead];
					Buffer.BlockCopy(buffer, 0, actualRead, 0, (int)bytesRead);
					stream.Write(actualRead, 0, actualRead.Length);
					fieldOffset += bytesRead;
				}
				return stream.ToArray();
			}
		}
	
		void CreateNewDB () {
		    SqliteConnection.CreateFile (dbFile);
			ExecNonQuery("CREATE TABLE projects_desc (ID STRING PRIMARY KEY NOT NULL, Data BLOB)");
			ExecNonQuery("CREATE TABLE projects (ID STRING PRIMARY KEY NOT NULL, Data BLOB)");
		}
	}
}

