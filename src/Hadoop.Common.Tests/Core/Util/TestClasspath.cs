using System.IO;
using System.Text;
using Hadoop.Common.Core.IO;
using Org.Apache.Commons.Logging;
using Org.Apache.Hadoop.FS;
using Org.Apache.Hadoop.IO;

using Jar;

namespace Org.Apache.Hadoop.Util
{
	/// <summary>Tests covering the classpath command-line utility.</summary>
	public class TestClasspath
	{
		private static readonly Log Log = LogFactory.GetLog(typeof(TestClasspath));

		private static readonly FilePath TestDir = new FilePath(Runtime.GetProperty("test.build.data"
			, "/tmp"), "TestClasspath");

		private static readonly Encoding Utf8 = Extensions.GetEncoding("UTF-8");

		static TestClasspath()
		{
			ExitUtil.DisableSystemExit();
		}

		private TextWriter oldStdout;

		private TextWriter oldStderr;

		private ByteArrayOutputStream stdout;

		private ByteArrayOutputStream stderr;

		private TextWriter printStdout;

		private TextWriter printStderr;

		[NUnit.Framework.SetUp]
		public virtual void SetUp()
		{
			Assert.True(FileUtil.FullyDelete(TestDir));
			Assert.True(TestDir.Mkdirs());
			oldStdout = System.Console.Out;
			oldStderr = System.Console.Error;
			stdout = new ByteArrayOutputStream();
			printStdout = new TextWriter(stdout);
			Runtime.SetOut(printStdout);
			stderr = new ByteArrayOutputStream();
			printStderr = new TextWriter(stderr);
			Runtime.SetErr(printStderr);
		}

		[NUnit.Framework.TearDown]
		public virtual void TearDown()
		{
			Runtime.SetOut(oldStdout);
			Runtime.SetErr(oldStderr);
			IOUtils.Cleanup(Log, printStdout, printStderr);
			Assert.True(FileUtil.FullyDelete(TestDir));
		}

		[Fact]
		public virtual void TestGlob()
		{
			Classpath.Main(new string[] { "--glob" });
			string strOut = new string(stdout.ToByteArray(), Utf8);
			Assert.Equal(Runtime.GetProperty("java.class.path"), strOut.Trim
				());
			Assert.True(stderr.ToByteArray().Length == 0);
		}

		/// <exception cref="System.IO.IOException"/>
		[Fact]
		public virtual void TestJar()
		{
			FilePath file = new FilePath(TestDir, "classpath.jar");
			Classpath.Main(new string[] { "--jar", file.GetAbsolutePath() });
			Assert.True(stdout.ToByteArray().Length == 0);
			Assert.True(stderr.ToByteArray().Length == 0);
			Assert.True(file.Exists());
			AssertJar(file);
		}

		/// <exception cref="System.IO.IOException"/>
		[Fact]
		public virtual void TestJarReplace()
		{
			// Run the command twice with the same output jar file, and expect success.
			TestJar();
			TestJar();
		}

		/// <exception cref="System.IO.IOException"/>
		[Fact]
		public virtual void TestJarFileMissing()
		{
			try
			{
				Classpath.Main(new string[] { "--jar" });
				NUnit.Framework.Assert.Fail("expected exit");
			}
			catch (ExitUtil.ExitException)
			{
				Assert.True(stdout.ToByteArray().Length == 0);
				string strErr = new string(stderr.ToByteArray(), Utf8);
				Assert.True(strErr.Contains("requires path of jar"));
			}
		}

		[Fact]
		public virtual void TestHelp()
		{
			Classpath.Main(new string[] { "--help" });
			string strOut = new string(stdout.ToByteArray(), Utf8);
			Assert.True(strOut.Contains("Prints the classpath"));
			Assert.True(stderr.ToByteArray().Length == 0);
		}

		[Fact]
		public virtual void TestHelpShort()
		{
			Classpath.Main(new string[] { "-h" });
			string strOut = new string(stdout.ToByteArray(), Utf8);
			Assert.True(strOut.Contains("Prints the classpath"));
			Assert.True(stderr.ToByteArray().Length == 0);
		}

		[Fact]
		public virtual void TestUnrecognized()
		{
			try
			{
				Classpath.Main(new string[] { "--notarealoption" });
				NUnit.Framework.Assert.Fail("expected exit");
			}
			catch (ExitUtil.ExitException)
			{
				Assert.True(stdout.ToByteArray().Length == 0);
				string strErr = new string(stderr.ToByteArray(), Utf8);
				Assert.True(strErr.Contains("unrecognized option"));
			}
		}

		/// <summary>
		/// Asserts that the specified file is a jar file with a manifest containing a
		/// non-empty classpath attribute.
		/// </summary>
		/// <param name="file">File to check</param>
		/// <exception cref="System.IO.IOException">if there is an I/O error</exception>
		private static void AssertJar(FilePath file)
		{
			JarFile jarFile = null;
			try
			{
				jarFile = new JarFile(file);
				Manifest manifest = jarFile.GetManifest();
				NUnit.Framework.Assert.IsNotNull(manifest);
				Attributes mainAttributes = manifest.GetMainAttributes();
				NUnit.Framework.Assert.IsNotNull(mainAttributes);
				Assert.True(mainAttributes.Contains(Attributes.Name.ClassPath));
				string classPathAttr = mainAttributes.GetValue(Attributes.Name.ClassPath);
				NUnit.Framework.Assert.IsNotNull(classPathAttr);
				NUnit.Framework.Assert.IsFalse(classPathAttr.IsEmpty());
			}
			finally
			{
				// It's too bad JarFile doesn't implement Closeable.
				if (jarFile != null)
				{
					try
					{
						jarFile.Close();
					}
					catch (IOException e)
					{
						Log.Warn("exception closing jarFile: " + jarFile, e);
					}
				}
			}
		}
	}
}
