using System;
using System.Collections.Generic;
using System.Text;

namespace LobbyLeaders.Models
{
    public class Tally
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public string Jurisdiction { get; set; }
        public int Count { get; set; }
        public int Wins { get; set; }
        public double Total { get; set; }
        public double Republican { get; set; }
        public double Democrat { get; set; }
    }
}
