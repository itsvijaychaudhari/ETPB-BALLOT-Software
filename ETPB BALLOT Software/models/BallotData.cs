using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETPB_BALLOT_Software.models
{
    public class BallotData
    {
        public int BallotID { get; set; }

        public string state { get; set; }

        public string constituency { get; set; }
        public string language1 { get; set; }
        public string language2 { get; set; }
        public string IsFinalized { get; set; }

        public string dateOfFinalization { get; set; }

    }
}
