using System;
using System.Collections.Generic;
using System.Linq;

namespace CrestApps.RetsSdk.Models
{
    public class SearchRequest
    {
        public string SearchType { get; set; }
        public string Class { get; set; }
        public string QueryType { get; set; } = "DMQL2";
        public int Count { get; set; } = 0;
        public string Format { get; set; } = "COMPACT-DECODED"; // COMPACT-DECODED
        public string RestrictedIndicator { get; set; } = "****";
        public int Limit { get; set; } = int.MaxValue;
        public int StandardNames { get; set; } = 0;
        public QueryParameterGroup ParameterGroup { get; set; }
        private List<string> Columns = new List<string>();

        public SearchRequest()
        {
            ParameterGroup = new QueryParameterGroup();
        }

        public SearchRequest(string resourceName, string className)
            : this()
        {
            SearchType = resourceName;
            Class = className;
        }

        public void AddColumn(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName) || Columns.Contains(columnName, StringComparer.CurrentCultureIgnoreCase))
            {
                return;
            }

            Columns.Add(columnName);
        }

        public void AddColumns(IEnumerable<string> columnNames)
        {
            if (columnNames == null)
            {
                return;
            }

            foreach (var columnName in columnNames)
            {
                AddColumn(columnName);
            }
        }

        public void RemoveColumn(string columnName)
        {
            Columns = Columns.Where(x => x != columnName).ToList();
        }

        public void RemoveColumns(IEnumerable<string> columnNames)
        {
            Columns = Columns.Where(x => !columnNames.Contains(x)).ToList();
        }

        public bool HasColumns()
        {
            return Columns.Any();
        }


        public bool HasColumn(string columnName)
        {
            bool exists = Columns.Any(x => x.Equals(columnName, StringComparison.CurrentCultureIgnoreCase));

            return exists;
        }

        public IEnumerable<string> GetColumns()
        {
            return Columns.Distinct();
        }
    }
}
