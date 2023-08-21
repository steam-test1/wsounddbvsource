using System;
using System.IO;

namespace Orangelynx.Multimedia
{
	public class ImaAdpcmCodec
	{
		private const int PcmSampleSize = 16;

		private const int AdpcmSampleSize = 4;

		private readonly static int[] NextStepIndexTable;

		private readonly static int[] StepTable;

		static ImaAdpcmCodec()
		{
			ImaAdpcmCodec.NextStepIndexTable = new int[] { -1, -1, -1, -1, 2, 4, 6, 8, -1, -1, -1, -1, 2, 4, 6, 8 };
			ImaAdpcmCodec.StepTable = new int[] { 7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 19, 21, 23, 25, 28, 31, 34, 37, 41, 45, 50, 55, 60, 66, 73, 80, 88, 97, 107, 118, 130, 143, 157, 173, 190, 209, 230, 253, 279, 307, 337, 371, 408, 449, 494, 544, 598, 658, 724, 796, 876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066, 2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358, 5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899, 15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767 };
		}

		public ImaAdpcmCodec()
		{
		}

		private static byte[] DecodeBlock(byte[] adpcmBlock, int channelCount)
		{
			if (channelCount > 2 || channelCount < 1)
			{
				throw new InvalidOperationException("input data has to be mono or stereo.");
			}
			uint length = (uint)(((int)adpcmBlock.Length - 4) * 2);
			byte[] numArray = new byte[length * 2];
			BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(numArray));
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(adpcmBlock));
			short num = binaryReader.ReadInt16();
			int num1 = binaryReader.ReadByte();
			binaryReader.ReadByte();
			binaryWriter.Write(num);
			short num2 = num;
			int num3 = num1;
			for (int i = 2; i < length; i += 2)
			{
				byte num4 = binaryReader.ReadByte();
				num2 = ImaAdpcmCodec.DecodeSample((byte)(num4 & 15), num2, ImaAdpcmCodec.StepTable[num3]);
				num3 = ImaAdpcmCodec.NextStepIndex(num3, num4 & 15);
				binaryWriter.Write(num2);
				num2 = ImaAdpcmCodec.DecodeSample((byte)(num4 >> 4), num2, ImaAdpcmCodec.StepTable[num3]);
				num3 = ImaAdpcmCodec.NextStepIndex(num3, num4 >> 4);
				binaryWriter.Write(num2);
			}
			num2 = ImaAdpcmCodec.DecodeSample((byte)(binaryReader.ReadByte() & 15), num2, ImaAdpcmCodec.StepTable[num3]);
			binaryWriter.Write(num2);
			binaryReader.Close();
			binaryWriter.Close();
			return numArray;
		}

		private static short DecodeSample(byte sample, short prevSample, int step)
		{
			int num = step >> 3;
			if ((sample & 1) != 0)
			{
				num = num + (step >> 2);
			}
			if ((sample & 2) != 0)
			{
				num = num + (step >> 1);
			}
			if ((sample & 4) != 0)
			{
				num += step;
			}
			if ((sample & 8) != 0)
			{
				num = -num;
			}
			int num1 = prevSample + num;
			if (num1 > 32767)
			{
				num1 = 32767;
			}
			else if (num1 < -32768)
			{
				num1 = -32768;
			}
			return Convert.ToInt16(num1);
		}

		public static byte[] DecodeStream(byte[] adpcmData, int channelCount, int blockAlignment)
		{
			if (channelCount > 2 || channelCount < 1)
			{
				throw new InvalidOperationException("input data has to be mono or stereo.");
			}
			if (blockAlignment < 32 || blockAlignment > 512)
			{
				throw new ArgumentOutOfRangeException("blockAlignment has to be between 32 and 512 bytes.");
			}
			uint num = (uint)((blockAlignment - 4) * 2);
			byte[] numArray = new byte[(long)(adpcmData.Length / blockAlignment) * (long)num * 2L];
            MemoryStream memoryStream = new MemoryStream(adpcmData);
			MemoryStream memoryStream1 = new MemoryStream(numArray);
			byte[] numArray1 = new byte[blockAlignment];
			while (memoryStream1.Position < memoryStream1.Length)
			{
				memoryStream.Read(numArray1, 0, blockAlignment);
				byte[] numArray2 = ImaAdpcmCodec.DecodeBlock(numArray1, channelCount);
				memoryStream1.Write(numArray2, 0, (int)numArray2.Length);
			}
			memoryStream.Close();
			memoryStream1.Close();
			return numArray;
		}

		private static byte[] EncodeBlock(byte[] pcmBlock, int seedStepIndex, out int nextStepIndex, int channelCount)
		{
			if (channelCount > 2 || channelCount < 1)
			{
				throw new InvalidOperationException("input data has to be mono or stereo.");
			}
			if (seedStepIndex > 88 || seedStepIndex < 0)
			{
				throw new ArgumentOutOfRangeException("Seed Step Index has to be in the interval [0,88].");
			}
			uint length = (uint)((double)((int)pcmBlock.Length) / 2);
			byte[] numArray = new byte[(uint)(4 + (double)((float)length) * 0.5)];
			BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(numArray));
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(pcmBlock));
			short num = binaryReader.ReadInt16();
			binaryWriter.Write(num);
			binaryWriter.Write(Convert.ToByte(seedStepIndex));
			binaryWriter.Write((byte)0);
			int num1 = seedStepIndex;
			short num2 = num;
			short num3 = 0;
			byte num4 = 0;
			for (int i = 2; i < 64; i += 2)
			{
				num3 = binaryReader.ReadInt16();
				num4 = ImaAdpcmCodec.EncodeSample(num3, num2, ImaAdpcmCodec.StepTable[num1], out num2);
				num1 = ImaAdpcmCodec.NextStepIndex(num1, num4 & 15);
				num3 = binaryReader.ReadInt16();
				num4 = (byte)(num4 | (byte)(ImaAdpcmCodec.EncodeSample(num3, num2, ImaAdpcmCodec.StepTable[num1], out num2) << 4));
				num1 = ImaAdpcmCodec.NextStepIndex(num1, num4 >> 4);
				binaryWriter.Write(num4);
			}
			num3 = binaryReader.ReadInt16();
			num4 = ImaAdpcmCodec.EncodeSample(num3, num2, ImaAdpcmCodec.StepTable[num1], out num2);
			num1 = ImaAdpcmCodec.NextStepIndex(num1, num4 & 15);
			binaryWriter.Write(num4);
			nextStepIndex = num1;
			binaryWriter.Close();
			binaryReader.Close();
			return numArray;
		}

		private static byte EncodeSample(short sample, short predSample, int step, out short newPredSample)
		{
			byte num = 0;
			int num1 = (int)sample - (int)predSample;
            int num2 = 0;
			int num3 = Math.Abs((int)num1);
			if (num1 < 0)
			{
				num = (byte)(num | 8);
			}
			if (num3 >= step)
			{
				num = (byte)(num | 4);
				num3 -= step;
			}
			if (num3 >= step >> 1)
			{
				num = (byte)(num | 2);
				num3 = num3 - (step >> 1);
			}
			if (num3 >= step >> 2)
			{
				num = (byte)(num | 1);
				num3 = num3 - (step >> 2);
			}
			num2 = (num1 >= 0 ? sample - num3 + (step >> 3) : sample + num3 - (step >> 3));
			if (num2 > 32767)
			{
				newPredSample = 32767;
			}
			else if (num2 >= -32768)
			{
				newPredSample = Convert.ToInt16(num2);
			}
			else
			{
				newPredSample = -32768;
			}
			return num;
		}

		public static byte[] EncodeStream(byte[] pcmData, int channelCount, int blockAlignment)
		{
			if (channelCount > 2 || channelCount < 1)
			{
				throw new InvalidOperationException("input data has to be mono or stereo.");
			}
			if (blockAlignment < 32 || blockAlignment > 512)
			{
				throw new ArgumentOutOfRangeException("blockAlignment has to be between 32 and 512 bytes.");
			}
			uint num = (uint)((blockAlignment - 4) * 2);
            int num1 = pcmData.Length / 2;
            byte[] numArray = new byte[(long)(uint)Math.Ceiling((double)(uint)num / (double)num1) * (long)blockAlignment];
            MemoryStream memoryStream = new MemoryStream(pcmData);
			MemoryStream memoryStream1 = new MemoryStream(numArray);
			byte[] numArray1 = new byte[num * 2];
			while (memoryStream.Position < memoryStream.Length)
			{
				memoryStream.Read(numArray1, 0, (int)numArray1.Length);
				byte[] numArray2 = ImaAdpcmCodec.EncodeBlock(numArray1, num1, out num1, channelCount);
				memoryStream1.Write(numArray2, 0, (int)numArray2.Length);
			}
			memoryStream.Close();
			memoryStream1.Close();
			return numArray;
		}

		private static int NextStepIndex(int previousStep, int sample)
		{
			int num = previousStep + ImaAdpcmCodec.NextStepIndexTable[sample];
			if (num > 88)
			{
				num = 88;
			}
			else if (num < 0)
			{
				num = 0;
			}
			return num;
		}
	}
}