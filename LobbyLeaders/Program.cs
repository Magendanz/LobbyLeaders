using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using DataAccess;

using LobbyLeaders.Helpers;
using LobbyLeaders.Models;
using LobbyLeaders.Services;

namespace LobbyLeaders
{
    class Program
    {
        static PdcService pdc = new PdcService();

        static async Task Main(string[] args)
        {
            //await MineCaucusPacDonors(2008, 2020);
            //await MineCaucusPacExpenses(2008, 2020);
            //await MineCaucusMemberDonors(2008, 2020, "R");
            //await MineDistrictDonors(2008, 2020, 2, 5, 10, 13, 19, 25, 26, 28);
            //await MineOrganizationalDonors(2008, 2020);
            //await MineIndividuallDonors(2008, 2020);
            //await MineCampaignExpenses(2008, 2020);
            //await MineOrganizationalRecipients(2008, 2020, "WEA");
            await MineCampaignsRaisedByType(2020, 2020, null, "Legislative");
        }

        static async Task MineOrganizationalDonors(short start, short end)
        {
            var contributions = new List<Contribution>();
            for (var year = end; year >= start; year--)
            {
                Console.WriteLine($"Analyzing donations for {year}...");
                Console.WriteLine("  Loading businesses...");
                contributions.AddRange(await GetContributions(year, "Business", "Legislative"));
                contributions.AddRange(await GetContributions(year, "Business", "Statewide"));
                Console.WriteLine("  Loading unions...");
                contributions.AddRange(await GetContributions(year, "Union", "Legislative"));
                contributions.AddRange(await GetContributions(year, "Union", "Statewide"));
                Console.WriteLine("  Loading PACs...");
                contributions.AddRange(await GetContributions(year, "Political Action Committee", "Legislative"));
                contributions.AddRange(await GetContributions(year, "Political Action Committee", "Statewide"));
            }
            Console.WriteLine();

            var donors = AggregateTransactionsByDonor(contributions);
            await GenerateDonorReport(donors, start, end, "Organizational Donors", true);
        }

        static async Task MineOrganizationalRecipients(short start, short end, string description)
        {
            var contributions = new List<Contribution>();
            for (var year = end; year >= start; year--)
            {
                Console.WriteLine($"Analyzing donations for {year}...");
                Console.WriteLine("  Loading businesses...");
                contributions.AddRange(await GetContributions(year, "Business", "Legislative"));
                contributions.AddRange(await GetContributions(year, "Business", "Statewide"));
                Console.WriteLine("  Loading unions...");
                contributions.AddRange(await GetContributions(year, "Union", "Legislative"));
                contributions.AddRange(await GetContributions(year, "Union", "Statewide"));
                Console.WriteLine("  Loading PACs...");
                contributions.AddRange(await GetContributions(year, "Political Action Committee", "Legislative"));
                contributions.AddRange(await GetContributions(year, "Political Action Committee", "Statewide"));
            }
            Console.WriteLine();

            var recipients = FilterTransactions(contributions, description, 0.8);
            GenerateFilerReport(recipients, start, end, description + " Recipients");
        }

        static async Task MineIndividuallDonors(short start, short end)
        {
            var contributions = new List<Contribution>();
            for (var year = end; year >= start; year--)
            {
                Console.WriteLine($"Analyzing donations for {year}...");
                Console.WriteLine("  Loading individuals...");
                contributions.AddRange(await GetContributions(year, "Individual", "Legislative"));
                contributions.AddRange(await GetContributions(year, "Individual", "Statewide"));
            }
            Console.WriteLine();

            var donors = AggregateTransactionsByDonor(contributions);
            await GenerateDonorReport(donors, start, end, "Individual Donors", true);
        }

        static async Task MineCampaignExpenses(short start, short end)
        {
            var expenses = new List<Expenditure>();
            for (var year = end; year >= start; year--)
            {
                Console.WriteLine($"Analyzing expenses for {year}...");
                expenses.AddRange(await pdc.GetExpensesByType(year, "Candidate", null, "Legislative"));
                expenses.AddRange(await pdc.GetExpensesByType(year, "Candidate", null, "Statewide"));
            }
            Console.WriteLine();

            var recipients = AggregateTransactionsByDonor(expenses);
            await GenerateDonorReport(recipients, start, end, "Campaign Expenditures", true);
        }

        static async Task MineDistrictDonors(short start, short end, params int[] districts)
        {
            foreach (var ld in districts)
            {
                Console.WriteLine($"Analyzing donations for LD{ld:D2}...");
                await MineDonorsByType(start, end, $"LD{ld:D2} Donors", null, null, $"{ld:D2}");
            }
        }

