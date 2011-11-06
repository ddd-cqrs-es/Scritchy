﻿using System.Collections.Generic;
using Scritchy.Infrastructure.Implementations.EventStorage;
using Scritchy.Infrastructure.Implementations.EventStorage.Models;
using System.Data;
using System.Configuration;
using System;
using System.Data.Common;

namespace Scritchy.Infrastructure.Implementations.EventStorage.Adapters
{

    public class DBEventstoreAdapter : IEventstoreAdapter
    {
        IDbConnection conn;

        public DBEventstoreAdapter()
        {
            var cstr = ConfigurationManager.ConnectionStrings["eventstore"];
            var fact = DbProviderFactories.GetFactory(cstr.ProviderName);
            conn = fact.CreateConnection();
            conn.ConnectionString = cstr.ConnectionString;
            conn.Open();
        }

        public DBEventstoreAdapter(IDbConnection DB)
        {
            this.conn = DB;
        }

        public bool SaveEvent(EventBlob blob, IEnumerable<EventHeader> Headers)
        {
            var res = conn.Insert(blob);
            foreach (var h in Headers)
            {
                h.EventId = res.Id;
                conn.Insert(h);
            }
            return true;
        }

        public IEnumerable<EventBlob> FindAll(long fromid = 0)
        {
            return conn.AllBlobs((int)fromid);
        }

        public IEnumerable<EventBlob> FindAllWithHeader(string name, string value, long fromid = 0)
        {
            return conn.AllBlobsWithHeaderNameAndValue(name, value, (int)fromid);
        }
    }

}
