//
//  Copyright (C) 2016 Fluendo S.A.
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
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.VirtualFileSystem;
using VFileAttributes = ICSharpCode.SharpZipLib.VirtualFileSystem.FileAttributes;
using System.IO;

namespace LongoMatch.DB
{
	class FileSystem : IVirtualFileSystem
	{
		class ElementInfo : IVfsElement
		{
			protected FileSystemInfo Info;

			public ElementInfo (FileSystemInfo info)
			{
				Info = info;
			}

			public string Name {
				get { return Info.Name; }
			}

			public bool Exists {
				get { return Info.Exists; }
			}

			public VFileAttributes Attributes {
				get {
					VFileAttributes attrs = 0;
					if (Info.Attributes.HasFlag (System.IO.FileAttributes.Normal))
						attrs |= VFileAttributes.Normal;
					if (Info.Attributes.HasFlag (System.IO.FileAttributes.ReadOnly))
						attrs |= VFileAttributes.ReadOnly;
					if (Info.Attributes.HasFlag (System.IO.FileAttributes.Hidden))
						attrs |= VFileAttributes.Hidden;
					if (Info.Attributes.HasFlag (System.IO.FileAttributes.Directory))
						attrs |= VFileAttributes.Directory;
					if (Info.Attributes.HasFlag (System.IO.FileAttributes.Archive))
						attrs |= VFileAttributes.Archive;

					return attrs;
				}
			}

			public DateTime CreationTime {
				get { return Info.CreationTime; }
			}

			public DateTime LastAccessTime {
				get { return Info.LastAccessTime; }
			}

			public DateTime LastWriteTime {
				get { return Info.LastWriteTime; }
			}
		}

		class DirInfo : ElementInfo, IDirectoryInfo
		{
			public DirInfo (DirectoryInfo dInfo)
				: base (dInfo)
			{
			}
		}

		class FilInfo : ElementInfo, IFileInfo
		{
			protected FileInfo FInfo { get { return (FileInfo)Info; } }

			public FilInfo (FileInfo fInfo)
				: base (fInfo)
			{
			}

			public long Length {
				get { return FInfo.Length; }
			}
		}

		public System.Collections.Generic.IEnumerable<string> GetFiles (string directory)
		{
			return Directory.GetFiles (directory);
		}

		public System.Collections.Generic.IEnumerable<string> GetDirectories (string directory)
		{
			return Directory.GetDirectories (directory);
		}

		public string GetFullPath (string path)
		{
			return Path.GetFullPath (path);
		}

		public IDirectoryInfo GetDirectoryInfo (string directoryName)
		{
			return new DirInfo (new DirectoryInfo (directoryName));
		}

		public IFileInfo GetFileInfo (string filename)
		{
			return new FilInfo (new FileInfo (filename));
		}

		public void SetLastWriteTime (string name, DateTime dateTime)
		{
			File.SetLastWriteTime (name, dateTime);
		}

		public void SetAttributes (string name, VFileAttributes attributes)
		{
			System.IO.FileAttributes attrs = 0;
			if (attributes.HasFlag (VFileAttributes.Normal))
				attrs |= System.IO.FileAttributes.Normal;
			if (attributes.HasFlag (VFileAttributes.ReadOnly))
				attrs |= System.IO.FileAttributes.ReadOnly;
			if (attributes.HasFlag (VFileAttributes.Hidden))
				attrs |= System.IO.FileAttributes.Hidden;
			if (attributes.HasFlag (VFileAttributes.Directory))
				attrs |= System.IO.FileAttributes.Directory;
			if (attributes.HasFlag (VFileAttributes.Archive))
				attrs |= System.IO.FileAttributes.Archive;
			File.SetAttributes (name, attrs);
		}

		public void CreateDirectory (string directory)
		{
			Directory.CreateDirectory (directory);
		}

		public string GetTempFileName ()
		{
			return Path.GetTempFileName ();
		}

		public void CopyFile (string fromFileName, string toFileName, bool overwrite)
		{
			File.Copy (fromFileName, toFileName, overwrite);
		}

		public void MoveFile (string fromFileName, string toFileName)
		{
			File.Move (fromFileName, toFileName);
		}

		public void DeleteFile (string fileName)
		{
			File.Delete (fileName);
		}

		public VfsStream CreateFile (string filename)
		{
			return new VfsProxyStream (new FileStream (filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read), filename);
		}

		public VfsStream OpenReadFile (string filename)
		{
			return new VfsProxyStream (new FileStream (filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), filename);
		}

		public VfsStream OpenWriteFile (string filename)
		{
			return new VfsProxyStream (File.OpenWrite (filename), filename);
		}

		public string CurrentDirectory {
			get { return Environment.CurrentDirectory; }
		}

		public char DirectorySeparatorChar {
			get { return Path.DirectorySeparatorChar; }
		}
	}
}
