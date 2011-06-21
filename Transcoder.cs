/***************************************************************************
 *  Transcoder.cs
 *
 *  cc-sharp is a library to verify Creative Commons license metadata.
 *  Copyright (C) 2006 Luke Hoersten
 *  Written by Luke Hoersten <luke.hoersten@gmail.com>
 *
 *  The "Encode" method is based on Java code by Robert Kaye and Gordon Mohr
 *  Public Domain (PD) 2006 The Bitzi Corporation (http://bitzi.com/publicdomain)
 *  (RFC http://www.faqs.org/rfcs/rfc3548.html)
 ****************************************************************************/
/*  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */
using System.Text;

namespace CreativeCommons
{
	public class Transcoder
	{
		private const int IN_BYTE_SIZE = 8;
		private const int OUT_BYTE_SIZE = 5;
		private static char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

		public static string Base32Encode(byte[] data)
		{
			int i = 0, index = 0, digit = 0;
			int current_byte, next_byte;
			StringBuilder result = new StringBuilder((data.Length + 7) * IN_BYTE_SIZE / OUT_BYTE_SIZE);

			while (i < data.Length)
			{
				current_byte = (data[i] >= 0) ? data[i] : (data[i] + 256); // Unsign

				/* Is the current digit going to span a byte boundary? */
				if (index > (IN_BYTE_SIZE - OUT_BYTE_SIZE))
				{
					if ((i + 1) < data.Length)
						next_byte = (data[i + 1] >= 0) ? data[i + 1] : (data[i + 1] + 256);
					else
						next_byte = 0;

					digit = current_byte & (0xFF >> index);
					index = (index + OUT_BYTE_SIZE) % IN_BYTE_SIZE;
					digit <<= index;
					digit |= next_byte >> (IN_BYTE_SIZE - index);
					i++;
				}
				else
				{
					digit = (current_byte >> (IN_BYTE_SIZE - (index + OUT_BYTE_SIZE))) & 0x1F;
					index = (index + OUT_BYTE_SIZE) % IN_BYTE_SIZE;
					if (index == 0)
						i++;
				}
				result.Append(alphabet[digit]);
			}

			return result.ToString();
		}
	}
}
