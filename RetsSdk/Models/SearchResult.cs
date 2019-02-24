using System.Collections.Generic;
using System.Linq;
using System;

namespace CrestApps.RetsSdk.Models
{
    public class SearchResult
    {
        public RetsResource Resource { get; private set; }
        public string ClassName { get; private set; }
        private string RestrictedValue;
        private string[] Columns { get; set; }
        private Dictionary<string, SearchResultRow> Rows { get; set; }

        public SearchResult(RetsResource resource, string className, string restrictedValue)
        {
            Columns = new string[] { };
            Rows = new Dictionary<string, SearchResultRow>();
            Resource = resource ?? throw new ArgumentNullException($"{nameof(resource)} cannot be null.");
            ClassName = className ?? throw new ArgumentNullException($"{nameof(className)} cannot be null.");
            RestrictedValue = restrictedValue ?? throw new ArgumentNullException($"{nameof(restrictedValue)} cannot be null.");
        }

        public SearchResultRow GetRow(string primaryKeyValue)
        {
            if(Rows.ContainsKey(primaryKeyValue))
            {
                return Rows[primaryKeyValue];
            }

            return null;
        }

        public bool AddRow(SearchResultRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException($"{nameof(row)} cannot be null.");
            }

            return Rows.TryAdd(row.PrimaryKeyValue, row);
        }

        public bool RemoveRow(string primaryKeyValue)
        {
            if (primaryKeyValue == null)
            {
                throw new ArgumentNullException($"{nameof(primaryKeyValue)} cannot be null.");
            }

            if (Rows.ContainsKey(primaryKeyValue))
            {
                return Rows.Remove(primaryKeyValue);
            }

            return false;
        }
        public bool RemoveRow(SearchResultRow row)
        {
            if(row == null)
            {
                throw new ArgumentNullException($"{nameof(row)} cannot be null.");
            }

            return Rows.Remove(row.PrimaryKeyValue);
        }

        public IEnumerable<SearchResultCellValue> Pluck(string columnName)
        {
            var values = Rows.Select(x => x.Value.Get(columnName));

            return values;
        }

        public IEnumerable<T> Pluck<T>(string columnName) 
            where T : struct
        {
            IEnumerable<T> values = Rows.Select(x => x.Value.Get(columnName).Get<T>());

            return values;
        }

        public IEnumerable<T?> PluckNullable<T>(string columnName)
            where T : struct
        {
            IEnumerable<T?> values = Rows.Select(x => x.Value.Get(columnName).GetNullable<T>());

            return values;
        }

        public IEnumerable<SearchResultRow> GetRows()
        {
            return Rows.Select(x => x.Value);
        }

        public IEnumerable<string> GetColumns()
        {
            return Columns;
        }

        public void SetColumns(string[] columns)
        {
            Columns = columns ?? throw new ArgumentNullException($"{nameof(columns)} cannot be null.");
        }


        public void SetColumns(IEnumerable<string> columns)
        {
            SetColumns(columns?.AsEnumerable());
        }

        public bool IsRestricted(string value)
        {
            return RestrictedValue.Equals(value);
        }

        public int Count()
        {
            return Rows.Count();
        }
    }
}
