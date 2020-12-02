﻿using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace GStoreServer.Services
{
    class ServerToClientService : AttachServerService.AttachServerServiceBase
    {
        private ServerShell shell;
        public ServerToClientService(ServerShell shell)
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
            Debug.WriteLine("TESTOU!");
            string value = shell.GetObjectValue(request.PartitionID,request.ObjectID);
            ReadReply reply = new ReadReply
            {
                Value = value
            };
            return reply;
        }


        public async override Task<WriteReply> Write(WriteRequest request, ServerCallContext context)
        {
            return await Task.FromResult((Wrt(request)));
        }

        private WriteReply Wrt(WriteRequest request)
        {
            bool Ack = shell.Write(request.PartitionID,request.ObjectID,request.Value);
            WriteReply reply = new WriteReply
            {
                ObjID = 1,
                Value = "teste"
            };
            return reply;
        }
    }
}