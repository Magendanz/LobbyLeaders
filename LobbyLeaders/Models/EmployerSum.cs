using System;
using System.Collections.Generic;
using System.Text;

namespace LobbyList.Models
{
    public class EmployerSum
    {
        // Metadata: https://www.pdc.wa.gov/browse/open-data/lobbyist-agent-employers
        public string id { get; set; }
        public short year { get; set; }
        public string Employer_Name { get; set; }
        public string Employer_Email { get; set; }
        public string Employer_Phone { get; set; } 
        public string Employer_Address { get; set; }
        public string Employer_City { get; set; }
        public string Employer_State { get; set; } 
        public string Employer_Zip { get; set; } 
        public string Employer_Country { get; set; }
        public double compensation { get; set; }
        public double expenditures { get; set; }
        public double agg_contrib { get; set; }
        public double ballot_prop { get; set; }
        public double entertain { get; set; }
        public double vendor { get; set; }
        public double expert_retain { get; set; }
        public double inform_material { get; set; }
        public double lobbying_comm { get; set; }
        public double ie_in_support { get; set; }
        public double itemized_exp { get; set; }
        public double other_l3_exp { get; set; }
        public double political { get; set; }
        public double corr_compensation { get; set; }
        public double corr_expend { get; set; }
        public double total_exp { get; set; }
        public string l3_nid { get; set; }
        public string employer_nid { get; set; }
    }
}
