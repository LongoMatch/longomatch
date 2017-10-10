//
//  Copyright (C) 2017 ${CopyrightHolder}
using System;
using System.IO;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using VAS.Core.Common;
using VAS.DB;

namespace LongoMatch.DB
{
	/// <summary>
	/// Do a backup of the available databases in the DBDir defined
	/// </summary>
	public static class DatabasesBackup
	{
		/// <summary>
		/// Do a backup of the available databases
		/// </summary>
		public static void Backup ()
		{
			VFS.SetCurrent (new FileSystem ());
			// Do a backup of databases in version 1 to allow the usage of an open source version
			InternalBackup (Path.Combine (App.Current.DBDir, "templates"), "templates");
			foreach (var file in Directory.EnumerateFiles (App.Current.DBDir)) {
				if (file.EndsWith (".cblite")) {
					InternalBackup (App.Current.DBDir, Path.GetFileNameWithoutExtension(file));
				}
			}
		}

		static void InternalBackup (string dbPath, string dbName)
		{
			var backup = Path.Combine (App.Current.DBDir, "backup");
			try {
				if (!Directory.Exists (backup)) {
					Directory.CreateDirectory (backup);
				}

				string outputFilename = Path.Combine (backup, dbName + ".tar.gz");
				if (File.Exists (outputFilename)) {
					File.Delete (outputFilename);
				}

				// storage backup in old format
				using (FileStream fs = new FileStream (outputFilename, FileMode.Create, FileAccess.Write, FileShare.None)) {
					using (Stream gzipStream = new GZipOutputStream (fs)) {
						using (TarArchive tarArchive = TarArchive.CreateOutputTarArchive (gzipStream)) {
							foreach (string n in new string [] { "", "-wal", "-shm" }) {
								TarEntry tarEntry = TarEntry.CreateEntryFromFile (Path.Combine (dbPath, dbName + ".cblite" + n));
								tarArchive.WriteEntry (tarEntry, true);
							}
							AddDirectoryFilesToTar (tarArchive, Path.Combine (dbPath, dbName + " attachments"), true);
						}
					}
				}
			} catch (Exception ex) {
				Log.Exception (ex);
			}
		}

		static void AddDirectoryFilesToTar (TarArchive tarArchive, string sourceDirectory, bool recurse)
		{
			// Recursively add sub-folders
			if (recurse) {
				string [] directories = Directory.GetDirectories (sourceDirectory);
				foreach (string directory in directories)
					AddDirectoryFilesToTar (tarArchive, directory, recurse);
			}

			// Add files
			string [] filenames = Directory.GetFiles (sourceDirectory);
			foreach (string filename in filenames) {
				TarEntry tarEntry = TarEntry.CreateEntryFromFile (filename);
				tarArchive.WriteEntry (tarEntry, true);
			}
		}
	}
}
