using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace WwiseSoundLib
{
	public class WwiseSoundbank
	{
		private List<WwiseSoundbank.SectionMeta> Sections_ = new List<WwiseSoundbank.SectionMeta>();

		private Dictionary<long, long> EmbeddedWemsOffsetTable_ = new Dictionary<long, long>();

		private List<WwiseSoundbank.WwiseObject> WwiseObjects_ = new List<WwiseSoundbank.WwiseObject>();

		private uint STIDUnknown1_;

		public Dictionary<long, byte[]> EmbeddedWems
		{
			get;
			set;
		}

		public Dictionary<long, string> ReferencedSoundbanks
		{
			get;
			set;
		}

		public long Size
		{
			get;
			private set;
		}

		public long SoundbankID
		{
			get;
			set;
		}

		public long SoundbankVersion
		{
			get;
			set;
		}

		public WwiseSoundbank()
		{
			this.EmbeddedWems = new Dictionary<long, byte[]>();
			this.ReferencedSoundbanks = new Dictionary<long, string>();
		}

		public WwiseSoundbank(string filename) : this()
		{
			FileStream fileStream = File.OpenRead(filename);
			this.ParseStream(fileStream);
			fileStream.Close();
		}

		public WwiseSoundbank(Stream stream) : this()
		{
			this.ParseStream(stream);
		}

		private void ParseBKHDSection(BinaryReader dataReader, long endpos)
		{
			this.SoundbankVersion = (long)dataReader.ReadUInt32();
			this.SoundbankID = (long)dataReader.ReadUInt32();
		}

		private void ParseDATASection(BinaryReader dataReader, long endpos)
		{
			long position = dataReader.BaseStream.Position;
			foreach (KeyValuePair<long, long> embeddedWemsOffsetTable_ in this.EmbeddedWemsOffsetTable_)
			{
				long key = embeddedWemsOffsetTable_.Key;
				long value = embeddedWemsOffsetTable_.Value;
				int length = (int)this.EmbeddedWems[key].Length;
				dataReader.BaseStream.Position = position + value;
				this.EmbeddedWems[key] = dataReader.ReadBytes(length);
			}
			dataReader.BaseStream.Position = endpos;
		}

		private void ParseDIDXSection(BinaryReader dataReader, long endpos)
		{
			while (endpos - dataReader.BaseStream.Position >= (long)12)
			{
				uint num = dataReader.ReadUInt32();
				uint num1 = dataReader.ReadUInt32();
				uint num2 = dataReader.ReadUInt32();
				this.EmbeddedWems.Add((long)num, new byte[num2]);
				this.EmbeddedWemsOffsetTable_.Add((long)num, (long)num1);
			}
		}

		private void ParseHIRCSection(BinaryReader dataReader, long endpos)
		{
			int num = Convert.ToInt32(dataReader.ReadUInt32());
			while (dataReader.BaseStream.Position < endpos)
			{
				byte num1 = dataReader.ReadByte();
				int num2 = Convert.ToInt32(dataReader.ReadUInt32());
				long num3 = (long)dataReader.ReadUInt32();
				byte[] numArray = dataReader.ReadBytes(num2 - 4);
				this.WwiseObjects_.Add(new WwiseSoundbank.WwiseObject(num3, (WwiseSoundbank.WwiseObjectType)num1, numArray));
				num--;
			}
			if (num != 0)
			{
				throw new InvalidOperationException("mismatch between wwise objects read and wwise object count");
			}
		}

		private WwiseSoundbank.SectionMeta ParseSection(BinaryReader dataReader)
		{
			string str = Encoding.ASCII.GetString(dataReader.ReadBytes(4));
			uint num = dataReader.ReadUInt32();
			long position = dataReader.BaseStream.Position + (long)num;
            if (str == "BKHD")
			{
				this.ParseBKHDSection(dataReader, position);
			}
			else if (str == "DIDX")
			{
				this.ParseDIDXSection(dataReader, position);
			}
			else if (str == "DATA")
			{
				this.ParseDATASection(dataReader, position);
			}
			else if (str == "HIRC")
			{
				this.ParseHIRCSection(dataReader, position);
			}
			else if (str == "STID")
			{
				this.ParseSTIDSection(dataReader, position);
			}
			else
			{
				this.ParseUnkownSection(dataReader, position);
			}
			if (dataReader.BaseStream.Position > position)
			{
				throw new ArgumentException("section parsing exceeded section bounds.", "stream");
			}
			byte[] numArray = dataReader.ReadBytes(Convert.ToInt32(position - dataReader.BaseStream.Position));
			return new WwiseSoundbank.SectionMeta(str, numArray);
		}

		private void ParseSTIDSection(BinaryReader dataReader, long endpos)
		{
			this.STIDUnknown1_ = dataReader.ReadUInt32();
			uint num = dataReader.ReadUInt32();
            for (int index = 0; (long)index < (long)num; ++index)
            {
				uint num1 = dataReader.ReadUInt32();
				byte num2 = dataReader.ReadByte();
				string str = Encoding.ASCII.GetString(dataReader.ReadBytes((int)num2));
				this.ReferencedSoundbanks.Add((long)num1, str);
			}
		}

		private void ParseStream(Stream stream)
		{
			BinaryReader binaryReader = new BinaryReader(stream);
			while (stream.Length - stream.Position > (long)8)
			{
				WwiseSoundbank.SectionMeta sectionMetum = this.ParseSection(binaryReader);
				this.Sections_.Add(sectionMetum);
			}
		}

		private void ParseUnkownSection(BinaryReader dataReader, long endpos)
		{
		}

		private void WriteBKHDSection(BinaryWriter dataWriter)
		{
			dataWriter.Write((uint)this.SoundbankVersion);
			dataWriter.Write((uint)this.SoundbankID);
		}

		private void WriteDATASection(BinaryWriter dataWriter)
		{
			long position = dataWriter.BaseStream.Position;
			long num = position;
			foreach (KeyValuePair<long, long> embeddedWemsOffsetTable_ in this.EmbeddedWemsOffsetTable_)
			{
				long key = embeddedWemsOffsetTable_.Key;
				long value = embeddedWemsOffsetTable_.Value;
				Stream baseStream = dataWriter.BaseStream;
				baseStream.Position = baseStream.Position + value;
				dataWriter.Write(this.EmbeddedWems[key]);
				if (dataWriter.BaseStream.Position > num)
				{
					num = dataWriter.BaseStream.Position;
				}
				dataWriter.BaseStream.Position = position;
			}
			dataWriter.BaseStream.Position = num;
		}

		private void WriteDIDXSection(BinaryWriter dataWriter)
		{
			foreach (KeyValuePair<long, long> embeddedWemsOffsetTable_ in this.EmbeddedWemsOffsetTable_)
			{
				uint key = (uint)embeddedWemsOffsetTable_.Key;
				uint value = (uint)embeddedWemsOffsetTable_.Value;
				dataWriter.Write(key);
				dataWriter.Write(value);
                dataWriter.Write((uint)this.EmbeddedWems[(long)key].Length);
            }
		}

		public void WriteFile(string filename)
		{
			FileStream fileStream = File.OpenWrite(filename);
			this.WriteStream(fileStream);
			fileStream.Close();
		}

		private void WriteHIRCSection(BinaryWriter dataWriter)
		{
			dataWriter.Write((uint)this.WwiseObjects_.Count);
			foreach (WwiseSoundbank.WwiseObject wwiseObjects_ in this.WwiseObjects_)
			{
				dataWriter.Write((byte)wwiseObjects_.Type);
				dataWriter.Write((uint)wwiseObjects_.Size);
				dataWriter.Write((uint)wwiseObjects_.ID);
				dataWriter.Write(wwiseObjects_.Data);
			}
		}

		private void WriteSection(WwiseSoundbank.SectionMeta section, BinaryWriter dataWriter)
		{
			dataWriter.Write(Encoding.ASCII.GetBytes(section.SectionTag));
			long position = dataWriter.BaseStream.Position;
			dataWriter.Write((uint)0);
			long num = dataWriter.BaseStream.Position;
			string sectionTag = section.SectionTag;
			if (sectionTag == "BKHD")
			{
				this.WriteBKHDSection(dataWriter);
			}
			else if (sectionTag == "DIDX")
			{
				this.WriteDIDXSection(dataWriter);
			}
			else if (sectionTag == "DATA")
			{
				this.WriteDATASection(dataWriter);
			}
			else if (sectionTag == "STID")
			{
				this.WriteSTIDSection(dataWriter);
			}
			else if (sectionTag == "HIRC")
			{
				this.WriteHIRCSection(dataWriter);
			}
			else
			{
				this.WriteUnkownSection(dataWriter);
			}
			dataWriter.Write(section.RemainingData);
			int num1 = Convert.ToInt32(dataWriter.BaseStream.Position - num);
			dataWriter.BaseStream.Position = position;
			dataWriter.Write((uint)num1);
			Stream baseStream = dataWriter.BaseStream;
			baseStream.Position = baseStream.Position + (long)num1;
		}

		private void WriteSTIDSection(BinaryWriter dataWriter)
		{
			dataWriter.Write(this.STIDUnknown1_);
			dataWriter.Write((uint)this.ReferencedSoundbanks.Count);
			foreach (KeyValuePair<long, string> referencedSoundbank in this.ReferencedSoundbanks)
			{
				dataWriter.Write((uint)referencedSoundbank.Key);
				dataWriter.Write((byte)referencedSoundbank.Value.Length);
				dataWriter.Write(Encoding.ASCII.GetBytes(referencedSoundbank.Value));
			}
		}

		private void WriteStream(Stream stream)
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			foreach (WwiseSoundbank.SectionMeta sections_ in this.Sections_)
			{
				this.WriteSection(sections_, binaryWriter);
			}
		}

		private void WriteUnkownSection(BinaryWriter dataWriter)
		{
		}

		private class SectionMeta
		{
			private string SectionTag_;

			public byte[] RemainingData
			{
				get;
				set;
			}

			public string SectionTag
			{
				get
				{
					return this.SectionTag_;
				}
				set
				{
					if (value.Length != 4)
					{
						throw new ArgumentException("section tag has to be four characters long.", "SectionTag");
					}
					this.SectionTag_ = value;
				}
			}

			public SectionMeta(string sectionTag, byte[] remainingData = null)
			{
				this.SectionTag = sectionTag;
				this.RemainingData = remainingData;
			}
		}

		private class WwiseObject
		{
			public byte[] Data
			{
				get;
				set;
			}

			public long ID
			{
				get;
				private set;
			}

			public int Size
			{
				get
				{
					return (int)this.Data.Length + 4;
				}
			}

			public WwiseSoundbank.WwiseObjectType Type
			{
				get;
				private set;
			}

			public WwiseObject(long id, WwiseSoundbank.WwiseObjectType type) : this(id, type, new byte[0])
			{
			}

			public WwiseObject(long id, WwiseSoundbank.WwiseObjectType type, byte[] data)
			{
				this.Type = type;
				this.ID = id;
				this.Data = data;
			}
		}

		private enum WwiseObjectType
		{
			Settings = 1,
			SoundSfxOrVoice = 2,
			EventAction = 3,
			EventArgs = 4,
			SequenceContainer = 5,
			SwitchContainer = 6,
			ActorMixer = 7,
			AudioBus = 8,
			BlendContainer = 9,
			MusicSegment = 10,
			MusicTrack = 11,
			MusicSwitchContainer = 12,
			MusicPlaylistContainer = 13,
			Attenuation = 14,
			DialogueEvent = 15,
			MotionBus = 16,
			MotionFX = 17,
			Effect = 18,
			AuxillaryBus = 19
		}
	}
}