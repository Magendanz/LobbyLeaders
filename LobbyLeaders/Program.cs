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
            await BuildLobbyList();

            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

        static async Task BuildLobbyList()
        {
            var pdc = new PdcService();

            Console.WriteLine("Loading lobbyists...");
            var agents = await pdc.GetAgents(2018);
            Console.WriteLine();

            Console.WriteLine("Loading donors...");
            var donors = await TsvSerializer<Donor>.DeserializeAsync("Donors (2008-18).tsv");
            Console.WriteLine();

            Console.WriteLine($"Loading employers...");
            var lobbyists = await pdc.GetLobbyListFromAgents(agents, donors);
            Console.WriteLine();

            Console.WriteLine($"Writing lobby list...");
            await TsvSerializer<Lobbyist>.SerializeAsync(lobbyists, "Lobbyists (2008-18).tsv");
            Console.WriteLine();
        }

        async Task MineDonors(short start, short end, int[] districts)
        {
            var pdc = new PdcService();
            var contributions = new List<Contribution>();

            for (var year = start; year <= end; ++year)
            {
                Console.WriteLine($"Analyzing donations for {year}...");
                Console.WriteLine("  Loading individuals...");
                foreach (var ld in districts)
                {
                    Console.WriteLine($"    District {ld}...");
                    contributions.AddRange(await pdc.GetContributionsByType(year, "Individual", "Legislative", $"{ld:D2}", "Cash"));
                }

                //contributions.AddRange(await pdc.GetContributionsByType(year, "Individual", "Statewide", null, "Cash"));
            }
            Console.WriteLine();

            Console.WriteLine("Normalizing donor names...");
            var donors = pdc.GetDonorsFromContributions(contributions);
            Console.WriteLine();

            Console.WriteLine($"Writing donor list...");
            await TsvSerializer<Donor>.SerializeAsync(donors, "Donors (2008-18).tsv");
            Console.WriteLine();

            Console.WriteLine($"Tallying contributions...");
            var scores = pdc.GetDonorTotals(donors);
            await TsvSerializer<Tally>.SerializeAsync(scores, "Scores (2008-18).tsv");
            Console.WriteLine();
        }

        async Task MineOrganizationalDonors(short start, short end)
        {
            var pdc = new PdcService();
            var contributions = new List<Contribution>();

            for (var year = start; year <= end; ++year)
            {
                Console.WriteLine($"Analyzing donations for {year}...");
                Console.WriteLine("  Loading businesses...");
                contributions.AddRange(await pdc.GetContributionsByType(year, "Business", "Legislative", null, "Cash"));
                contributions.AddRange(await pdc.GetContributionsByType(year, "Business", "Statewide", null, "Cash"));
                Console.WriteLine("  Loading unions...");
                contributions.AddRange(await pdc.GetContributionsByType(year, "Union", "Legislative", null, "Cash"));
                contributions.AddRange(await pdc.GetContributionsByType(year, "Union", "Statewide", null, "Cash"));
                Console.WriteLine("  Loading PACs...");
                contributions.AddRange(await pdc.GetContributionsByType(year, "Political Action Committee", "Legislative", null, "Cash"));
                contributions.AddRange(await pdc.GetContributionsByType(year, "Political Action Committee", "Statewide", null, "Cash"));
            }
            Console.WriteLine();

            Console.WriteLine("Normalizing donor names...");
            var donors = pdc.GetDonorsFromContributions(contributions);
            Console.WriteLine();

            Console.WriteLine($"Writing donor list...");
            await TsvSerializer<Donor>.SerializeAsync(donors, "Donors (2008-18).tsv");
            Console.WriteLine();

            Console.WriteLine($"Tallying contributions...");
            var scores = pdc.GetDonorTotals(donors);
            await TsvSerializer<Tally>.SerializeAsync(scores, "Scores (2008-18).tsv");
            Console.WriteLine();
        }
    }
}
