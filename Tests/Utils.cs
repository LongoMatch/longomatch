//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Reflection;
using System.Timers;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Serialization;
using VAS.Core.Store;
using VAS.Core.Store.Templates;

namespace Tests
{
	public static class Utils
	{
		static bool debugLine = false;

		//Check for specific LongoMatch objects
		public static T SerializeDeserialize<T> (T obj)
		{
			var stream = new MemoryStream ();
			Serializer.Instance.Save (obj, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			if (debugLine) {
				var jsonString = new StreamReader (stream).ReadToEnd ();
				Console.WriteLine (jsonString);
			}
			stream.Seek (0, SeekOrigin.Begin);

			return Serializer.Instance.Load<T> (stream, SerializationType.Json);
		}

		//Check for specific LongoMatch objects
		public static void CheckSerialization<T> (T obj, bool ignoreIsChanged = false)
		{
			List<IStorable> children, changed;

			if (!ignoreIsChanged) {
				Assert.IsInstanceOf<IChanged> (obj);
			}
			var stream = new MemoryStream ();
			Serializer.Instance.Save (obj, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			var jsonString = new StreamReader (stream).ReadToEnd ();
			if (debugLine) {
				Console.WriteLine (jsonString);
			}
			stream.Seek (0, SeekOrigin.Begin);

			var newobj = Serializer.Instance.Load<T> (stream, SerializationType.Json);
			if (!ignoreIsChanged) {
				ObjectChangedParser parser = new ObjectChangedParser ();
				if (obj is IStorable) {
					StorableNode parentNode;
					Assert.IsTrue (parser.ParseInternal (out parentNode, newobj as IStorable, Serializer.JsonSettings));
					Assert.IsFalse (parentNode.HasChanges ());
				} else {
					Assert.IsFalse ((newobj as IChanged).IsChanged);
				}
			}

			stream = new MemoryStream ();
			Serializer.Instance.Save (newobj, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			var newJsonString = new StreamReader (stream).ReadToEnd ();
			if (debugLine) {
				Console.WriteLine (newJsonString);
			}
			Assert.AreEqual (jsonString, newJsonString);
		}

		//keep this test, Assembly is different from VAS
		public static Image LoadImageFromFile (bool scaled = false)
		{
			Image img = null;
			string tmpFile = Path.GetTempFileName ();

			using (Stream resource = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("vas-dibujo.svg")) {
				using (Stream output = File.OpenWrite (tmpFile)) {
					resource.CopyTo (output);
				}
			}
			try {
				if (!scaled) {
					img = new Image (tmpFile);
				} else {
					img = new Image (tmpFile);
					img.ScaleInplace (20, 20);
				}
			} catch (Exception ex) {
				Assert.Fail (ex.Message);
			} finally {
				File.Delete (tmpFile);
			}
			return img;
		}

		//Specific LongoMatch objects
		public static LMProject CreateProject (bool withEvents = true)
		{
			LMTimelineEvent pl;
			LMProject p = new LMProject ();
			p.Dashboard = LMDashboard.DefaultTemplate (10);
			p.LocalTeamTemplate = LMTeam.DefaultTemplate (5);
			p.LocalTeamTemplate.FormationStr = "2-2";
			p.VisitorTeamTemplate = LMTeam.DefaultTemplate (5);
			p.VisitorTeamTemplate.FormationStr = "2-2";
			var pd = new ProjectDescription ();
			pd.FileSet = new MediaFileSet ();
			pd.FileSet.Add (new MediaFile (Path.GetTempFileName (), 34000, 25, true, true, "mp4", "h264",
				"aac", 320, 240, 1.3, null, "Test asset 1"));
			pd.FileSet.Add (new MediaFile (Path.GetTempFileName (), 34000, 25, true, true, "mp4", "h264",
				"aac", 320, 240, 1.3, null, "Test asset 2"));
			p.Description = pd;
			p.UpdateEventTypesAndTimers ();

			if (withEvents) {
				AnalysisEventButton b = p.Dashboard.List [0] as AnalysisEventButton;

				/* No tags, no players */
				pl = new LMTimelineEvent {
					EventType = b.EventType,
					Start = new Time (0),
					Stop = new Time (100),
					FileSet = pd.FileSet
				};
				p.Timeline.Add (pl);
				/* tags, but no players */
				b = p.Dashboard.List [1] as AnalysisEventButton;
				pl = new LMTimelineEvent {
					EventType = b.EventType,
					Start = new Time (0),
					Stop = new Time (100),
					FileSet = pd.FileSet
				};
				pl.Tags.Add (b.AnalysisEventType.Tags [0]);
				p.Timeline.Add (pl);
				/* tags and players */
				b = p.Dashboard.List [2] as AnalysisEventButton;
				pl = new LMTimelineEvent {
					EventType = b.EventType,
					Start = new Time (0),
					Stop = new Time (100),
					FileSet = pd.FileSet
				};
				pl.Tags.Add (b.AnalysisEventType.Tags [1]);
				pl.Players.Add (p.LocalTeamTemplate.List [0]);
				p.Timeline.Add (pl);
			}

			return p;
		}

		//Specific LongoMatch objects
		public static void DeleteProject (LMProject p)
		{
			foreach (MediaFile mf in p.Description.FileSet) {
				if (File.Exists (mf.FilePath)) {
					File.Delete (mf.FilePath);
				}
			}
		}

		//Check for specific LongoMatch objects
		public static void AreEquals (IStorable obj1, IStorable obj2, bool areEquals = true)
		{
			var stream = new MemoryStream ();
			Serializer.Instance.Save (obj1, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			var obj1Str = new StreamReader (stream).ReadToEnd ();
			stream = new MemoryStream ();
			Serializer.Instance.Save (obj2, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			var obj2Str = new StreamReader (stream).ReadToEnd ();
			if (areEquals) {
				Assert.AreEqual (obj1Str, obj2Str);
			} else {
				Assert.AreNotEqual (obj1Str, obj2Str);
			}
		}

		//keep this test, Assembly is different from VAS
		public static string SaveResource (string name, string path)
		{
			string filePath;
			var assembly = Assembly.GetExecutingAssembly ();
			using (Stream inS = assembly.GetManifestResourceStream (name)) {
				filePath = Path.Combine (path, name);
				using (Stream outS = new FileStream (filePath, FileMode.Create)) {
					inS.CopyTo (outS);
				}
			}
			return filePath;
		}

		public class PlayerDummy : Player
		{
			//dummy class for abstract validation. Copied from LongoMatch and adapted to VAS.
		}

		public interface IDummyCapturerPanel : IPanel
		{
			ICapturerBin Capturer { get; set; }
		}
	}
}

