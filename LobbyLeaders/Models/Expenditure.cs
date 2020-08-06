using System;
using System.Collections.Generic;
using System.Text;

namespace LobbyLeaders.Models
{
    public class Expenditure
    {
        // Metadata: https://www.pdc.wa.gov/browse/open-data/expenditures-candidates-and-political-committees
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
        public string itemized_or_non_itemized { get; set; }
        public DateTime expenditure_date { get; set; }
        public string description { get; set; }
        public string code { get; set; }
        public string recipient_name { get; set; }
        public string recipient_address { get; set; }
        public string recipient_city { get; set; }
        public string recipient_state { get; set; }
        public string recipient_zip { get; set; }

        public string Description
            => String.Join(' ', recipient_name, recipient_address, recipient_zip);
    }
}
