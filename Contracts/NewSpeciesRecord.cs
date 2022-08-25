using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Models;

namespace Contracts
{
    internal class NewSpeciesRecord : IMessage
    {
        public SpeciesRecord Record { init; get; }

        public NewSpeciesRecord(SpeciesRecord record)
        {
            Record = record;
        }

        public IMessage FromJson(string json)
        {
            throw new NotImplementedException();
        }

        public string ToJson()
        {
            throw new NotImplementedException();
        }
    }
}
