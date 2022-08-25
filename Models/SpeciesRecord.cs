using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public record SpeciesRecord(string DateRecorded, string DateAdded, string SpeciesId, string SpeciesName, string Recorder, double latitude, double longitude);

    public record SpeciesRecordWithId(string Id, SpeciesRecord Record);
}
