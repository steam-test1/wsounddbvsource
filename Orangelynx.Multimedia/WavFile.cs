using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Orangelynx.Multimedia
{
	public class WavFile
	{
		private RiffFile RiffFile_;

		private Dictionary<string, WavFile.ChunkParser> ChunkParsers_;

		private Dictionary<string, WavFile.ChunkUpdater> ChunkUpdaters_;

		public byte[] AudioData
		{
			get;
			set;
		}

		public uint AvgBytesPerSec
		{
			get;
			set;
		}

		public ushort BitsPerSample
		{
			get;
			set;
		}

		public ushort BlockAlign
		{
			get;
			set;
		}

		public ushort Channels
		{
			get;
			set;
		}

		public byte[] FormatExtension
		{
			get;
			set;
		}

		public ushort FormatExtensionSize
		{
			get
			{
				return Convert.ToUInt16((int)this.FormatExtension.Length);
			}
		}

		public ushort FormatTag
		{
			get;
			set;
		}

		public uint SamplesPerSec
		{
			get;
			set;
		}

		private WavFile()
		{
		}

		public WavFile(Stream inStream) : this()
		{
			this.ParseStream(inStream);
		}

		public WavFile(string filename) : this()
		{
			this.LoadFile(filename);
		}

		private void LoadFile(string filename)
		{
			using (FileStream fileStream = File.OpenRead(filename))
			{
				this.ParseStream(fileStream);
			}
		}

		private void ParseDataChunk(RiffChunk chunk)
		{
			this.AudioData = chunk.Data;
		}

		private void ParseFormatChunk(RiffChunk chunk)
		{
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(chunk.Data));
			this.FormatTag = binaryReader.ReadUInt16();
			this.Channels = binaryReader.ReadUInt16();
			this.SamplesPerSec = binaryReader.ReadUInt32();
			this.AvgBytesPerSec = binaryReader.ReadUInt32();
			this.BlockAlign = binaryReader.ReadUInt16();
			if (binaryReader.BaseStream.Length - binaryReader.BaseStream.Position > (long)0)
			{
				this.BitsPerSample = binaryReader.ReadUInt16();
			}
			if (binaryReader.BaseStream.Length - binaryReader.BaseStream.Position > (long)0)
			{
				this.FormatExtension = binaryReader.ReadBytes((int)binaryReader.ReadUInt16());
			}
			binaryReader.Close();
		}

		private void ParseStream(Stream stream)
		{
			this.RiffFile_ = new RiffFile(stream);
			foreach (RiffChunk subchunk in this.RiffFile_.Root.Subchunks)
			{
				string d = subchunk.ID;
				if (d == "fmt ")
				{
					this.ParseFormatChunk(subchunk);
				}
				else if (d == "data")
				{
					this.ParseDataChunk(subchunk);
				}
			}
		}

		private void UpdateDataChunk(RiffChunk chunk)
		{
			chunk.Data = this.AudioData;
		}

		private void UpdateFormatChunk(RiffChunk chunk)
		{
			using (MemoryStream memoryStream = new MemoryStream(22))
			{
				BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write(this.FormatTag);
				binaryWriter.Write(this.Channels);
				binaryWriter.Write(this.SamplesPerSec);
				binaryWriter.Write(this.AvgBytesPerSec);
				binaryWriter.Write(this.BlockAlign);
				binaryWriter.Write(this.BitsPerSample);
				if (this.FormatExtension == null)
				{
					binaryWriter.Write((ushort)0);
				}
				else
				{
					binaryWriter.Write((ushort)((int)this.FormatExtension.Length));
					binaryWriter.Write(this.FormatExtension);
				}
				chunk.Data = memoryStream.ToArray();
			}
		}

		public void WriteFile(string filename)
		{
			foreach (RiffChunk subchunk in this.RiffFile_.Root.Subchunks)
			{
				string d = subchunk.ID;
				if (d == "fmt ")
				{
					this.UpdateFormatChunk(subchunk);
				}
				else if (d == "data")
				{
					this.UpdateDataChunk(subchunk);
				}
			}
			this.RiffFile_.WriteFile(filename);
		}

		private delegate void ChunkParser(RiffChunk chunk);

		private delegate void ChunkUpdater(RiffChunk chunk);

		public enum WAVEFormatCategories
		{
			PCM = 1,
			MS_ADPCM = 2,
			IEEE_FLOAT = 3,
			A_LAW = 6,
			MU_LAW = 7
		}
	}
}