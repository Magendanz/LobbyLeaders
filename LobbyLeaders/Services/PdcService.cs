using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using LobbyList.Helpers;
using LobbyList.Models;

namespace LobbyList.Services
{
    public class PdcService
    {
        HttpClient client = new HttpClient();
        Uri baseUri = new Uri("https://data.wa.gov/resource/");
        short limit = Int16.MaxValue;

        public async Task<List<Agent>> GetAgents(int year)
        {
            var uri = new Uri(baseUri, $"bp5b-jrti.json?employment_year={year}");
            return await client.SendAsync<List<Agent>>(HttpMethod.Get, uri);
        }

        public async Task<List<Employer>> GetEmployers(int year)
        {
            var uri = new Uri(baseUri, $"e7sd-jbuy.json?employment_year={year}");
            return await client.SendAsync<List<Employer>>(HttpMethod.Get, uri);
        }

        public async Task<List<EmployerSum>> GetEmployerSummaries(int year)
        {
            var uri = new Uri(baseUri, $"biux-xiwe.json?Year={year}");
            return await client.SendAsync<List<EmployerSum>>(HttpMethod.Get, uri);
        }

        public async Task<List<Expenditure>> GetExpenditures(int year, string filer_id = null)
        {
            var query = $"election_year={year}";
            if (!String.IsNullOrWhiteSpace(filer_id))
                query += $"&filer_id={filer_id}";
            return await client.SendAsync<List<Expenditure>>(HttpMethod.Get, new Uri(baseUri, $"tijg-9zyp.json?{query}&$limit={limit}"));
        }

        public async Task<List<Contribution>> GetContributions(int year, string filer_id = null)
        {
            var query = $"election_year={year}";
            if (!String.IsNullOrWhiteSpace(filer_id))
                query += $"&filer_id={filer_id}";
            return await client.SendAsync<List<Contribution>>(HttpMethod.Get, new Uri(baseUri, $"kv7h-kjye.json?{query}&$limit={limit}"));
        }

        public async Task<List<Contribution>> GetContributionsByType(int year, string entityType = null, string jurisdictionType = null, string contributionType = null)
        {
            var query = $"election_year={year}";
            if (!String.IsNullOrWhiteSpace(entityType))
                query += $"&code={entityType}";
            if (!String.IsNullOrWhiteSpace(jurisdictionType))
                query += $"&jurisdiction_type={jurisdictionType}";
            if (!String.IsNullOrWhiteSpace(contributionType))
                query += $"&cash_or_in_kind={contributionType}";
            return await client.SendAsync<List<Contribution>>(HttpMethod.Get, new Uri(baseUri, $"kv7h-kjye.json?{query}&$limit={limit}"));
        }

        public List<Donor> GetDonorsFromContributions(IEnumerable<Contribution> contributions)
        {
            var result = new List<Donor>();
            var index = 1;
            foreach (var group in contributions.GroupBy(i => i.contributor_name, new FuzzyComparer()).ToList())
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

        public List<Score> GetDonorScores(IEnumerable<Donor> donors, int year, string jurisdictionType = null, string contributionType = null)
        {
            var result = new List<Score>();
            foreach (var donor in donors)
            {
                var contributions = donor.Contributions.Where(i => i.election_year == year
                && (String.IsNullOrWhiteSpace(jurisdictionType) || i.jurisdiction_type == jurisdictionType)
                && (String.IsNullOrWhiteSpace(contributionType) || i.cash_or_in_kind == contributionType)).ToList();

                var count = contributions.Count();
                if (count > 0)
                {
                    var total = contributions.Sum(i => i.amount);
                    var dem = contributions.Where(i => i.party == "REPUBLICAN").Sum(i => i.amount);
                    var rep = contributions.Where(i => i.party == "DEMOCRAT").Sum(i => i.amount);

                    result.Add(new Score
                    {
                        Donor = donor.Id,
                        Year = year,
                        Jurisdiction = jurisdictionType,
                        Count = count,
                        Total = total,
                        Bias = rep - dem
                    });
                }
            }

            return result;
        }
    }

    public class FuzzyComparer : IEqualityComparer<string>
    {
        public bool Equals(string strA, string strB) => StringUtilities.FuzzyMatch(strA, strB) > 0.8;   // Adjust fuzzy match threshold here!
        public int GetHashCode(string str) => 0;    // Only executes Equals if two hash codes are equal, so we always return zero
    }
}
