using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GStoreServer.Services
{
    class SNodeService :NodeServerService.NodeServerServiceBase
    {
        private ServerShell shell;

        public SNodeService(ServerShell shell)
        {
            this.shell = shell;
        }

        public async override Task<SAck> Acknoledge(SCheckUp request, ServerCallContext context)
        {
            return await Task.FromResult(Ackk(request));
        }

        private SAck Ackk(SCheckUp request)
        {
            
            SAck reply = new SAck
            {
                Ack = shell.Setuper()
        };
            return reply;
        }
    }
}
