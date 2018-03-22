using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LobbyList.Helpers;
using LobbyList.Models;
using LobbyList.Services;

namespace LobbyList
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var pdc = new PdcService();
            var contributions = new List<Contribution>();

            for (var year = 2018; year >= 2003; --year)
            {
                Console.WriteLine($"Analyzing donations for {year}...");
                Console.WriteLine("  Loading businesses...");
                contributions.AddRange(await pdc.GetContributionsByType(year, "Business", "Legislative", "Cash"));
                Console.WriteLine("  Loading unions...");
                contributions.AddRange(await pdc.GetContributionsByType(year, "Union", "Legislative", "Cash"));
                Console.WriteLine("  Loading PACs...");
                contributions.AddRange(await pdc.GetContributionsByType(year, "Political Action Committee", "Legislative", "Cash"));
            }
            Console.WriteLine();

            Console.WriteLine("Normalizing donor names...");
            var donors = pdc.GetDonorsFromContributions(contributions).OrderByDescending(i => i.Total);
            Console.WriteLine();

            Console.WriteLine($"Writing results...");
            await TsvSerializer<Donor>.SerializeAsync(donors.OrderByDescending(i => i.Total), "Donors (2003-18).txt");

            for (var year = 2018; year >= 2003; --year)
            {
                var path = $"Donors ({year}).txt";
                Console.WriteLine($"  File: {path}");

                foreach (var donor in donors)
                {
                    var donations = donor.Contributions.Where(i => i.election_year == year).ToList();
                    donor.Count = donations.Count();
                    if (donor.Count > 0)
                    {
                        donor.Total = donations.Sum(i => i.amount);
                        donor.Republican = donations.Where(i => i.party == "REPUBLICAN").Sum(i => i.amount);
                        donor.Democrat = donations.Where(i => i.party == "DEMOCRAT").Sum(i => i.amount);
                    }
                }

                await TsvSerializer<Donor>.SerializeAsync(donors.Where(i => i.Count > 0).OrderByDescending(i => i.Total), path);
            }
            Console.WriteLine();

            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }
    }

}
