using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CandidateDetails
{
    public class CandidateRecord
    {
        public int DetailBallotID { get; set; }
        public int BallotID { get; set; }
        public int CandidateNO { get; set; }
        public string CandidateNameENG { get; set; }
        public string CandidateNameOL { get; set; }
        public string PartyNameENG { get; set; }
        public string PartyNameOL { get; set; }
        public byte[] CandidatePhoto { get; set; }

        public int ISNOTA { get; set; }

        
    }

    public class BallotCandidateList
    {
        public List<CandidateRecord> Candidate { get; set; }
    }


}
