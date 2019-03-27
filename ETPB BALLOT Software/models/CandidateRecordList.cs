using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CandidateDetails;

namespace ETPB_BALLOT_Software.models
{
    public static class CandidateRecordList
    {
        private static List<CandidateRecord> candidateRecords = new List<CandidateRecord>();

        public static List<CandidateRecord> CandidateRecords
        {
            get { return candidateRecords; }
            set { candidateRecords = value; }
        }

    }
}
