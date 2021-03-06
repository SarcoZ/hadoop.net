using System.Collections.Generic;

namespace Hadoop.Common.Tests.Core.IO
{
	/// <summary>Tests MapWritable</summary>
	public class TestMapWritable : TestCase
	{
		/// <summary>the test</summary>
		public virtual void TestMapWritable()
		{
			Text[] keys = new Text[] { new Text("key1"), new Text("key2"), new Text("Key3") };
			BytesWritable[] values = new BytesWritable[] { new BytesWritable(Runtime.GetBytesForString
				("value1")), new BytesWritable(Runtime.GetBytesForString("value2")), new 
				BytesWritable(Runtime.GetBytesForString("value3")) };
			MapWritable inMap = new MapWritable();
			for (int i = 0; i < keys.Length; i++)
			{
				inMap[keys[i]] = values[i];
			}
			MapWritable outMap = new MapWritable(inMap);
			Assert.Equal(inMap.Count, outMap.Count);
			foreach (KeyValuePair<Writable, Writable> e in inMap)
			{
				Assert.True(outMap.Contains(e.Key));
				Assert.Equal(0, ((WritableComparable)outMap[e.Key]).CompareTo(
					e.Value));
			}
			// Now for something a little harder...
			Text[] maps = new Text[] { new Text("map1"), new Text("map2") };
			MapWritable mapOfMaps = new MapWritable();
			mapOfMaps[maps[0]] = inMap;
			mapOfMaps[maps[1]] = outMap;
			MapWritable copyOfMapOfMaps = new MapWritable(mapOfMaps);
			for (int i_1 = 0; i_1 < maps.Length; i_1++)
			{
				Assert.True(copyOfMapOfMaps.Contains(maps[i_1]));
				MapWritable a = (MapWritable)mapOfMaps[maps[i_1]];
				MapWritable b = (MapWritable)copyOfMapOfMaps[maps[i_1]];
				Assert.Equal(a.Count, b.Count);
				foreach (Writable key in a.Keys)
				{
					Assert.True(b.Contains(key));
					// This will work because we know what we put into each set
					WritableComparable aValue = (WritableComparable)a[key];
					WritableComparable bValue = (WritableComparable)b[key];
					Assert.Equal(0, aValue.CompareTo(bValue));
				}
			}
		}

		/// <summary>Test that number of "unknown" classes is propagated across multiple copies.
		/// 	</summary>
		public virtual void TestForeignClass()
		{
			MapWritable inMap = new MapWritable();
			inMap[new Text("key")] = new UTF8("value");
			inMap[new Text("key2")] = new UTF8("value2");
			MapWritable outMap = new MapWritable(inMap);
			MapWritable copyOfCopy = new MapWritable(outMap);
			Assert.Equal(1, copyOfCopy.GetNewClasses());
		}

		/// <summary>Assert MapWritable does not grow across calls to readFields.</summary>
		/// <exception cref="System.Exception"/>
		/// <seealso><a href="https://issues.apache.org/jira/browse/HADOOP-2244">HADOOP-2244</a>
		/// 	</seealso>
		public virtual void TestMultipleCallsToReadFieldsAreSafe()
		{
			// Create an instance and add a key/value.
			MapWritable m = new MapWritable();
			Text t = new Text(GetName());
			m[t] = t;
			// Get current size of map.  Key values are 't'.
			int count = m.Count;
			// Now serialize... save off the bytes.
			ByteArrayOutputStream baos = new ByteArrayOutputStream();
			DataOutputStream dos = new DataOutputStream(baos);
			m.Write(dos);
			dos.Close();
			// Now add new values to the MapWritable.
			m[new Text("key1")] = new Text("value1");
			m[new Text("key2")] = new Text("value2");
			// Now deserialize the original MapWritable.  Ensure count and key values
			// match original state.
			ByteArrayInputStream bais = new ByteArrayInputStream(baos.ToByteArray());
			DataInputStream dis = new DataInputStream(bais);
			m.ReadFields(dis);
			Assert.Equal(count, m.Count);
			Assert.True(m[t].Equals(t));
			dis.Close();
		}

		public virtual void TestEquality()
		{
			MapWritable map1 = new MapWritable();
			MapWritable map2 = new MapWritable();
			MapWritable map3 = new MapWritable();
			IntWritable k1 = new IntWritable(5);
			IntWritable k2 = new IntWritable(10);
			Text value = new Text("value");
			map1[k1] = value;
			// equal
			map2[k1] = value;
			// equal
			map3[k2] = value;
			// not equal
			Assert.True(map1.Equals(map2));
			Assert.True(map2.Equals(map1));
			NUnit.Framework.Assert.IsFalse(map1.Equals(map3));
			Assert.Equal(map1.GetHashCode(), map2.GetHashCode());
			NUnit.Framework.Assert.IsFalse(map1.GetHashCode() == map3.GetHashCode());
		}
	}
}
