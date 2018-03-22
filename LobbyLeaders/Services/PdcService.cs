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
            if (filer_id != null)
                query += $"&filer_id={filer_id}";
            return await client.SendAsync<List<Expenditure>>(HttpMethod.Get, new Uri(baseUri, $"tijg-9zyp.json?{query}&$limit={limit}"));
        }

        public async Task<List<Contribution>> GetContributions(int year, string filer_id = null)
        {
            var query = $"election_year={year}";
            if (filer_id != null)
                query += $"&filer_id={filer_id}";
            return await client.SendAsync<List<Contribution>>(HttpMethod.Get, new Uri(baseUri, $"kv7h-kjye.json?{query}&$limit={limit}"));
        }

        public async Task<List<Contribution>> GetContributionsByType(int year, string entityType, string jurisdictionType, string contributionType)
        {
            var uri = new Uri(baseUri, $"kv7h-kjye.json?election_year={year}&code={entityType}&jurisdiction_type={jurisdictionType}&cash_or_in_kind={contributionType}&$limit={limit}");
            return await client.SendAsync<List<Contribution>>(HttpMethod.Get, uri);
        }

        public List<Donor> GetDonorsFromContributions(IEnumerable<Contribution> contributions)
        {
            var result = new List<Donor>();
            foreach (var group in contributions.GroupBy(i => i.contributor_name, new FuzzyComparer()).ToList())
            {
                result.Add(new Donor
                {
                    // Pick most frequently used name and address
                    Name = group.MostCommon(i => i.contributor_name),
                    Address = group.MostCommon(i => i.contributor_address),
                    City = group.MostCommon(i => i.contributor_city),
                    State = group.MostCommon(i => i.contributor_state),
                    Zip = group.MostCommon(i => i.contributor_zip),

                    // Tally contribution amounts
                    Count = group.Count(),
                    Total = group.Sum(i => i.amount),
                    Republican = group.Where(i => i.party == "REPUBLICAN").Sum(i => i.amount),
                    Democrat = group.Where(i => i.party == "DEMOCRAT").Sum(i => i.amount),

                    // Save the list of contributions from this donor
                    Contributions = new List<Contribution>(group)
                });
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
