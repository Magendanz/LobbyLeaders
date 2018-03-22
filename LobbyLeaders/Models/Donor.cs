using System;
using System.Collections.Generic;
using System.Text;

namespace LobbyList.Models
{
    public class Donor
    {
//        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public int Count { get; set; }
        public double Total { get; set; }
        public double Republican { get; set; }
        public double Democrat { get; set; }

        public List<Contribution> Contributions;
    }
}
