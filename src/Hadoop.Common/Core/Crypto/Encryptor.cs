

namespace Org.Apache.Hadoop.Crypto
{
	public interface Encryptor
	{
		/// <summary>Initialize the encryptor and the internal encryption context.</summary>
		/// <param name="key">encryption key.</param>
		/// <param name="iv">encryption initialization vector</param>
		/// <exception cref="System.IO.IOException">if initialization fails</exception>
		void Init(byte[] key, byte[] iv);

		/// <summary>Indicate whether the encryption context is reset.</summary>
		/// <remarks>
		/// Indicate whether the encryption context is reset.
		/// <p/>
		/// Certain modes, like CTR, require a different IV depending on the
		/// position in the stream. Generally, the encryptor maintains any necessary
		/// context for calculating the IV and counter so that no reinit is necessary
		/// during the encryption. Reinit before each operation is inefficient.
		/// </remarks>
		/// <returns>boolean whether context is reset.</returns>
		bool IsContextReset();

		/// <summary>This presents a direct interface encrypting with direct ByteBuffers.</summary>
		/// <remarks>
		/// This presents a direct interface encrypting with direct ByteBuffers.
		/// <p/>
		/// This function does not always encrypt the entire buffer and may potentially
		/// need to be called multiple times to process an entire buffer. The object
		/// may hold the encryption context internally.
		/// <p/>
		/// Some implementations may require sufficient space in the destination
		/// buffer to encrypt the entire input buffer.
		/// <p/>
		/// Upon return, inBuffer.position() will be advanced by the number of bytes
		/// read and outBuffer.position() by bytes written. Implementations should
		/// not modify inBuffer.limit() and outBuffer.limit().
		/// <p/>
		/// </remarks>
		/// <param name="inBuffer">
		/// a direct
		/// <see cref="ByteBuffer"/>
		/// to read from. inBuffer may
		/// not be null and inBuffer.remaining() must be &gt; 0
		/// </param>
		/// <param name="outBuffer">
		/// a direct
		/// <see cref="ByteBuffer"/>
		/// to write to. outBuffer may
		/// not be null and outBuffer.remaining() must be &gt; 0
		/// </param>
		/// <exception cref="System.IO.IOException">if encryption fails</exception>
		void Encrypt(ByteBuffer inBuffer, ByteBuffer outBuffer);
	}
}