        static async Task MineCaucusMemberDonors(short start, short end, string party)
        {
            Console.WriteLine($"Retrieving active campaigns:");
            var campaigns = await GetCampaigns(end, party, "Legislative");
            var active = campaigns.Where(i => (i.primary_election_status == "Qualified for general" || i.primary_election_status == "Unopposed in primary"));
            foreach (var campaign in active)
                await MineDonors(start, end, campaign.filer_name, campaign.filer_id);
        }

        static async Task MineCaucusPacDonors(short start, short end)
        {
            await MineDonors(start, end, "HROC", "HOUSRO%20507");
            await MineDonors(start, end, "Reagan Fund", "REAGF%20%20507");
            await MineDonors(start, end, "HDCC", "HOUSDC%20507");
            await MineDonors(start, end, "Harry Truman Fund", "HARRTF%20506");
            await MineDonors(start, end, "SRCC", "SENARC%20148");
            await MineDonors(start, end, "Leadership Council", "LEADC%20%20148");
            await MineDonors(start, end, "SDCC", "SENADC%20507");
            await MineDonors(start, end, "SDCC2", "WASHSD%20101");
            await MineDonors(start, end, "Kennedy Fund", "HARRTF%20506");
        }

        static async Task MineCaucusPacExpenses(short start, short end)
        {
            await MineExpenses(start, end, "HROC", "HOUSRO%20507");
            await MineExpenses(start, end, "Reagan Fund", "REAGF%20%20507");
            await MineExpenses(start, end, "HDCC", "HOUSDC%20507");
            await MineExpenses(start, end, "Harry Truman Fund", "HARRTF%20506");
            await MineExpenses(start, end, "SRCC", "SENARC%20148");
            await MineExpenses(start, end, "Leadership Council", "LEADC%20%20148");
            await MineExpenses(start, end, "SDCC", "SENADC%20507");
            await MineExpenses(start, end, "SDCC2", "WASHSD%20101");
            await MineExpenses(start, end, "Kennedy Fund", "HARRTF%20506");
        }

        static async Task MineDonors(short start, short end, string desc, string filer)
        {
            var contributions = new List<Contribution>();

            Console.WriteLine($"Mining donors for {desc}:");
            for (var year = end; year >= start; year--)
            {
                Console.WriteLine($"  Analyzing donations for {year}...");
                contributions.AddRange(await pdc.GetContributions(year, filer));
            }
            Console.WriteLine();

            var donors = AggregateTransactionsByDonor(contributions);
            await GenerateDonorReport(donors, start, end, desc);
        }

        static async Task MineExpenses(short start, short end, string desc, string filer)
        {
            var expenses = new List<Expenditure>();

            Console.WriteLine($"Mining expenses for {desc}:");
            for (var year = end; year >= start; year--)
            {
                Console.WriteLine($"  Analyzing expenditures for {year}...");
                expenses.AddRange(await pdc.GetExpenditures(year, filer));
            }
            Console.WriteLine();

            var recipients = AggregateTransactionsByDonor(expenses);
            await GenerateDonorReport(recipients, start, end, desc);
        }

        static async Task MineDonorsByType(short start, short end, string desc, string entityType = null, string jurisdictionType = null, string legislativeDistrict = null, string contributionType = null)
        {
            var contributions = new List<Contribution>();

            Console.WriteLine($"Mining donors for {desc}:");
            for (var year = end; year >= start; year--)
            {
                Console.WriteLine($"  Analyzing donations for {year}...");
                contributions.AddRange(await GetContributions(year, entityType, jurisdictionType, legislativeDistrict, contributionType));
            }
            Console.WriteLine();

            var donors = AggregateTransactionsByDonor(contributions);
            await GenerateDonorReport(donors, start, end, desc, true);

            Console.WriteLine();
        }

        static async Task MineCampaignsRaisedByType(short start, short end, string entityType = null, string jurisdictionType = null)
        {
            var contributions = new List<Contribution>();

            Console.WriteLine($"Mining campaign contributions:");
            for (var year = end; year >= start; year--)
            {
                Console.WriteLine($"  Analyzing donations for {year}...");
                contributions.AddRange(await GetContributions(year, entityType, jurisdictionType));
            }
            Console.WriteLine();

            var campaigns = AggregateTransactionsByCampaign(contributions);
            GenerateFilerReport(campaigns, start, end, entityType + "Raised by Type");

            Console.WriteLine();
        }

