//
//  Copyright (C) 2016 Fluendo S.A.
//
//
using System;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{
	[Serializable]
	public class EncryptedProject
	{
		readonly byte[] encryptedKey;
		readonly byte[] iv;
		readonly byte[] data;

		public EncryptedProject (byte[] key, byte[] iv, byte[] data)
		{
			this.encryptedKey = key;
			this.iv = iv;
			this.data = data;
		}

		public byte[] EncryptedKey {
			get {
				return encryptedKey;
			}
		}

		public byte[] Iv {
			get {
				return iv;
			}
		}

		public byte[] Data {
			get {
				return data;
			}
		}
	}
}
