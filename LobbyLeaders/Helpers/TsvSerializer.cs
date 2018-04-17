using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LobbyList.Helpers
{
    public class TsvSerializer<T> where T : new()
    {
        public static string Serialize(IEnumerable<T> list)
        {
            var fields = typeof(T).GetProperties();
            var sb = new StringBuilder();

            // Grab header names from field list
            sb.AppendLine(String.Join("\t", Array.ConvertAll(fields, f => f.Name)));

            // Process each record
            foreach (var record in list)
            {
                sb.AppendLine(String.Join("\t", Array.ConvertAll(fields, f => Serialize(f.GetValue(record)))));
            }

            return sb.ToString();
        }

        public static string Serialize(object obj)
        {
            // Scrub extra whitespace, including tabs, newlines and returns
            return Regex.Replace(String.Format("{0}", obj), @"\s+", " ").Trim();
        }

        public static async Task SerializeAsync(IEnumerable<T> list, string path)
        {
            using (var sw = new StreamWriter(path))
            {
                await sw.WriteAsync(Serialize(list));
            }
        }

        public static async Task<List<T>> DeserializeAsync(string path)
        {
            using (var sr = new StreamReader(path))
            {
                var result = new List<T>();
                var properties = new List<PropertyInfo>();
                String line;

                // Load the tab-delimited header
                line = await sr.ReadLineAsync();

                // Create field list from header names
                foreach (var item in line.Split('\t'))
                {
                    properties.Add(typeof(T).GetProperty(item));   // Note: Some may not map, which yields null
                }

                // Process each record
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    T record = new T();
                    var items = line.Split('\t');
                    Debug.Assert(items.Length == properties.Count);

                    for (var i = 0; i < items.Length; ++i)
                    {
                        if (!String.IsNullOrWhiteSpace(items[i]) && properties[i] != null)
                        {
                            properties[i].SetValue(record, Convert.ChangeType(items[i], properties[i].PropertyType));
                        }
                    }

                    result.Add(record);
                }

                return result;
            }
        }
    }
}
