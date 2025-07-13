using System.Collections.Generic;
using System.Diagnostics;

namespace Scabra.Rpc.Server
{
    internal class ReplyQueue : MutexableQueue<Reply>
    {        
        public ReplyQueue(IEndpointLogger logger) : this(128, 1000, logger) { }

        public ReplyQueue(int length, int gainAccessTimeoutInMs, IEndpointLogger logger) 
            : base(length, initialTimeoutInMs: 1, gainAccessTimeoutInMs, logger)
        {   
        }

        public void Enqueue(IEnumerable<byte[]> replyEnvelop, byte[] replyData)
        {
            Debug.Assert(replyEnvelop != null, "Reply envelop is null.");
            Debug.Assert(replyData != null, "Reply data is null.");

            Enqueue(reply => reply.Initialize(replyEnvelop, replyData));
        }

        public bool TryDequeue(Reply reply)
        {
            return TryDequeue(reply.CopyFrom);
        }
    }
}