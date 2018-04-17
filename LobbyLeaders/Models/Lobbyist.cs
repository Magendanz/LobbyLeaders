using System;
using System.Collections.Generic;
using System.Text;

namespace LobbyList.Models
{
    public class Lobbyist
    {
        // This is a composite from several sources
        public string Firm { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Employer { get; set; }
        public int DonorId { get; set; }
        public string Donor { get; set; }
        public string Total { get; set; }
        public string Republican { get; set; }
        public string Democrat { get; set; }
    }
}
