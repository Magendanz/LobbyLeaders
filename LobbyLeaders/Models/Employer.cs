using System;
using System.Collections.Generic;
using System.Text;

namespace LobbyList.Models
{
    public class Employer
    {
        // Metadata: https://www.pdc.wa.gov/browse/open-data/lobbyist-agent-employers
        public string id { get; set; }
        public string filer_id { get; set; }
        public string lobbyist_firm_name { get; set; }
        public string lobbyist_phone { get; set; } 
        public string lobbyist_email { get; set; }
        public string lobbyist_address { get; set; }
        public string employment_registration_id { get; set; } 
        public string employment_registration_title { get; set; } 
        public string employer_id { get; set; }
        public string employer_title { get; set; } 
        public string agent_name { get; set; } 
        public string agent_bio { get; set; } 
        public short employment_year { get; set; }
        //        public Link agent_pic_url { get; set; } 
        //        public Link lobbyist_firm_url { get; set; } 
        //        public Link employment_registration_url { get; set; } 
        //        public Link employer_url { get; set; } 
    }

    public class Link
    {
        public string url { get; set; }
    }
}
