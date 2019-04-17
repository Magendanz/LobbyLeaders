using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LobbyLeaders.Helpers;
using LobbyLeaders.Models;
using LobbyLeaders.Services;

namespace LobbyLeaders
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //await MineCaucusMemberDonors(2018, "R", "Legislative");
            //await MineCaucusDonors(2008, 2019);
            await MineOrganizationalDonors(2008, 2019, true);
            //await MineCampaignExpenses(2008, 2018);

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
            var donors = await TsvSerializer<Donor>.DeserializeAsync("Donors (2008-2018).tsv");
            Console.WriteLine();

            Console.WriteLine($"Loading employers...");
            var lobbyists = await pdc.GetLobbyListFromAgents(agents, donors);
            Console.WriteLine();

            Console.WriteLine($"Writing lobby list...");
            await TsvSerializer<Lobbyist>.SerializeAsync(lobbyists, "Lobbyists (2008-2018).tsv");
            Console.WriteLine();
        }

        static async Task MineDonors(short start, short end, int[] districts)
        {
            var pdc = new PdcService();
            var contributions = new List<Contribution>();
            var campaigns = new List<Committee>();

            for (var year = start; year <= end; ++year)
            {
                Console.WriteLine($"Analyzing donations for {year}...");
                Console.WriteLine("  Loading individuals...");
                foreach (var ld in districts)
                {
                    Console.WriteLine($"    District {ld}...");
                    contributions.AddRange(await pdc.GetContributionsByType(year, "Individual", "Legislative", $"{ld:D2}", "Cash"));
                }
                campaigns.AddRange(await pdc.GetCommittees(year, null, null, "Legislative"));

                //contributions.AddRange(await pdc.GetContributionsByType(year, "Individual", "Statewide", null, "Cash"));
                //campaigns.AddRange(await pdc.GetCommittees(year, null, null, "Statewide"));
            }
            Console.WriteLine();

            Console.WriteLine("Normalizing donor names...");
            var donors = pdc.GetDonorsFromContributions(contributions);
            Console.WriteLine();

            Console.WriteLine($"Writing donor list...");
            await TsvSerializer<Donor>.SerializeAsync(donors, $"Donors ({start}-{end}).tsv");
            Console.WriteLine();

            Console.WriteLine($"Tallying contributions...");
            var scores = pdc.GetDonorTotals(donors, campaigns);
            await TsvSerializer<Tally>.SerializeAsync(scores, $"Scores ({start}-{end}).tsv");
            Console.WriteLine();
        }

        static async Task MineOrganizationalDonors(short start, short end, bool summary)
        {
            var pdc = new PdcService();
            var contributions = new List<Contribution>();
            var campaigns = new List<Committee>();
            var scores = new List<Tally>();

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
                Console.WriteLine("  Loading campaigns...");
                campaigns.AddRange(await pdc.GetCommittees(year, null, null, "Legislative"));
                campaigns.AddRange(await pdc.GetCommittees(year, null, null, "Statewide"));
            }
            Console.WriteLine();

            Console.WriteLine("Normalizing donor names...");
            var donors = pdc.GetDonorsFromContributions(contributions);
            Console.WriteLine();

            Console.WriteLine($"Writing donor list...");
            await TsvSerializer<Donor>.SerializeAsync(donors, $"Donors ({start}-{end}).tsv");
            Console.WriteLine();

            if (summary)
            {
                Console.WriteLine($"Tallying contributions...");
                scores.AddRange(pdc.GetDonorTotals(donors, campaigns));
            }
            else
            {
                Console.WriteLine($"Tallying contributions by year...");
                for (short year = start; year <= end; ++year)
                {
                    Console.WriteLine($"  {year}...");

                    scores.AddRange(pdc.GetDonorTotals(donors, campaigns, year, "Legislative", "Cash"));
                    scores.AddRange(pdc.GetDonorTotals(donors, campaigns, year, "Statewide", "Cash"));
                }
            }

            await TsvSerializer<Tally>.SerializeAsync(scores, $"Scores ({start}-{end}).tsv");
            Console.WriteLine();
        }

        static async Task MineCaucusDonors(short start, short end)
        {
            await MineDonors(start, end, "HOUSRO%20507", "HROC");
            await MineDonors(start, end, "REAGF%20%20507", "Reagan Fund");
            await MineDonors(start, end, "HOUSDC%20507", "HDCC");
            await MineDonors(start, end, "HARRTF%20506", "Harry Truman Fund");
            await MineDonors(start, end, "SENARC%20148", "SRCC");
            await MineDonors(start, end, "LEADC%20%20148", "Leadership Council");
            await MineDonors(start, end, "SENADC%20507", "SDCC");
            await MineDonors(start, end, "HARRTF%20506", "Kennedy Fund");
        }

        static async Task MineDonors(short start, short end, string filer, string desc)
        {
            var pdc = new PdcService();
            var contributions = new List<Contribution>();

            Console.WriteLine($"Mining donors for {desc}:");
            for (var year = start; year <= end; ++year)
            {
                Console.WriteLine($"  Analyzing donations for {year}...");
                contributions.AddRange(await pdc.GetContributions(year, filer));
            }
            Console.WriteLine();

            Console.WriteLine("Normalizing donor names...");
            var donors = pdc.GetDonorsFromContributions(contributions);
            Console.WriteLine();

            Console.WriteLine($"Tallying contributions...");
            var sb = new StringBuilder("Contibutor");
            for (var year = start; year <= end; ++year)
                sb.Append($"\t{year}");
            sb.Append("\tTotal");

            foreach (var donor in donors)
            {
                sb.Append($"\n{donor.Name}");
                for (var year = start; year <= end; ++year)
                    sb.Append($"\t{donor.Contributions.Where(i => i.election_year == year).Sum(j => j.amount)}");
                sb.Append($"\t{donor.Contributions.Sum(i => i.amount)}");
            }

            File.WriteAllText($"{desc} ({start}-{end % 100}).tsv", sb.ToString());
            Console.WriteLine();
        }

        static async Task MineCaucusMemberDonors(short year, string party, string jurisdictionType = null)
        {
            var pdc = new PdcService();

            Console.WriteLine($"Retrieving active campaigns:");
            var campaigns = await pdc.GetCommittees(year, party, null, jurisdictionType);
            var active = campaigns.Where(i => (i.primary_election_status == "Qualified for general" || i.primary_election_status == "Unopposed in primary"));
            foreach (var candidate in active)
                await MineDonors(candidate.first_name, candidate.last_name, jurisdictionType);
        }

        static async Task MineDonors(string first, string last, string jurisdictionType = null)
        {
            var pdc = new PdcService();
            var contributions = new List<Contribution>();

            Console.WriteLine($"Retrieving campaigns for {first} {last}:");
            var campaigns = await pdc.GetCommittees(first, last, jurisdictionType);
            var years = campaigns.Select(i => i.election_year).Distinct().OrderBy(i => i).ToArray();
            foreach (var year in years)
            {
                Console.WriteLine($"  Analyzing donations for {year}...");
                foreach (var campaign in campaigns.Where(i => i.election_year == year))
                    contributions.AddRange(await pdc.GetContributions(year, campaign.filer_id));
            }
            Console.WriteLine();

            Console.WriteLine("Normalizing donor names...");
            var donors = pdc.GetDonorsFromContributions(contributions);
            Console.WriteLine();

            Console.WriteLine($"Tallying contributions...");
            var sb = new StringBuilder("Contributor");
            foreach (var year in years)
                sb.Append($"\t{year}");
            sb.Append("\tTotal");

            foreach (var donor in donors)
            {
                sb.Append($"\n{donor.Name}");
                foreach (var year in years)
                    sb.Append($"\t{donor.Contributions.Where(i => i.election_year == year).Sum(j => j.amount)}");
                sb.Append($"\t{donor.Contributions.Sum(i => i.amount)}");
            }

            File.WriteAllText($"{first} {last}.tsv", sb.ToString());
            Console.WriteLine();
        }

        static async Task MineCampaignExpenses(short start, short end)
        {
            var pdc = new PdcService();
            var expenses = new List<Expenditure>();
            var campaigns = new List<Committee>();

            for (var year = start; year <= end; ++year)
            {
                Console.WriteLine($"Analyzing expenses for {year}...");
                expenses.AddRange(await pdc.GetExpensesByType(year, "Candidate", null, "Legislative"));
                campaigns.AddRange(await pdc.GetCommittees(year, null, "Candidate", "Legislative"));
            }
            Console.WriteLine();

            Console.WriteLine("Normalizing recipient names...");
            var recipients = pdc.GetRecipientsFromExpenses(expenses);
            Console.WriteLine();

            Console.WriteLine($"Writing recipient list...");
            await TsvSerializer<Recipient>.SerializeAsync(recipients, $"Recipients ({start}-{end}).tsv");
            Console.WriteLine();

            Console.WriteLine($"Tallying expenses...");
            var scores = pdc.GetRecipientTotals(recipients, campaigns);
            await TsvSerializer<Tally>.SerializeAsync(scores, $"Scores ({start}-{end}).tsv");
            Console.WriteLine();
        }

    }
}
