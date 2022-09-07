using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Models;

namespace Contracts
{
    public class NewSpeciesRecord : IMessage
    {
        public SpeciesRecordWithId Record { init; get; }

        public NewSpeciesRecord(SpeciesRecordWithId record)
        {
            Record = record;
        }

        public string ToJson()
        {
            throw new NotImplementedException();
        }
    }
}
