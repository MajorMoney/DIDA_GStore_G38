using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GStoreClient.Services
{
    class CNodeService : NodeClientService.NodeClientServiceBase
    {
        private ClientLogic client;

        public CNodeService(ClientLogic client)
        {
            this.client = client;
        }


        public async override Task<CAck> Acknoledge(CCheckUp request, ServerCallContext context)
        {
            return await Task.FromResult(Ackk(request));
        }

        private CAck Ackk(CCheckUp request)
        {
            CAck reply = new CAck
            {
                Ack = true
            };
            return reply;
        }

    }
}
