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

            for (var year = 2018; year >= 2008; --year)
            {
                Console.WriteLine($"Analyzing donations for {year}...");
                Console.WriteLine("  Loading businesses...");
                contributions.AddRange(await pdc.GetContributionsByType(year, "Business", "Legislative", "Cash"));
                contributions.AddRange(await pdc.GetContributionsByType(year, "Business", "Statewide", "Cash"));
                Console.WriteLine("  Loading unions...");
                contributions.AddRange(await pdc.GetContributionsByType(year, "Union", "Legislative", "Cash"));
                contributions.AddRange(await pdc.GetContributionsByType(year, "Union", "Statewide", "Cash"));
                Console.WriteLine("  Loading PACs...");
                contributions.AddRange(await pdc.GetContributionsByType(year, "Political Action Committee", "Legislative", "Cash"));
                contributions.AddRange(await pdc.GetContributionsByType(year, "Political Action Committee", "Statewide", "Cash"));
            }
            Console.WriteLine();

            Console.WriteLine("Normalizing donor names...");
            var donors = pdc.GetDonorsFromContributions(contributions);
            Console.WriteLine();

            Console.WriteLine($"Writing donor list...");
            await TsvSerializer<Donor>.SerializeAsync(donors, "Donors (2008-18).txt");
            Console.WriteLine();

            Console.WriteLine($"Tallying contributions by year...");
            var scores = new List<Tally>();
            for (var year = 2018; year >= 2008; --year)
            {
                Console.WriteLine($"  {year}...");

                scores.AddRange(pdc.GetDonorSubtotals(donors, year, "Legislative", "Cash"));
                scores.AddRange(pdc.GetDonorSubtotals(donors, year, "Statewide", "Cash"));

                await TsvSerializer<Tally>.SerializeAsync(scores, "Scores (2008-18).txt");
            }
            Console.WriteLine();

            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }
    }

}
