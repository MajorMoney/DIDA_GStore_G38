using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GStoreClient.Services
{
    class ClientToServerService : ClientService.ClientServiceBase
    {
        private ClientLogic client;

        public ClientToServerService(ClientLogic client)
        {
            this.client = client;
        }

        public async override Task<ValueAck> ReadValue(ValueNotification request, ServerCallContext context)
        {
            return await Task.FromResult(RV(request));
        }

        private ValueAck RV(ValueNotification request)
        {
            var a= client.ReceiveValue(request.Value);
            ValueAck reply = new ValueAck
            {
                Ack = true
            };
            return reply;
        }

        public async override Task<ValueAck> WriteValue(ValueNotification request, ServerCallContext context)
        {
            return await Task.FromResult(Wrt(request));
        }

        private ValueAck Wrt(ValueNotification request)
        {
            var a = client.ReceiveValue(request.Value);
            ValueAck reply = new ValueAck
            {
                Ack = true
            };
            return reply;
        }
    }
}
