using System;
using System.Collections.Generic;
using Com.Google.Common.Annotations;
using Com.Google.Common.Base;


namespace Org.Apache.Hadoop.Metrics2.Util
{
	/// <summary>
	/// Implementation of the Cormode, Korn, Muthukrishnan, and Srivastava algorithm
	/// for streaming calculation of targeted high-percentile epsilon-approximate
	/// quantiles.
	/// </summary>
	/// <remarks>
	/// Implementation of the Cormode, Korn, Muthukrishnan, and Srivastava algorithm
	/// for streaming calculation of targeted high-percentile epsilon-approximate
	/// quantiles.
	/// This is a generalization of the earlier work by Greenwald and Khanna (GK),
	/// which essentially allows different error bounds on the targeted quantiles,
	/// which allows for far more efficient calculation of high-percentiles.
	/// See: Cormode, Korn, Muthukrishnan, and Srivastava
	/// "Effective Computation of Biased Quantiles over Data Streams" in ICDE 2005
	/// Greenwald and Khanna,
	/// "Space-efficient online computation of quantile summaries" in SIGMOD 2001
	/// </remarks>
	public class SampleQuantiles
	{
		/// <summary>Total number of items in stream</summary>
		private long count = 0;

		/// <summary>Current list of sampled items, maintained in sorted order with error bounds
		/// 	</summary>
		private List<SampleQuantiles.SampleItem> samples;

		/// <summary>Buffers incoming items to be inserted in batch.</summary>
		/// <remarks>
		/// Buffers incoming items to be inserted in batch. Items are inserted into
		/// the buffer linearly. When the buffer fills, it is flushed into the samples
		/// array in its entirety.
		/// </remarks>
		private long[] buffer = new long[500];

		private int bufferCount = 0;

		/// <summary>Array of Quantiles that we care about, along with desired error.</summary>
		private readonly Quantile[] quantiles;

		public SampleQuantiles(Quantile[] quantiles)
		{
			this.quantiles = quantiles;
			this.samples = new List<SampleQuantiles.SampleItem>();
		}

		/// <summary>
		/// Specifies the allowable error for this rank, depending on which quantiles
		/// are being targeted.
		/// </summary>
		/// <remarks>
		/// Specifies the allowable error for this rank, depending on which quantiles
		/// are being targeted.
		/// This is the f(r_i, n) function from the CKMS paper. It's basically how wide
		/// the range of this rank can be.
		/// </remarks>
		/// <param name="rank">the index in the list of samples</param>
		private double AllowableError(int rank)
		{
			int size = samples.Count;
			double minError = size + 1;
			foreach (Quantile q in quantiles)
			{
				double error;
				if (rank <= q.quantile * size)
				{
					error = (2.0 * q.error * (size - rank)) / (1.0 - q.quantile);
				}
				else
				{
					error = (2.0 * q.error * rank) / q.quantile;
				}
				if (error < minError)
				{
					minError = error;
				}
			}
			return minError;
		}

		/// <summary>Add a new value from the stream.</summary>
		/// <param name="v"/>
		public virtual void Insert(long v)
		{
			lock (this)
			{
				buffer[bufferCount] = v;
				bufferCount++;
				count++;
				if (bufferCount == buffer.Length)
				{
					InsertBatch();
					Compress();
				}
			}
		}

		/// <summary>Merges items from buffer into the samples array in one pass.</summary>
		/// <remarks>
		/// Merges items from buffer into the samples array in one pass.
		/// This is more efficient than doing an insert on every item.
		/// </remarks>
		private void InsertBatch()
		{
			if (bufferCount == 0)
			{
				return;
			}
			Arrays.Sort(buffer, 0, bufferCount);
			// Base case: no samples
			int start = 0;
			if (samples.Count == 0)
			{
				SampleQuantiles.SampleItem newItem = new SampleQuantiles.SampleItem(buffer[0], 1, 
					0);
				samples.AddItem(newItem);
				start++;
			}
			ListIterator<SampleQuantiles.SampleItem> it = samples.ListIterator();
			SampleQuantiles.SampleItem item = it.Next();
			for (int i = start; i < bufferCount; i++)
			{
				long v = buffer[i];
				while (it.NextIndex() < samples.Count && item.value < v)
				{
					item = it.Next();
				}
				// If we found that bigger item, back up so we insert ourselves before it
				if (item.value > v)
				{
					it.Previous();
				}
				// We use different indexes for the edge comparisons, because of the above
				// if statement that adjusts the iterator
				int delta;
				if (it.PreviousIndex() == 0 || it.NextIndex() == samples.Count)
				{
					delta = 0;
				}
				else
				{
					delta = ((int)Math.Floor(AllowableError(it.NextIndex()))) - 1;
				}
				SampleQuantiles.SampleItem newItem = new SampleQuantiles.SampleItem(v, 1, delta);
				it.Add(newItem);
				item = newItem;
			}
			bufferCount = 0;
		}

