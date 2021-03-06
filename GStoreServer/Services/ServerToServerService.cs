﻿using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GStoreServer.Services
{


    class ServerToServerService : PartitionMasterService.PartitionMasterServiceBase
    {
        private ServerShell shell;
        public ServerToServerService(ServerShell shell)
        {
            this.shell = shell;
        }

        public async override Task<LockAck> Lock(LockRequest request, ServerCallContext context)
        {
            return await Task.FromResult(Lck(request));
        }

        private LockAck Lck(LockRequest request)
        {
            
            LockAck reply = new LockAck
            {
                Ack = shell.Lock(request.PartitionID, request.ObjectID, request.Newvalue)
        };
            return reply;
        }

        public async override Task<UnlockAck> Unlock(UnlockRequest request, ServerCallContext context)
        {
            return await Task.FromResult(Ulck(request));
        }

        private UnlockAck Ulck(UnlockRequest request)
        {
            shell.Unlock();
            UnlockAck reply = new UnlockAck
            {
                Ack = true
            };
            return reply;
        }
    }

}
