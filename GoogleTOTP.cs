using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.Drawing;

namespace GoogleAuth
{
	class GoogleTOTP
	{
		RNGCryptoServiceProvider rnd;
		protected string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

		private int intervalLength;
		private int pinCodeLength;
		private int pinModulo;

		private byte[] randomBytes = new byte[10];

		public GoogleTOTP()
		{
			rnd = new RNGCryptoServiceProvider();

			pinCodeLength = 6;
			intervalLength = 30;
			pinModulo = (int)Math.Pow(10, pinCodeLength);

			rnd.GetBytes(randomBytes);
		}

		public byte[] getPrivateKey()
		{
			return randomBytes;
		}

		/// <summary>
		/// Generates a PIN of desired length when given a challenge (counter)
		/// </summary>
		/// <param name="challenge">Counter to calculate hash</param>
		/// <returns>Desired length PIN</returns>
		private String generateResponseCode(long challenge, byte[] randomBytes)
		{
			HMACSHA1 myHmac = new HMACSHA1(randomBytes);
			myHmac.Initialize();

			byte[] value = BitConverter.GetBytes(challenge);
			Array.Reverse(value); //reverses the challenge array due to differences in c# vs java
			myHmac.ComputeHash(value);
			byte[] hash = myHmac.Hash;
			int offset = hash[hash.Length - 1] & 0xF;
			byte[] SelectedFourBytes = new byte[4];
			//selected bytes are actually reversed due to c# again, thus the weird stuff here
			SelectedFourBytes[0] = hash[offset];
			SelectedFourBytes[1] = hash[offset + 1];
			SelectedFourBytes[2] = hash[offset + 2];
			SelectedFourBytes[3] = hash[offset + 3];
			Array.Reverse(SelectedFourBytes);
			int finalInt = BitConverter.ToInt32(SelectedFourBytes, 0);
			int truncatedHash = finalInt & 0x7FFFFFFF; //remove the most significant bit for interoperability as per HMAC standards
			int pinValue = truncatedHash % pinModulo; //generate 10^d digits where d is the number of digits
			return padOutput(pinValue);
		}

		/// <summary>
		/// Gets current interval number since Unix Epoch based on given interval length
		/// </summary>
		/// <returns>Current interval number</returns>
		public long getCurrentInterval()
		{
			TimeSpan TS = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			long currentTimeSeconds = (long)Math.Floor(TS.TotalSeconds);
			long currentInterval = currentTimeSeconds / intervalLength; // 30 Seconds
			return currentInterval;
		}

		/// <summary>
		/// Pads the output string with leading zeroes just in case the result is less than the length of desired digits
		/// </summary>
		/// <param name="value">Value to pad</param>
		/// <returns>Padded Result</returns>
		private String padOutput(int value)
		{
			String result = value.ToString();
			for (int i = result.Length; i < pinCodeLength; i++)
			{
				result = "0" + result;
			}
			return result;
		}

		/// <summary>
		/// This is a different Url Encode implementation since the default .NET one outputs the percent encoding in lower case.
		/// While this is not a problem with the percent encoding spec, it is used in upper case throughout OAuth
		/// </summary>
		/// <param name="value">The value to Url encode</param>
		/// <returns>Returns a Url encoded string</returns>
		protected string UrlEncode(string value)
		{
			StringBuilder result = new StringBuilder();

			foreach (char symbol in value)
			{
				if (unreservedChars.IndexOf(symbol) != -1)
				{
					result.Append(symbol);
				}
				else
				{
					result.Append('%' + String.Format("{0:X2}", (int)symbol));
				}
			}

			return result.ToString();
		}

		public Image GenerateImage(int width, int height, string email)
		{
			string randomString = CreativeCommons.Transcoder.Base32Encode(randomBytes);
			string ProvisionUrl = UrlEncode(String.Format("otpauth://totp/{0}?secret={1}", email, randomString));
			string url = String.Format("http://chart.apis.google.com/chart?cht=qr&chs={0}x{1}&chl={2}", width, height, ProvisionUrl);

			WebClient wc = new WebClient();
			var data = wc.DownloadData(url);

			using (var imageStream = new MemoryStream(data))
			{
				return new Bitmap(imageStream);
			}
		}

		public string GeneratePin()
		{
			return generateResponseCode(getCurrentInterval(), randomBytes);
		}

	}
}