        static async Task<List<Contribution>> GetContributions(short year, string entityType, string jurisdictionType, string legislativeDistrict = null, string contributionType = null)
        {
            var result = await pdc.GetContributionsByType(year, entityType, jurisdictionType, legislativeDistrict, contributionType);
            Console.WriteLine($"    {jurisdictionType ?? entityType ?? "All"}: {result.Count}");
            return result;
        }

        static async Task<List<Committee>> GetCampaigns(short year, string party, string jurisdictionType)
        {
            var result = await pdc.GetCommittees(year, party, null, jurisdictionType);
            Console.WriteLine($"    {jurisdictionType ?? party ?? "All"}: {result.Count}");
            return result;
        }

        static List<Entity> AggregateTransactionsByDonor(IEnumerable<Transaction> transactions)
        {
            var results = new List<Entity>();
            var count = transactions.Count();
            int i = 0;

            // Load up our dictinoary of canonized names
            var wordParser = new KeywordParser("Data/Aliases.csv");

            using (var progress = new ProgressBar())
            {
                Console.Write("Aggregating transactions...");
                foreach (var item in transactions)
                {
                    var words = wordParser.Parse(item.Description);
                    if (words.Length > 0)
                    {
                        var entity = BestEntityMatch(results, words, 0.8);
                        if (entity == null)
                        {
                            // If no good entity match, create a new one
                            entity = new Entity
                            {
                                Keywords = words,
                                Transactions = new List<Transaction>()
                            };
                            results.Add(entity);
                        }
                        entity.Transactions.Add(item);
                    }
                    progress.Report((double)++i / count);
                }
                Console.WriteLine("Done.\n");
#if DEBUG
                DumpEntities(results);
#endif
            }

            return results;
        }

        static List<Entity> AggregateTransactionsByCampaign(IEnumerable<Transaction> transactions)
        {
            var results = new List<Entity>();

            var groups = transactions.GroupBy(i => i.filer_id);
            foreach (var group in groups)
                results.Add(new Entity { Transactions = group.ToList() });

            return results;
        }

        static Entity BestEntityMatch(IList<Entity> entities, string[] words, double threshold)
        {
            Entity result = null;
            double max = 0;
            foreach (var entity in entities)
            {
                var match = entity.Keywords.KeywordMatch(words);
                if (match >= 1.0)
                    return entity;
                else if (match > max)
                {
                    // If we don't find an exact match, keep track of the best match
                    max = match;
                    result = entity;
                }
            }

            // Return best match only if it meets threshold
            return max >= threshold ? result : null;
        }

        static IList<Entity> FilterTransactions(IEnumerable<Transaction> transactions, string description, double threshold)
        {
            // Load up our dictinoary of canonized names
            var wordParser = new KeywordParser("Data/Aliases.csv");
            var keywords = wordParser.Parse(description);

            var matches = new List<Transaction>();
            Console.WriteLine("Filtering transactions...");
            foreach (var item in transactions)
            {
                if (String.IsNullOrWhiteSpace(item.Name))
                    continue;

                var words = wordParser.Parse(item.Name);
                if (words.Length > 0 && words.KeywordMatch(keywords) >= threshold)
                    matches.Add(item);
            }

            var results = new List<Entity>();
            Console.WriteLine("Aggregating transactions...");
            var groups = matches.GroupBy(i => i.filer_id);
            foreach (var group in groups)
            {
                var entity = new Entity
                {
                    Keywords = keywords,
                    Transactions = group.ToList()
                };
                results.Add(entity);
            }

#if DEBUG
            DumpEntities(results);
#endif
            return results;
        }

        static void DumpEntities(IList<Entity> entities)
        {
            Console.WriteLine($"Dumping aggregated entities...");
            using (var sw = new StreamWriter("Entities.txt"))
            {
                foreach (var entity in entities.OrderByDescending(i => i.Transactions.Count))
                {
                    sw.Write($"{entity.Transactions.MostCommon(i => i.Name)} ({entity.Transactions.Count}), |");
                    foreach (var word in entity.Keywords)
                        sw.Write($"{word}|");
                    sw.WriteLine();
                    foreach (var transaction in entity.Transactions)
                        sw.WriteLine($" - {transaction.id}: {transaction.Name}, {transaction.Address} {transaction.Zip}");
                }
            }
            Console.WriteLine();
        }

