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

        public async override Task<SetUpAck> Acknoledge(CheckUp request, ServerCallContext context)
        {
            return await Task.FromResult(Ackk(request));
        }

        private SetUpAck Ackk(CheckUp request)
        {
            SetUpAck reply = new SetUpAck
            {
                Ack = shell.Setuper()
            };
            return reply;
        }

        public async override Task<FreezeReply> Freeze(FreezeRequest request, ServerCallContext context)
        {
            return await Task.FromResult(Frzz(request));
        }

        private FreezeReply Frzz(FreezeRequest request)
        {
            shell.Freeze();
            FreezeReply reply = new FreezeReply
            {
                Ack = true
            };
            return reply;
        }

        public async override Task<UnfreezeReply> Unfreeze(UnfreezeRequest request, ServerCallContext context)
        {
            return await Task.FromResult(Ufrzz(request));
        }

        private UnfreezeReply Ufrzz(UnfreezeRequest request)
        {
            shell.Unfreeze();
            UnfreezeReply reply = new UnfreezeReply
            {
                Ack = true
            };
            return reply;
        }

        public async override Task<CrashReply> Crash(CrashRequest request, ServerCallContext context)
        {
            return await Task.FromResult(Crsh(request));
        }

        private CrashReply Crsh(CrashRequest request)
        {
            CrashReply reply = new CrashReply
            {
                //Ack = shell.Setuper()
            };
            return reply;
        }
    }
}
