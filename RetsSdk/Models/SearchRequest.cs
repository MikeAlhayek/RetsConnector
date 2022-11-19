using JetBrains.Annotations;
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
        public CountType Count { get; set; } = CountType.NoCount;
        public string Format { get; set; } = "COMPACT-DECODED"; // COMPACT-DECODED
        public string RestrictedIndicator { get; set; } = "****";
        public int Limit { get; set; } = int.MaxValue;
        public int Offset { get; set; } = 1;
        
        public int StandardNames { get; set; } = 0;

        [CanBeNull] 
        public string RawQuery { get; set; } = null;

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

        public bool HasColumns() => Columns.Any();
        
        public bool HasColumn(string columnName)
        {
            var exists = Columns.Any(x => x.Equals(columnName, StringComparison.CurrentCultureIgnoreCase));
            return exists;
        }

        public IEnumerable<string> GetColumns() => Columns.Distinct();

        public SearchRequest Clone()
        {
            var newSr = new SearchRequest()
            {
                SearchType = this.SearchType,
                Class = this.Class,
                QueryType = this.QueryType,
                Count = this.Count,
                Format = this.Format,
                RestrictedIndicator = this.RestrictedIndicator,
                Limit = this.Limit,
                Offset = this.Offset,
                StandardNames = this.StandardNames,
                RawQuery = this.RawQuery,
                ParameterGroup = this.ParameterGroup,
            };
            newSr.AddColumns(this.Columns);
            return newSr;
        }
    }

    public enum CountType
    {
        NoCount = 0,
        CountWithData = 1,
        OnlyCount = 2
    }

}
