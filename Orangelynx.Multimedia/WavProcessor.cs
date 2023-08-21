using System;

namespace Orangelynx.Multimedia
{
	public class WavProcessor
	{
		public WavProcessor()
		{
		}

		public static void ConvertToADPCM(WavFile wavFile)
		{
			WavFile.WAVEFormatCategories wAVEFormatCategory;
			if (wavFile == null)
			{
				throw new ArgumentNullException("wavFile", "wavFile has to be loaded.");
			}
			if (wavFile.FormatTag != 1)
			{
				if (!Enum.IsDefined(typeof(WavFile.WAVEFormatCategories), wavFile.FormatTag))
				{
					wAVEFormatCategory = WavFile.WAVEFormatCategories.MS_ADPCM;
					throw new InvalidOperationException(string.Format("Conversion from 0x{0} to {1} is not supported.", wavFile.FormatTag, wAVEFormatCategory.ToString()));
				}
				wAVEFormatCategory = WavFile.WAVEFormatCategories.MS_ADPCM;
				throw new InvalidOperationException(string.Format("Conversion from {0} to {1} is not supported.", Enum.ToObject(typeof(WavFile.WAVEFormatCategories), wavFile.FormatTag).ToString(), wAVEFormatCategory.ToString()));
			}
			int num = 36;
			int num1 = 64;
			byte[] numArray = ImaAdpcmCodec.EncodeStream(wavFile.AudioData, (int)wavFile.Channels, num);
			wavFile.AudioData = numArray;
			wavFile.FormatTag = 2;
			WavFile channels = wavFile;
			channels.Channels = channels.Channels;
			WavFile samplesPerSec = wavFile;
			samplesPerSec.SamplesPerSec = samplesPerSec.SamplesPerSec;
			wavFile.AvgBytesPerSec = Convert.ToUInt32((float)((float)(wavFile.Channels * wavFile.SamplesPerSec)) / (float)num1 * (float)num);
			wavFile.BlockAlign = Convert.ToUInt16(num);
			wavFile.BitsPerSample = 4;
			if (wavFile.Channels == 1)
			{
				wavFile.FormatExtension = new byte[] { 0, 0, 4, 0, 0, 0 };
				return;
			}
			wavFile.FormatExtension = new byte[] { 0, 0, 3, 0, 0, 0 };
		}

		public static void ConvertToPCM(WavFile wavFile)
		{
			WavFile.WAVEFormatCategories wAVEFormatCategory;
			if (wavFile == null)
			{
				throw new ArgumentNullException("wavFile", "wavFile has to be loaded.");
			}
			if (wavFile.FormatTag != 2)
			{
				if (!Enum.IsDefined(typeof(WavFile.WAVEFormatCategories), wavFile.FormatTag))
				{
					wAVEFormatCategory = WavFile.WAVEFormatCategories.PCM;
					throw new InvalidOperationException(string.Format("Conversion from 0x{0} to {1} is not supported.", wavFile.FormatTag, wAVEFormatCategory.ToString()));
				}
				wAVEFormatCategory = WavFile.WAVEFormatCategories.PCM;
				throw new InvalidOperationException(string.Format("Conversion from {0} to {1} is not supported.", Enum.ToObject(typeof(WavFile.WAVEFormatCategories), wavFile.FormatTag).ToString(), wAVEFormatCategory.ToString()));
			}
			byte[] numArray = ImaAdpcmCodec.DecodeStream(wavFile.AudioData, (int)wavFile.Channels, (int)wavFile.BlockAlign);
			wavFile.AudioData = numArray;
			wavFile.FormatTag = 1;
			WavFile channels = wavFile;
			channels.Channels = channels.Channels;
			WavFile samplesPerSec = wavFile;
			samplesPerSec.SamplesPerSec = samplesPerSec.SamplesPerSec;
			WavFile channels1 = wavFile;
			channels1.AvgBytesPerSec = channels1.Channels * wavFile.SamplesPerSec * 2;
			wavFile.BlockAlign = Convert.ToUInt16(wavFile.Channels * 2);
			wavFile.BitsPerSample = 16;
			wavFile.FormatExtension = null;
		}
	}
}