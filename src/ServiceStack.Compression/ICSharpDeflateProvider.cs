﻿using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ServiceStack.Caching;

namespace ServiceStack.Compression
{
	public class ICSharpDeflateProvider
		: IDeflateProvider
	{
		public byte[] Deflate(string text)
		{
			var buffer = Encoding.UTF8.GetBytes(text);
			using (var ms = new MemoryStream())
			{
				using (var zipStream = new DeflaterOutputStream(ms, new Deflater(9)))
				{
					zipStream.Write(buffer, 0, buffer.Length);
					zipStream.Close();

					var compressed = ms.ToArray();

					var gzBuffer = new byte[compressed.Length + 4];
					Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
					Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);

					return gzBuffer;
				}
			}
		}

		public string Inflate(byte[] gzBuffer)
		{
			using (var ms = new MemoryStream())
			{
				var msgLength = BitConverter.ToInt32(gzBuffer, 0);
				ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

				var buffer = new byte[msgLength];

				ms.Position = 0;
				using (var zipStream = new InflaterInputStream(ms, new Inflater()))
				{
					zipStream.Read(buffer, 0, buffer.Length);
				}

				return Encoding.UTF8.GetString(buffer);
			}
		}
	}
}
