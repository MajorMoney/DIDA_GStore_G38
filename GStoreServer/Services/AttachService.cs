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
            Console.WriteLine(request.Nick+" registered on port"+request.Url);
            AttachReply reply = new AttachReply
            {
                Ok = true
            };
            return reply ;
        }
    }
}
