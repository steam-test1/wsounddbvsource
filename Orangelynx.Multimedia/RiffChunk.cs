using System;
using System.Collections.Generic;
using System.Text;

namespace Orangelynx.Multimedia
{
	public class RiffChunk
	{
		public const int HeaderSize = 8;

		private byte[] ID_;

		private byte[] Data_;

		private List<RiffChunk> Subchunks_ = new List<RiffChunk>();

		public byte[] Data
		{
			get
			{
				return this.Data_;
			}
			set
			{
				this.Data_ = value;
			}
		}

		public string ID
		{
			get
			{
				return Encoding.ASCII.GetString(this.ID_);
			}
			private set
			{
				if (value.Length != 4)
				{
					throw new InvalidOperationException("header has to be of length 4 characters.");
				}
				this.ID_ = Encoding.ASCII.GetBytes(value);
			}
		}

		public long Size
		{
			get
			{
				long length = (long)0;
				length += (long)this.Data_.Length;
				foreach (RiffChunk subchunk in this.Subchunks)
				{
					length = length + subchunk.Size + (long)8 + subchunk.Size % (long)2;
				}
				return length;
			}
		}

		public IEnumerable<RiffChunk> Subchunks
		{
			get
			{
				return this.Subchunks_.AsReadOnly();
			}
		}

		public RiffChunk(string id)
		{
			this.ID = id;
		}

		public void AddSubchunk(RiffChunk subchunk)
		{
			this.Subchunks_.Add(subchunk);
		}

		public void InsertSubchunk(RiffChunk subchunk, int index)
		{
			this.Subchunks_.Insert(index, subchunk);
		}

		public void RemoveSubchunk(RiffChunk subchunk)
		{
			this.Subchunks_.Remove(subchunk);
		}

		public void RemoveSubchunkAt(int index)
		{
			this.Subchunks_.RemoveAt(index);
		}
	}
}