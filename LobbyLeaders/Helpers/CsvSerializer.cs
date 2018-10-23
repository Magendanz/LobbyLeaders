using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LobbyLeaders.Helpers
{
    public class CsvSerializer<T>
    {
        public static string Serialize(IEnumerable<IEnumerable<T>> table)
        {
            var sb = new StringBuilder();

            foreach (var row in table)
                sb.AppendLine(String.Join(",", row));

            return sb.ToString();
        }

        public static async Task SerializeAsync(IEnumerable<IEnumerable<T>> table, string path)
        {
            using (var sw = new StreamWriter(path))
            {
                await sw.WriteAsync(Serialize(table));
            }
        }

        public static async Task<List<List<T>>> DeserializeAsync(string path)
        {
            using (var sr = new StreamReader(path))
            {
                var result = new List<List<T>>();
                String line;

                while ((line = await sr.ReadLineAsync()) != null)
                {
                    var row = new List<T>();
                    var items = line.Split(',');
                    foreach (var item in items)
                        row.Add((T)Convert.ChangeType(item, typeof(T)));

                    result.Add(row);
                }

                return result;
            }
        }
    }
}
