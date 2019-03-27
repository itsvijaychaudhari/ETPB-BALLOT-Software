using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETPB_BALLOT_Software.models
{
    public class LanguageCode
    {
        public Dictionary<string, string> LanguageDict;
        public Dictionary<string, string> StateLanguageDict;

        //added by vijay if user does not want state and language mapping 
        public Dictionary<string, string> LangMapedWithLocaleDict;

        public LanguageCode()
        {
            LanguageDict = new Dictionary<string, string>()
            {
                    {"U01" , "hi_in"},
                    {"S02" , ""},
                    {"S01" , "tl_in"},
                    {"S03" , "as_in"},
                    {"S04" , "hi_in"},
                    {"S26" , "hi_in"},
                    {"U02" , "hi_in"},
                    {"U03" , "gj_in"},
                    {"U04" , "gj_in"},
                    {"U05" , "hi_in"},
                    {"S05" , "hi_in"},
                    {"S06" , "gj_in"},
                    {"S07" , "hi_in"},
                    {"S08" , "hi_in"},
                    {"S09" , "ur_in"},
                    {"S27" , "hi_in"},
                    {"S10" , "kn_in"},
                    {"S11" , "ml_in"},
                    {"U06" , "ml_in"},
                    {"S12" , "hi_in"},
                    {"S13" , "mr_in"},
                    {"S14" , "bn_in"},
                    {"S15" , "hi_in"},
                    {"S16" , "hi_in"},
                    {"S17" , ""},
                    {"S18" , "or_in"},
                    {"U07" , "tm_in"},
                    {"S19" , "pn_in"},
                    {"S20" , "hi_in"},
                    {"S21" , "hi_in"},
                    {"S22" , "tm_in"},
                    {"S29" , "tl_in"},
                    {"S23" , "bn_in"},
                    {"S24" , "hi_in"},
                    {"S28" , "hi_in"},
                    {"S25" , "bn_in"}
            };

            StateLanguageDict = new Dictionary<string, string>()
            {
                    {"U01" , "Hindi"},
                    {"S02" , "English"},
                    {"S01" , "Telugu"},
                    {"S03" , "Assamese"},
                    {"S04" , "Hindi"},
                    {"S26" , "Hindi"},
                    {"U02" , "Hindi"},
                    {"U03" , "Gujarati"},
                    {"U04" , "Gujarati"},
                    {"U05" , "Hindi"},
                    {"S05" , "Kokani"},
                    {"S06" , "Gujarati"},
                    {"S07" , "Hindi"},
                    {"S08" , "Hindi"},
                    {"S09" , "Urdu"},
                    {"S27" , "Hindi"},
                    {"S10" , "Kannada"},
                    {"S11" , "Malayalam"},
                    {"U06" , "Malayalam"},
                    {"S12" , "Hindi"},
                    {"S13" , "Marathi"},
                    {"S14" , "Bengali"},
                    {"S15" , "Hindi"},
                    {"S16" , "Hindi"},
                    {"S17" , "English"},
                    {"S18" , "Oriya"},
                    {"U07" , "Tamil"},
                    {"S19" , "Punjabi"},
                    {"S20" , "Hindi"},
                    {"S21" , "Hindi"},
                    {"S22" , "Tamil"},
                    {"S29" , "Telugu"},
                    {"S23" , "Bengali"},
                    {"S24" , "Hindi"},
                    {"S28" , "Hindi"},
                    {"S25" , "Bengali"}
            };


            LangMapedWithLocaleDict = new Dictionary<string, string>()
            {
                {"Hindi"    , "hi_in"},
                {"English"  , ""},
                {"Telugu"   , "tl_in"},
                {"Assamese" , "as_in"},
                {"Gujarati" , "gj_in"},
                {"Kokani"   , "hi_in"},
                {"Urdu"     , "ur_in"},
                {"Kannada"  , "kn_in"},
                {"Malayalam", "ml_in"},
                {"Marathi"  , "mr_in"},
                {"Bengali"  , "bn_in"},
                {"Oriya"    , "or_in"},
                {"Tamil"    , "tm_in"},
                {"Punjabi"  , "pn_in"}
            };
        }
    }
}
