using System;
using System.Collections.Generic;
using System.Text;

namespace LobbyLeaders.Models
{
    public class Recipient
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public List<Expenditure> Payments;
    }
}
