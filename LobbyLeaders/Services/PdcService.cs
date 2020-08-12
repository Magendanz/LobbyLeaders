using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LobbyLeaders.Helpers;
using LobbyLeaders.Models;

namespace LobbyLeaders.Services
{
    public class PdcService
    {
        readonly HttpClient client = new HttpClient();
        readonly Uri baseUri = new Uri("https://data.wa.gov/resource/");
        const int limit = 262144;

        public async Task<List<Agent>> GetAgents(int? year = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"&employment_year={year}";
            return await client.SendAsync<List<Agent>>(HttpMethod.Get,
                new Uri(baseUri, $"bp5b-jrti.json?{query}"));
        }

        public async Task<List<Employer>> GetEmployers(int? year = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"employment_year={year}";
            return await client.SendAsync<List<Employer>>(HttpMethod.Get,
                new Uri(baseUri, $"e7sd-jbuy.json?{query}"));
        }

        public async Task<List<EmployerSum>> GetEmployerSummaries(int? year = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"Year={year}";
            return await client.SendAsync<List<EmployerSum>>(HttpMethod.Get,
                new Uri(baseUri, $"biux-xiwe.json?{query}"));
        }

        public async Task<List<Lobbyist>> GetLobbyListFromAgents(IEnumerable<Agent> agents, IEnumerable<Donor> donors)
        {
            var employers = (await GetEmployers()).ToList();
            var result = new List<Lobbyist>();
            var index = 2;
            foreach (var agent in agents.OrderBy(i => i.lobbyist_firm_name + i.agent_name).ToList())
            {
                var lobbyist = new Lobbyist
                {
                    Name = agent.agent_name.ToTitleCase(),
                    Firm = agent.lobbyist_firm_name.ToTitleCase(),
                    Phone = agent.lobbyist_phone.ToPhoneNumber(),
                    Email = agent.lobbyist_email.ToLowerInvariant(),
                    Address = agent.lobbyist_address,
                    Employer = agent.employers
                };
                result.Add(lobbyist);
                var clients = employers.Where(i => i.agent_name == agent.agent_name && i.employment_year == agent.employment_year).OrderBy(i => i.employer_title).ToList();
                if (clients.Count > 1)
                {
                    foreach (var employer in clients)
                    {
                        ++index;
                        result.Add(new Lobbyist
                        {
                            Employer = employer.employer_title,
                            DonorId = BestDonorMatch(employer.employer_title, donors),
                            Donor = $"=VLOOKUP(G{index},Donors!A$2:J$10513,2,FALSE)",
                            Total = $"=VLOOKUP(G{index},Donors!A$2:J$10513,8,FALSE)",
                            Republican = $"=VLOOKUP(G{index},Donors!A$2:J$10513,9,FALSE)",
                            Democrat = $"=VLOOKUP(G{index},Donors!A$2:J$10513,10,FALSE)"
                        });
                    }
                    lobbyist.Total = $"=SUM(I{index - clients.Count + 1}:I{index})";
                    lobbyist.Republican = $"=SUM(J{index - clients.Count + 1}:J{index})";
                    lobbyist.Democrat = $"=SUM(K{index - clients.Count + 1}:K{index})";
                }
                else
                {
                    var employer = clients.First();
                    lobbyist.Employer = employer.employer_title;
                    lobbyist.DonorId = BestDonorMatch(employer.employer_title, donors);
                    lobbyist.Donor = $"=VLOOKUP(G{index},Donors!A$2:J$10513,2,FALSE)";
                    lobbyist.Total = $"=VLOOKUP(G{index},Donors!A$2:J$10513,8,FALSE)";
                    lobbyist.Republican = $"=VLOOKUP(G{index},Donors!A$2:J$10513,9,FALSE)";
                    lobbyist.Democrat = $"=VLOOKUP(G{index},Donors!A$2:J$10513,10,FALSE)";
                }
                ++index;
            }

            return result;
        }

        public int BestDonorMatch(string str, IEnumerable<Donor> donors)
        {
            var match = donors.OrderByDescending(i => str.KeywordMatch(i.Name)).First();
            var strength = str.KeywordMatch(match.Name);

            return strength > 0.8 ? match.Id : 0;   // Adjust fuzzy match threshold here!
        }

        public async Task<List<Committee>> GetCommittees(short? year, string party, string filerType = null, string jurisdictionType = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"&election_year={year}";
            if (!String.IsNullOrWhiteSpace(party))
                query += $"&party_code={party}";
            if (!String.IsNullOrWhiteSpace(filerType))
                query += $"&filer_type={filerType}";
            if (!String.IsNullOrWhiteSpace(jurisdictionType))
                query += $"&jurisdiction_type={jurisdictionType}";
            return await client.SendAsync<List<Committee>>(HttpMethod.Get,
                new Uri(baseUri, $"d27u-zvri.json?{query}"));
        }

