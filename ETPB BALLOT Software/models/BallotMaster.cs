using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETPB_BALLOT_Software.models
{
    public class BallotMaster
    {
        public int MasterID { get; set; }
        public string ConstituencyType { get; set; }
        public string ElectionType { get; set; }
        public int ElectionYear { get; set; }
        public string ConstituencyName { get; set; }
    }
}
