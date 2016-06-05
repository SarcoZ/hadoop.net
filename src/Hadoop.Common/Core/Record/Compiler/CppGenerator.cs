using System.Collections.Generic;
using System.IO;
using Org.Apache.Hadoop.Util;


namespace Org.Apache.Hadoop.Record.Compiler
{
	/// <summary>C++ Code generator front-end for Hadoop record I/O.</summary>
	internal class CppGenerator : CodeGenerator
	{
		internal CppGenerator()
		{
		}

		/// <summary>Generate C++ code.</summary>
		/// <remarks>
		/// Generate C++ code. This method only creates the requested file(s)
		/// and spits-out file-level elements (such as include statements etc.)
		/// record-level code is generated by JRecord.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		internal override void GenCode(string name, AList<JFile> ilist, AList<JRecord> rlist
			, string destDir, AList<string> options)
		{
			name = new FilePath(destDir, (new FilePath(name)).GetName()).GetAbsolutePath();
			FileWriter cc = new FileWriter(name + ".cc");
			try
			{
				FileWriter hh = new FileWriter(name + ".hh");
				try
				{
					string fileName = (new FilePath(name)).GetName();
					hh.Write("#ifndef __" + StringUtils.ToUpperCase(fileName).Replace('.', '_') + "__\n"
						);
					hh.Write("#define __" + StringUtils.ToUpperCase(fileName).Replace('.', '_') + "__\n"
						);
					hh.Write("#include \"recordio.hh\"\n");
					hh.Write("#include \"recordTypeInfo.hh\"\n");
					for (IEnumerator<JFile> iter = ilist.GetEnumerator(); iter.HasNext(); )
					{
						hh.Write("#include \"" + iter.Next().GetName() + ".hh\"\n");
					}
					cc.Write("#include \"" + fileName + ".hh\"\n");
					cc.Write("#include \"utils.hh\"\n");
					for (IEnumerator<JRecord> iter_1 = rlist.GetEnumerator(); iter_1.HasNext(); )
					{
						iter_1.Next().GenCppCode(hh, cc, options);
					}
					hh.Write("#endif //" + StringUtils.ToUpperCase(fileName).Replace('.', '_') + "__\n"
						);
				}
				finally
				{
					hh.Close();
				}
			}
			finally
			{
				cc.Close();
			}
		}
	}
}