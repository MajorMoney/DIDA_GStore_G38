﻿using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    
    class PuppetMasterToNodeService :PuppetMasterService.PuppetMasterServiceBase
    {
        private PuppetMaster pm;


        public PuppetMasterToNodeService(PuppetMaster pm)
        {
            this.pm = pm;
        }

        public override async Task SetUp(SetUpRequest request, IServerStreamWriter<PartitionMap> responseStream, ServerCallContext context)
        {
            var partList = pm.GetPartitions();
            foreach (var i in partList) 
            {
                Dictionary<int, string> servers = pm.GetServersUrls(i);
                List<int> objectIds = pm.GetObjectsIDs(i);
                var reply = new PartitionMap();
                reply.PartitionID = i;
                reply.ServerInfo.Add(servers);
                reply.ObjectsID.Add(objectIds);
                await responseStream.WriteAsync(reply);                
            }
        }

        public override async Task<Objects> GetObjects(PopulateRequest request, ServerCallContext context)
        {
            Dictionary<int,string> objects = pm.GetObjects(request.PartitionID);
            var reply = new Objects();
            reply.PartitionID = request.PartitionID;
            reply.Objectos.Add(objects);
            return reply;
        }




    }
}
