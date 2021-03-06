using NUnit.Framework;


namespace Org.Apache.Hadoop.Oncrpc
{
	/// <summary>
	/// Test for
	/// <see cref="RpcMessage"/>
	/// </summary>
	public class TestRpcMessage
	{
		private RpcMessage GetRpcMessage(int xid, RpcMessage.Type msgType)
		{
			return new _RpcMessage_28(xid, msgType);
		}

		private sealed class _RpcMessage_28 : RpcMessage
		{
			public _RpcMessage_28(int baseArg1, RpcMessage.Type baseArg2)
				: base(baseArg1, baseArg2)
			{
			}

			public override XDR Write(XDR xdr)
			{
				return null;
			}
		}

		[Fact]
		public virtual void TestRpcMessage()
		{
			RpcMessage msg = GetRpcMessage(0, RpcMessage.Type.RpcCall);
			Assert.Equal(0, msg.GetXid());
			Assert.Equal(RpcMessage.Type.RpcCall, msg.GetMessageType());
		}

		[Fact]
		public virtual void TestValidateMessage()
		{
			RpcMessage msg = GetRpcMessage(0, RpcMessage.Type.RpcCall);
			msg.ValidateMessageType(RpcMessage.Type.RpcCall);
		}

		public virtual void TestValidateMessageException()
		{
			RpcMessage msg = GetRpcMessage(0, RpcMessage.Type.RpcCall);
			msg.ValidateMessageType(RpcMessage.Type.RpcReply);
		}
	}
}
