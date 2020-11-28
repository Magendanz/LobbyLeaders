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

        public async Task<List<Committee>> GetCommittees(short? year, string party = null, string filerType = null, string jurisdictionType = null)
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
                new Uri(baseUri, $"3h9x-7bvm.json?{query}"));
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
                new Uri(baseUri, $"3h9x-7bvm.json?{query}"));
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
    }
}