using System;
using Org.Apache.Hadoop.Conf;
using Org.Apache.Hadoop.FS;
using Org.Apache.Hadoop.IO;
using Org.Apache.Hadoop.IO.Compress;
using Org.Apache.Hadoop.Mapreduce;
using Org.Apache.Hadoop.Util;
using Sharpen;

namespace Org.Apache.Hadoop.Mapreduce.Lib.Output
{
	/// <summary>
	/// An
	/// <see cref="Org.Apache.Hadoop.Mapreduce.OutputFormat{K, V}"/>
	/// that writes
	/// <see cref="Org.Apache.Hadoop.IO.MapFile"/>
	/// s.
	/// </summary>
	public class MapFileOutputFormat : FileOutputFormat<WritableComparable<object>, Writable
		>
	{
		/// <exception cref="System.IO.IOException"/>
		public override RecordWriter<WritableComparable<object>, Writable> GetRecordWriter
			(TaskAttemptContext context)
		{
			Configuration conf = context.GetConfiguration();
			CompressionCodec codec = null;
			SequenceFile.CompressionType compressionType = SequenceFile.CompressionType.None;
			if (GetCompressOutput(context))
			{
				// find the kind of compression to do
				compressionType = SequenceFileOutputFormat.GetOutputCompressionType(context);
				// find the right codec
				Type codecClass = GetOutputCompressorClass(context, typeof(DefaultCodec));
				codec = (CompressionCodec)ReflectionUtils.NewInstance(codecClass, conf);
			}
			Path file = GetDefaultWorkFile(context, string.Empty);
			FileSystem fs = file.GetFileSystem(conf);
			// ignore the progress parameter, since MapFile is local
			MapFile.Writer @out = new MapFile.Writer(conf, fs, file.ToString(), context.GetOutputKeyClass
				().AsSubclass<WritableComparable>(), context.GetOutputValueClass().AsSubclass<Writable
				>(), compressionType, codec, context);
			return new _RecordWriter_75(@out);
		}

		private sealed class _RecordWriter_75 : RecordWriter<WritableComparable<object>, 
			Writable>
		{
			public _RecordWriter_75(MapFile.Writer @out)
			{
				this.@out = @out;
			}

			/// <exception cref="System.IO.IOException"/>
			public override void Write<_T0>(WritableComparable<_T0> key, Writable value)
			{
				@out.Append(key, value);
			}

			/// <exception cref="System.IO.IOException"/>
			public override void Close(TaskAttemptContext context)
			{
				@out.Close();
			}

			private readonly MapFile.Writer @out;
		}

		/// <summary>Open the output generated by this format.</summary>
		/// <exception cref="System.IO.IOException"/>
		public static MapFile.Reader[] GetReaders(Path dir, Configuration conf)
		{
			FileSystem fs = dir.GetFileSystem(conf);
			Path[] names = FileUtil.Stat2Paths(fs.ListStatus(dir));
			// sort names, so that hash partitioning works
			Arrays.Sort(names);
			MapFile.Reader[] parts = new MapFile.Reader[names.Length];
			for (int i = 0; i < names.Length; i++)
			{
				parts[i] = new MapFile.Reader(fs, names[i].ToString(), conf);
			}
			return parts;
		}

		/// <summary>Get an entry from output generated by this class.</summary>
		/// <exception cref="System.IO.IOException"/>
		public static Writable GetEntry<K, V>(MapFile.Reader[] readers, Partitioner<K, V>
			 partitioner, K key, V value)
			where K : WritableComparable<object>
			where V : Writable
		{
			int part = partitioner.GetPartition(key, value, readers.Length);
			return readers[part].Get(key, value);
		}
	}
}
