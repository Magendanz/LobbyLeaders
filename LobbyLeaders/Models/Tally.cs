using System;
using System.Collections.Generic;
using System.Text;

namespace LobbyList.Models
{
    public class Tally
    {
        public int Donor { get; set; }
        public int Year { get; set; }
        public string Jurisdiction { get; set; }
        public int Count { get; set; }
        public double Total { get; set; }
        public double Bias { get; set; }
    }
}
