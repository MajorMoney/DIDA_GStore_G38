using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GStoreServer.Services
{
    class AttachService : AttachServerService.AttachServerServiceBase
    {
        private ServerShell shell;
        public AttachService(ServerShell shell)
        {
            this.shell = shell;
        }

        public async override Task<AttachReply> Attach(AttachRequest request, ServerCallContext context)
        {
            return await Task.FromResult(Att(request));
        }

        private AttachReply Att(AttachRequest request)
        {
            AttachReply reply = new AttachReply
            {
                Ok = true
            };
            return reply ;
        }

        public async override Task<ReadReply> Read(ReadRequest request, ServerCallContext context)
        {
            return await Task.FromResult((Rdd(request)));
        }

        private ReadReply Rdd(ReadRequest request)
        {
            string value = shell.GetObjectValue(request.PartitionID,request.ObjectID);
            ReadReply reply = new ReadReply
            {
                Value = value
            };
            return reply;
        }
    }
}
