using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using LobbyLeaders.Helpers;
using LobbyLeaders.Models;

namespace LobbyLeaders.Services
{
    public class PdcService
    {
        HttpClient client = new HttpClient();
        readonly Uri baseUri = new Uri("https://data.wa.gov/resource/");
        const short limit = Int16.MaxValue;

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
            var match = donors.OrderByDescending(i => str.FuzzyMatch(i.Name)).First();
            var strength = str.FuzzyMatch(match.Name);

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
            var result = new List<Donor>();
            var index = 1;

            var synonyms = CsvSerializer<string>.DeserializeAsync("Data/States.csv").Result;
            synonyms.AddRange(CsvSerializer<string>.DeserializeAsync("Data/Organizations.csv").Result);
            //synonyms.AddRange(CsvSerializer<string>.DeserializeAsync("/Data/Nicknames.csv").Result);

            var comparer = new FuzzyComparer(0.8);
            foreach (var group in contributions.GroupBy(i => i.contributor_name, comparer).ToList())
            {
                result.Add(new Donor
                {
                    // Create auto-increment primary key
                    Id = index++,

                    // Pick most frequently used contributor properties
                    Name = group.MostCommon(i => i.contributor_name),
                    Address = group.MostCommon(i => i.contributor_address),
                    City = group.MostCommon(i => i.contributor_city),
                    State = group.MostCommon(i => i.contributor_state),
                    Zip = group.MostCommon(i => i.contributor_zip),
                    Type = group.MostCommon(i => i.code),

                    // Save the list of contributions from this donor
                    Contributions = new List<Contribution>(group)
                });
            }

            return result;
        }

        public List<Recipient> GetRecipientsFromExpenses(IEnumerable<Expenditure> expenses)
        {
            var result = new List<Recipient>();
            var index = 1;

            var comparer = new FuzzyComparer(0.8);
            foreach (var group in expenses.GroupBy(i => i.recipient_name, comparer).ToList())
            {
                result.Add(new Recipient
                {
                    // Create auto-increment primary key
                    Id = index++,

                    // Pick most frequently used contributor properties
                    Name = group.MostCommon(i => i.recipient_name),
                    Address = group.MostCommon(i => i.recipient_address),
                    City = group.MostCommon(i => i.recipient_city),
                    State = group.MostCommon(i => i.recipient_state),
                    Zip = group.MostCommon(i => i.recipient_zip),
                    Type = group.MostCommon(i => i.code),

                    // Save the list of contributions from this donor
                    Payments = new List<Expenditure>(group)
                });
            }

            return result;
        }


        public List<Tally> GetDonorSubtotals(IEnumerable<Donor> donors, short year, string jurisdictionType = null, string contributionType = null)
        {
            var result = new List<Tally>();
            foreach (var donor in donors)
            {
                var contributions = donor.Contributions.Where(i => i.election_year == year
                && (String.IsNullOrWhiteSpace(jurisdictionType) || i.jurisdiction_type == jurisdictionType)
                && (String.IsNullOrWhiteSpace(contributionType) || i.cash_or_in_kind == contributionType)).ToList();

                var count = contributions.Count();
                if (count > 0)
                {
                    var total = contributions.Sum(i => i.amount);
                    var rep = contributions.Where(i => i.party == "REPUBLICAN").Sum(i => i.amount);
                    var dem = contributions.Where(i => i.party == "DEMOCRAT").Sum(i => i.amount);

                    result.Add(new Tally
                    {
                        Id = donor.Id,
                        Year = year,
                        Jurisdiction = jurisdictionType,
                        Count = count,
                        Total = total,
                        Republican = rep,
                        Democrat = dem
                    });
                }
            }

            return result;
        }

        public List<Tally> GetDonorTotals(IEnumerable<Donor> donors)
        {
            var result = new List<Tally>();
            foreach (var donor in donors)
            {
                var count = donor.Contributions.Count();
                if (count > 0)
                {
                    var total = donor.Contributions.Sum(i => i.amount);
                    var rep = donor.Contributions.Where(i => i.party == "REPUBLICAN").Sum(i => i.amount);
                    var dem = donor.Contributions.Where(i => i.party == "DEMOCRAT").Sum(i => i.amount);

                    result.Add(new Tally
                    {
                        Id = donor.Id,
                        Jurisdiction = "Partisan",
                        Count = count,
                        Total = total,
                        Republican = rep,
                        Democrat = dem
                    });
                }
            }

            return result;
        }

        public List<Tally> GetRecipientTotals(IEnumerable<Recipient> recipients, IEnumerable<Committee> committees)
        {
            var result = new List<Tally>();
            foreach (var recipient in recipients)
            {
                var count = recipient.Payments.Count();
                if (count > 0)
                {
                    var filers = recipient.Payments.Select(i => i.filer_id).Distinct();

                    result.Add(new Tally
                    {
                        Id = recipient.Id,
                        Jurisdiction = "Partisan",
                        Count = filers.Count(),
                        Wins = filers.Count(i => Won(i, committees)),
                        Total = recipient.Payments.Sum(i => i.amount),
                        Republican = recipient.Payments.Where(i => i.party == "REPUBLICAN").Sum(i => i.amount),
                        Democrat = recipient.Payments.Where(i => i.party == "DEMOCRAT").Sum(i => i.amount)
                    });
                }
            }

            return result;
        }

        private bool Won(string filerId, IEnumerable<Committee> committees)
        {
            var campaign = committees.First(i => i.filer_id == filerId);
            return campaign.general_election_status?.StartsWith("Won") ?? false;  // May want to include "Unopposed" in future
        }
    }


    public class FuzzyComparer : IEqualityComparer<string>
    {
        readonly IEnumerable<IEnumerable<string>> _abbreviations;    // Table of known abbreviations or nicknames
        readonly double _threshold;                                  // Fuzzy match threshold

        public bool Equals(string strA, string strB)
        {
            if (_abbreviations != null)
            {
                var listA = strA.Variations(_abbreviations);
                var listB = strB.Variations(_abbreviations);

                foreach (var i in listA)
                    foreach (var j in listB)
                        if (i.FuzzyMatch(j) > _threshold)
                            return true;

                return false;
            }
            else
                return strA.FuzzyMatch(strB) > _threshold;
        }

        public int GetHashCode(string str) => 0;            // Only executes Equals if two hash codes are equal, so we always return zero

        public FuzzyComparer(double threshold, IEnumerable<IEnumerable<string>> abbreviations = null)
        {
            _abbreviations = abbreviations;
            _threshold = threshold;
        }
    }
}
