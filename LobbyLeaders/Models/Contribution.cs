using System;
using System.Collections.Generic;
using System.Text;

namespace LobbyList.Models
{
    public class Contribution
    {
        // Metadata: https://www.pdc.wa.gov/browse/open-data/contributions-candidates-and-political-committees
        public string id { get; set; }
        public string report_number { get; set; }
        public string origin { get; set; }
        public string filer_id { get; set; }
        public string type { get; set; }
        public string filer_name { get; set; }
        public string first_name { get; set; }
        public string middle_initial { get; set; }
        public string last_name { get; set; }
        public string office { get; set; }
        public string legislative_district { get; set; }
        public string position { get; set; }
        public string party { get; set; }
        public string ballot_number { get; set; }
        public string for_or_against { get; set; }
        public string jurisdiction { get; set; }
        public string jurisdiction_county { get; set; }
        public string jurisdiction_type { get; set; }
        public short election_year { get; set; }
        public double amount { get; set; }
        public string cash_or_in_kind { get; set; }
        public DateTime receipt_date { get; set; }
        public string description { get; set; }
        public string memo { get; set; }
        public string primary_general { get; set; }
        public string code { get; set; }
        public string contributor_name { get; set; }
        public string contributor_address { get; set; }
        public string contributor_city { get; set; }
        public string contributor_state { get; set; }
        public string contributor_zip { get; set; }
        public string contributor_occupation { get; set; }
        public string contributor_employer_name { get; set; }
        public string contributor_employer_city { get; set; }
        public string contributor_employer_state { get; set; }
    }
}