		/// <summary>Try to remove extraneous items from the set of sampled items.</summary>
		/// <remarks>
		/// Try to remove extraneous items from the set of sampled items. This checks
		/// if an item is unnecessary based on the desired error bounds, and merges it
		/// with the adjacent item if it is.
		/// </remarks>
		private void Compress()
		{
			if (samples.Count < 2)
			{
				return;
			}
			ListIterator<SampleQuantiles.SampleItem> it = samples.ListIterator();
			SampleQuantiles.SampleItem prev = null;
			SampleQuantiles.SampleItem next = it.Next();
			while (it.HasNext())
			{
				prev = next;
				next = it.Next();
				if (prev.g + next.g + next.delta <= AllowableError(it.PreviousIndex()))
				{
					next.g += prev.g;
					// Remove prev. it.remove() kills the last thing returned.
					it.Previous();
					it.Previous();
					it.Remove();
					// it.next() is now equal to next, skip it back forward again
					it.Next();
				}
			}
		}

		/// <summary>Get the estimated value at the specified quantile.</summary>
		/// <param name="quantile">Queried quantile, e.g. 0.50 or 0.99.</param>
		/// <returns>Estimated value at that quantile.</returns>
		private long Query(double quantile)
		{
			Preconditions.CheckState(!samples.IsEmpty(), "no data in estimator");
			int rankMin = 0;
			int desired = (int)(quantile * count);
			ListIterator<SampleQuantiles.SampleItem> it = samples.ListIterator();
			SampleQuantiles.SampleItem prev = null;
			SampleQuantiles.SampleItem cur = it.Next();
			for (int i = 1; i < samples.Count; i++)
			{
				prev = cur;
				cur = it.Next();
				rankMin += prev.g;
				if (rankMin + cur.g + cur.delta > desired + (AllowableError(i) / 2))
				{
					return prev.value;
				}
			}
			// edge case of wanting max value
			return samples[samples.Count - 1].value;
		}

		/// <summary>Get a snapshot of the current values of all the tracked quantiles.</summary>
		/// <returns>
		/// snapshot of the tracked quantiles. If no items are added
		/// to the estimator, returns null.
		/// </returns>
		public virtual IDictionary<Quantile, long> Snapshot()
		{
			lock (this)
			{
				// flush the buffer first for best results
				InsertBatch();
				if (samples.IsEmpty())
				{
					return null;
				}
				IDictionary<Quantile, long> values = new SortedDictionary<Quantile, long>();
				for (int i = 0; i < quantiles.Length; i++)
				{
					values[quantiles[i]] = Query(quantiles[i].quantile);
				}
				return values;
			}
		}

		/// <summary>Returns the number of items that the estimator has processed</summary>
		/// <returns>count total number of items processed</returns>
		public virtual long GetCount()
		{
			lock (this)
			{
				return count;
			}
		}

		/// <summary>Returns the number of samples kept by the estimator</summary>
		/// <returns>count current number of samples</returns>
		[VisibleForTesting]
		public virtual int GetSampleCount()
		{
			lock (this)
			{
				return samples.Count;
			}
		}

		/// <summary>Resets the estimator, clearing out all previously inserted items</summary>
		public virtual void Clear()
		{
			lock (this)
			{
				count = 0;
				bufferCount = 0;
				samples.Clear();
			}
		}

		public override string ToString()
		{
			lock (this)
			{
				IDictionary<Quantile, long> data = Snapshot();
				if (data == null)
				{
					return "[no samples]";
				}
				else
				{
					return Joiner.On("\n").WithKeyValueSeparator(": ").Join(data);
				}
			}
		}

		/// <summary>
		/// Describes a measured value passed to the estimator, tracking additional
		/// metadata required by the CKMS algorithm.
		/// </summary>
		private class SampleItem
		{
			/// <summary>Value of the sampled item (e.g.</summary>
			/// <remarks>Value of the sampled item (e.g. a measured latency value)</remarks>
			public readonly long value;

			/// <summary>
			/// Difference between the lowest possible rank of the previous item, and
			/// the lowest possible rank of this item.
			/// </summary>
			/// <remarks>
			/// Difference between the lowest possible rank of the previous item, and
			/// the lowest possible rank of this item.
			/// The sum of the g of all previous items yields this item's lower bound.
			/// </remarks>
			public int g;

			/// <summary>
			/// Difference between the item's greatest possible rank and lowest possible
			/// rank.
			/// </summary>
			public readonly int delta;

			public SampleItem(long value, int lowerDelta, int delta)
			{
				this.value = value;
				this.g = lowerDelta;
				this.delta = delta;
			}

			public override string ToString()
			{
				return string.Format("%d, %d, %d", value, g, delta);
			}
		}
	}
}
