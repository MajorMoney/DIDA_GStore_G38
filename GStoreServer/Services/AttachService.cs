using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GStoreServer.Services
{
    class AttachService : AttachServerService.AttachServerServiceBase
    {
        
        public async override Task<AttachReply> Attach(AttachRequest request, ServerCallContext context)
        {
            return await Task.FromResult(Att(request));
        }

        private AttachReply Att(AttachRequest request)
        {

            AttachReply reply = new AttachReply
            {
                Port = ServerManager.GetPort()
            };
            return reply ;
        }
    }
}
