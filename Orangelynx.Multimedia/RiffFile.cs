using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Orangelynx.Multimedia
{
	public class RiffFile
	{
		protected RiffChunk RootChunk_;

		public RiffChunk Root
		{
			get
			{
				return this.RootChunk_;
			}
		}

		public long Size
		{
			get
			{
				return this.RootChunk_.Size + (long)8;
			}
		}

		public string Type
		{
			get
			{
				return Encoding.ASCII.GetString(this.RootChunk_.Data);
			}
		}

		public RiffFile(Stream inStream)
		{
			this.ParseChunk(inStream, out this.RootChunk_);
		}

		public RiffFile(string filename)
		{
			this.LoadFile(filename);
		}

		private void LoadFile(string filename)
		{
			using (FileStream fileStream = File.OpenRead(filename))
			{
				this.ParseChunk(fileStream, out this.RootChunk_);
			}
		}

		private void ParseChunk(Stream input, out RiffChunk chunk)
		{
			BinaryReader binaryReader = new BinaryReader(input);
			if (input.Length - input.Position < (long)8)
			{
				throw new ArgumentException("not a valid RIFF chunk - too short", "input");
			}
			string str = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
			long num = (long)binaryReader.ReadUInt32();
			chunk = new RiffChunk(str);
			if (str == "LIST" || str == "RIFF")
			{
				chunk.Data = binaryReader.ReadBytes(4);
				this.ParseSubchunks(input, chunk, num - (long)4);
			}
			else
			{
				chunk.Data = binaryReader.ReadBytes(Convert.ToInt32(num));
			}
			if (num % (long)2 != 0)
			{
				binaryReader.ReadByte();
			}
		}

		private void ParseSubchunks(Stream input, RiffChunk chunk, long size)
		{
			RiffChunk riffChunk;
			long position = input.Position + size;
			while (position - input.Position > (long)8)
			{
				this.ParseChunk(input, out riffChunk);
				chunk.AddSubchunk(riffChunk);
			}
			input.Position = position;
		}

		private void WriteChunk(RiffChunk chunk, Stream stream)
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(Encoding.ASCII.GetBytes(chunk.ID));
			binaryWriter.Write((uint)chunk.Size);
			binaryWriter.Write(chunk.Data);
			foreach (RiffChunk subchunk in chunk.Subchunks)
			{
				this.WriteChunk(subchunk, stream);
			}
			if (chunk.Size % (long)2 != 0)
			{
				binaryWriter.Write((byte)0);
			}
		}

		public void WriteFile(string filename)
		{
			FileStream fileStream = File.Create(filename);
			this.WriteChunk(this.RootChunk_, fileStream);
			fileStream.Close();
		}
	}
}