        public async Task<List<Committee>> GetCommittees(string first, string last, string jurisdictionType = null)
        {
            var query = $"$limit={limit}";
            if (!String.IsNullOrWhiteSpace(first))
                query += $"&first_name={first}";
            if (!String.IsNullOrWhiteSpace(last))
                query += $"&last_name={last}";
            if (!String.IsNullOrWhiteSpace(jurisdictionType))
                query += $"&jurisdiction_type={jurisdictionType}";
            return await client.SendAsync<List<Committee>>(HttpMethod.Get,
                new Uri(baseUri, $"d27u-zvri.json?{query}"));
        }

        public async Task<List<Expenditure>> GetExpenditures(short? year = null, string filer_id = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"&election_year={year}";
            if (!String.IsNullOrWhiteSpace(filer_id))
                query += $"&filer_id={filer_id}";
            return await client.SendAsync<List<Expenditure>>(HttpMethod.Get,
                new Uri(baseUri, $"tijg-9zyp.json?{query}"));
        }

        public async Task<List<Expenditure>> GetExpensesByType(short? year = null, string entityType = null, string expenseCode = null, string jurisdictionType = null, string legislativeDistrict = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"&election_year={year}";
            if (!String.IsNullOrWhiteSpace(entityType))
                query += $"&type={entityType}";
            if (!String.IsNullOrWhiteSpace(expenseCode))
                query += $"&code={expenseCode}";
            if (!String.IsNullOrWhiteSpace(jurisdictionType))
                query += $"&jurisdiction_type={jurisdictionType}";
            if (!String.IsNullOrWhiteSpace(legislativeDistrict))
                query += $"&legislative_district={legislativeDistrict}";

            return await client.SendAsync<List<Expenditure>>(HttpMethod.Get,
                new Uri(baseUri, $"tijg-9zyp.json?{query}"));
        }

        public async Task<List<Contribution>> GetContributions(short? year = null, string filer_id = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"&election_year={year}";
            if (!String.IsNullOrWhiteSpace(filer_id))
                query += $"&filer_id={filer_id}";
            return await client.SendAsync<List<Contribution>>(HttpMethod.Get,
                new Uri(baseUri, $"kv7h-kjye.json?{query}"));
        }

        public async Task<List<Contribution>> GetContributionsByType(short? year = null, string entityType = null, string jurisdictionType = null, string legislativeDistrict = null, string contributionType = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"&election_year={year}";
            if (!String.IsNullOrWhiteSpace(entityType))
                query += $"&code={entityType}";
            if (!String.IsNullOrWhiteSpace(jurisdictionType))
                query += $"&jurisdiction_type={jurisdictionType}";
            if (!String.IsNullOrWhiteSpace(legislativeDistrict))
                query += $"&legislative_district={legislativeDistrict}";
            if (!String.IsNullOrWhiteSpace(contributionType))
                query += $"&cash_or_in_kind={contributionType}";
            return await client.SendAsync<List<Contribution>>(HttpMethod.Get,
                new Uri(baseUri, $"kv7h-kjye.json?{query}"));
        }

        public List<Donor> GetDonorsFromContributions(IEnumerable<Contribution> contributions)
        {
            var donors = new List<Donor>();
            var count = contributions.Count();
            int i = 0, j = 0;

            // Load up my dictinoary of canonized names
            var table = CsvSerializer<string>.Deserialize("Data/Addresses.csv");
            table.AddRange(CsvSerializer<string>.Deserialize("Data/Nicknames.csv"));
            table.AddRange(CsvSerializer<string>.Deserialize("Data/Organizations.csv"));
            var dict = table.ToDictionary(i => i[0], i => i[1]);

            using (var progress = new ProgressBar())
            {
                foreach (var item in contributions)
                {
                    var desc = String.Join(' ', item.contributor_name, item.contributor_address, item.contributor_zip);
                    var words = KeywordParser.Parse(desc).Canonize(dict);
                    var donor = BestDonorMatch(donors, words, 0.8);
                    if (donor == null)
                    {
                        donor = new Donor
                        {
                            Id = ++i,       // Auto-increment primary key
                            Name = item.contributor_name,
                            Address = item.contributor_address,
                            Zip = item.contributor_zip,
                            Keywords = words,
                            Contributions = new List<Contribution>()
                        };
                        donors.Add(donor);
                    }
                    donor.Contributions.Add(item);
                    progress.Report((double) ++j / count);
                }

                foreach (var item in donors)
                {
                    // Pick most frequently used contributor properties
                    item.Name = item.Contributions.MostCommon(i => i.contributor_name);
                    item.Address = item.Contributions.MostCommon(i => i.contributor_address);
                    item.City = item.Contributions.MostCommon(i => i.contributor_city);
                    item.State = item.Contributions.MostCommon(i => i.contributor_state);
                    item.Zip = item.Contributions.MostCommon(i => i.contributor_zip);
                    item.Type = item.Contributions.MostCommon(i => i.code);
                }
            }

            return donors;
        }