        static async Task GenerateDonorReport(IList<Entity> entities, short start, short end, string desc, bool analysis = false)
        {
            Console.WriteLine($"Generating report...");
            var dt = NewDataTable("Name", entities.Select(i => i.Transactions.MostCommon(j => j.Name)));
            dt.CreateColumn("Type").Values = entities.Select(i => i.Transactions.MostCommon(j => j.code)).ToArray();
            dt.CreateColumn("Count").Values = entities.Select(i => $"{i.Transactions.Count()}").ToArray();
            for (var year = start; year <= end; year++)
            {
                var values = entities.Select(i => i.Transactions.Where(j => j.election_year == year).Sum(k => k.amount));
                if (values.Sum() > 0)
                    dt.CreateColumn($"{year}").Values = values.Select(i => $"{i:C}").ToArray();
            }
            dt.CreateColumn("Total").Values = entities.Select(i => $"{i.Transactions.Sum(j => j.amount):C}").ToArray();

            if (analysis)
            {
                var campaigns = new List<Committee>();

                for (var year = end; year >= start; year--)
                {
                    // We're only looking at partisan races
                    campaigns.AddRange(await pdc.GetCommittees(year, null, null, "Legislative"));
                    campaigns.AddRange(await pdc.GetCommittees(year, null, null, "Statewide"));
                }

                dt.CreateColumn("Legislative").Values = entities.Select(i => JurisdictionTotal(i.Transactions, "Legislative")).ToArray();
                dt.CreateColumn("Statewide").Values = entities.Select(i => JurisdictionTotal(i.Transactions, "Statewide")).ToArray();
                dt.CreateColumn("Republican").Values = entities.Select(i => PartyTotal(i.Transactions, "REPUBLICAN")).ToArray();
                dt.CreateColumn("Democrat").Values = entities.Select(i => PartyTotal(i.Transactions, "DEMOCRATIC")).ToArray();
                dt.CreateColumn("Campaigns").Values = entities.Select(i => CampaignCount(i.Transactions, campaigns)).ToArray();
                dt.CreateColumn("Wins").Values = entities.Select(i => CampaignCount(i.Transactions, campaigns, "Won")).ToArray();
                dt.CreateColumn("Unopposed").Values = entities.Select(i => CampaignCount(i.Transactions, campaigns, "Unopposed")).ToArray();
            }

            dt.SaveCSV($"Output/{desc} ({start}-{end % 100}).csv");
            Console.WriteLine();
        }

        static void GenerateFilerReport(IList<Entity> entities, short start, short end, string desc)
        {
            string[] codes = { "Individual", "Business", "Union", "Political Action Committee", "Party", "Caucus", "Self", "Other" };

            Console.WriteLine($"Generating report...");
            var dt = NewDataTable("Campaign", entities.Select(i => i.Transactions.First().filer_name));
            dt.CreateColumn("Type").Values = entities.Select(i => i.Transactions.First().jurisdiction_type).ToArray();
            dt.CreateColumn("Pty").Values = entities.Select(i => i.Transactions.First().party).ToArray();
            dt.CreateColumn("Count").Values = entities.Select(i => $"{i.Transactions.Count()}").ToArray();
            for (var year = start; year <= end; year++)
            {
                var values = entities.Select(i => i.Transactions.Where(j => j.election_year == year).Sum(k => k.amount));
                if (values.Sum() > 0)
                    dt.CreateColumn($"{year}").Values = values.Select(i => $"{i:C}").ToArray();
            }
            foreach (var code in codes)
            {
                var values = entities.Select(i => i.Transactions.Where(j => j.code == code).Sum(k => k.amount));
                if (values.Sum() > 0)
                    dt.CreateColumn($"{code}").Values = values.Select(i => $"{i:C}").ToArray();
            }
            dt.CreateColumn("Total").Values = entities.Select(i => $"{i.Transactions.Sum(j => j.amount):C}").ToArray();

            dt.SaveCSV($"Output/{desc} ({start}-{end % 100}).csv");
            Console.WriteLine();
        }

        static string JurisdictionTotal(IList<Transaction> transactions, string jurisdictionType)
            => $"{transactions.Where(i => i?.jurisdiction_type == jurisdictionType).Sum(j => j.amount):C}";

        static string PartyTotal(IList<Transaction> transactions, string party)
            => $"{transactions.Where(i => i?.party == party).Sum(j => j.amount):C}";

        static string CampaignCount(IList<Transaction> transactions, IList<Committee> committees, string status = null)
        {
            var campaigns = committees.Join(transactions, c => new { c.filer_id, c.election_year }, t => new { t.filer_id, t.election_year }, (c, t) => c).Distinct().ToList();
            return campaigns.Count(i => status == null || (i.general_election_status?.StartsWith(status) ?? false)).ToString();
        }

        static MutableDataTable NewDataTable(string colName, IEnumerable<string> values)
        {
            var col = new Column(colName, values.Count());
            col.Values = values.ToArray();

            return new MutableDataTable { Columns = new[] { col } };
        }
    }
}
