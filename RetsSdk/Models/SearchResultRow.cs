using System;
using System.Collections.Generic;

namespace CrestApps.RetsSdk.Models
{
    public class SearchResultRow
    {
        public string PrimaryKeyValue { get; private set; }
        private string RestrictedValue;
        private Dictionary<string, SearchResultCellValue> Values { get; set; } = new Dictionary<string, SearchResultCellValue>();

        public SearchResultRow(string[] columns, string[] values, string primaryKeyColumnName, string restrictedValue)
        {
            if (columns == null)
            {
                throw new ArgumentNullException($"{nameof(columns)} cannot be null.");
            }

            if (values == null)
            {
                throw new ArgumentNullException($"{nameof(values)} cannot be null.");
            }

            if (primaryKeyColumnName == null)
            {
                throw new ArgumentNullException($"{nameof(primaryKeyColumnName)} cannot be null.");
            }
            RestrictedValue = restrictedValue ?? throw new ArgumentNullException($"{nameof(restrictedValue)} cannot be null."); ;

            var columnLength = columns.Length;

            if (columnLength != values.Length)
            {
                throw new ArgumentOutOfRangeException($"Both '{nameof(columns)}' and '{nameof(values)}' must have the same size!");
            }

            int keyIndex = Array.IndexOf(columns, primaryKeyColumnName);
            if (keyIndex == -1)
            {
                throw new IndexOutOfRangeException($"The provided {nameof(primaryKeyColumnName)} is not found in the {nameof(columns)} array.");
            }

            PrimaryKeyValue = values[keyIndex];

            for (int index = 0; index < columnLength; index++)
            {
                string rawValue = values[index];
                SearchResultCellValue value = new SearchResultCellValue(rawValue);

                value.SetIsPrimaryKeyValue(keyIndex == index);
                value.SetIsRestricted(RestrictedValue);

                Values.TryAdd(columns[index].ToLower(), value);
            }

        }

        public bool IsRestricted(string columnName)
        {
            return RestrictedValue.Equals(Get(columnName));
        }

        public SearchResultCellValue Get(string columnName)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException($"{nameof(columnName)} cannot be null.");
            }

            string columnNameLower = columnName.ToLower();
            if (!Values.ContainsKey(columnNameLower))
            {
                return null;
            }

            return Values[columnNameLower];
        }


        public string GetValue(string columnName)
        {
            var cell = Get(columnName);

            return cell?.Get();
        }

        public T? GetValueNullable<T>(string columnName)
            where T : struct
        {
            var cell = Get(columnName);

            return cell?.GetNullable<T>();
        }

        public T GetValue<T>(string columnName)
            where T : struct
        {
            var cell = Get(columnName);

            if(cell == null)
            {
                throw new Exception("Unable to find the provided column");
            }

            return cell.Get<T>();
        }

        public string GetNullOrValue(string columnName)
        {
            var cell = Get(columnName);

            return cell?.NullOrValue();
        }

    }
}
