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

namespace LongoMatch.Core.Common
{
	[Serializable]
	/// <summary>
	/// Represents an encrypted object using a digital "envelope".
	/// The data is encrypted with a randomly generated AES key and a random IV.
	/// The AES key is encrypted with a public key using the asymetric RSA algorithm.
	/// The recipient decrypts the AES key using the private RSA key and decrypts the data
	/// with the key and the IV
	/// </summary>
	public class EncryptedObject
	{
		public EncryptedObject (byte[] key, byte[] iv, byte[] data)
		{
			Key = key;
			IV = iv;
			Data = data;
		}

		/// <summary>
		/// Encrypted AES key used to encrypt the data
		/// </summary>
		public byte[] Key {
			get;
			set;
		}

		/// <summary>
		/// Initial Vector used to encrypt the data.
		/// </summary>
		public byte[] IV {
			get;
			set;
		}


		/// <summary>
		/// AES encrypted data.
		/// </summary>
		public byte[] Data {
			get;
			set;
		}
	}
}