        private Donor BestDonorMatch(IEnumerable<Donor> donors, string[] keywords, double threshold)
        {
            Donor result = null;
            double max = 0;
            foreach(var donor in donors)
            {
                var match = donor.Keywords.KeywordMatch(keywords);
                if (match > max)
                {
                    max = match;
                    result = donor;
                }
            }

            return max > threshold ? result: null;
        }

        public List<Recipient> GetRecipientsFromExpenses(IEnumerable<Expenditure> expenses)
        {
            var recipients = new List<Recipient>();
            var count = expenses.Count();
            int i = 0, j = 0;

            // Load up my dictinoary of canonized names
            var table = CsvSerializer<string>.Deserialize("Data/Addresses.csv");
            table.AddRange(CsvSerializer<string>.Deserialize("Data/Nicknames.csv"));
            table.AddRange(CsvSerializer<string>.Deserialize("Data/Organizations.csv"));
            var dict = table.ToDictionary(i => i[0], i => i[1]);

            using (var progress = new ProgressBar())
            {
                foreach (var item in expenses)
                {
                    var desc = String.Join(' ', item.recipient_name, item.recipient_address, item.recipient_zip);
                    var words = KeywordParser.Parse(desc).Canonize(dict);
                    var recipient = recipients.FirstOrDefault(r => r.Keywords.KeywordMatch(words) > 0.8);
                    if (recipient == null)
                    {
                        recipient = new Recipient
                        {
                            Id = ++i,       // Auto-increment primary key
                            Name = item.recipient_name,
                            Address = item.recipient_address,
                            Zip = item.recipient_zip,
                            Keywords = words,
                            Payments = new List<Expenditure>()
                        };
                        recipients.Add(recipient);
                    }
                    recipient.Payments.Add(item);
                    progress.Report((double) ++j / count);
                }

                foreach (var item in recipients)
                {
                    // Pick most frequently used contributor properties
                    item.Name = item.Payments.MostCommon(i => i.recipient_name);
                    item.Address = item.Payments.MostCommon(i => i.recipient_address);
                    item.City = item.Payments.MostCommon(i => i.recipient_city);
                    item.State = item.Payments.MostCommon(i => i.recipient_state);
                    item.Zip = item.Payments.MostCommon(i => i.recipient_zip);
                    item.Type = item.Payments.MostCommon(i => i.code);
                }
            }

            return recipients;
        }

        public List<Tally> GetDonorTotals(IEnumerable<Donor> donors, IEnumerable<Committee> committees,
            short year = 0, string jurisdictionType = null, string contributionType = null)
        {
            var totals = new List<Tally>();
            foreach (var donor in donors)
            {
                var contributions = donor.Contributions.Where(i => (year == 0 || i.election_year == year)
                && (String.IsNullOrWhiteSpace(jurisdictionType) || i.jurisdiction_type == jurisdictionType)
                && (String.IsNullOrWhiteSpace(contributionType) || i.cash_or_in_kind == contributionType)).ToList();

                var count = contributions.Count();
                if (count > 0)
                {
                    var filers = contributions.Select(i => i.filer_id).Distinct();

                    totals.Add(new Tally
                    {
                        Id = donor.Id,
                        Year = year,
                        Jurisdiction = jurisdictionType,
                        Contributions = contributions.Count(),
                        Campaigns = filers.Count(),
                        Wins = filers.Count(i => Status(i, committees, "Won")),
                        Unopposed = filers.Count(i => Status(i, committees, "Unopposed")),
                        Total = contributions.Sum(i => i.amount),
                        Republican = contributions.Where(i => i.party == "REPUBLICAN").Sum(i => i.amount),
                        Democrat = contributions.Where(i => i.party == "DEMOCRAT").Sum(i => i.amount)
                    });
                }
            }

            return totals;
        }

        public List<Tally> GetRecipientTotals(IEnumerable<Recipient> recipients, IEnumerable<Committee> committees)
        {
            var totals = new List<Tally>();
            foreach (var recipient in recipients)
            {
                var count = recipient.Payments.Count();
                if (count > 0)
                {
                    var filers = recipient.Payments.Select(i => i.filer_id).Distinct();

                    totals.Add(new Tally
                    {
                        Id = recipient.Id,
                        Jurisdiction = "Partisan",
                        Contributions = recipient.Payments.Count(),
                        Campaigns = filers.Count(),
                        Wins = filers.Count(i => Status(i, committees, "Won")),
                        Unopposed = filers.Count(i => Status(i, committees, "Unopposed")),
                        Total = recipient.Payments.Sum(i => i.amount),
                        Republican = recipient.Payments.Where(i => i.party == "REPUBLICAN").Sum(i => i.amount),
                        Democrat = recipient.Payments.Where(i => i.party == "DEMOCRAT").Sum(i => i.amount)
                    });
                }
            }

            return totals;
        }

        private bool Status(string filerId, IEnumerable<Committee> committees, string value)
        {
            var campaign = committees.FirstOrDefault(i => i.filer_id == filerId);
            return campaign?.general_election_status?.StartsWith(value) ?? false;
        }
    }
}