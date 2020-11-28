using System;
using System.Collections.Generic;
using System.Text;

namespace LobbyLeaders.Models
{
    public abstract class Transaction
    {
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
        public string description { get; set; }
        public string memo { get; set; }
        public string code { get; set; }

        public abstract string Name { get; }
        public abstract string Address { get; }
        public abstract string City { get; }
        public abstract string State { get; }
        public abstract string Zip { get; }

        public string Description
            => code == "Individual" ? String.Join(' ', Name, Address, Zip) : Name ?? String.Empty;
    }
}
