using CrestApps.RetsSdk.Helpers.Extensions;

namespace CrestApps.RetsSdk.Models
{
    public class SearchResultCellValue
    {
        public bool IsPrimaryKeyValue { get; private set; }
        public bool IsRestricted  { get; private set; }
        private string Value { get; set; }

        public SearchResultCellValue(string value)
        {
            Value = value;
        }

        public void SetIsPrimaryKeyValue(bool isPrimaryKeyValue)
        {
            IsPrimaryKeyValue = isPrimaryKeyValue;
        }

        public void SetIsRestricted(bool isRestricted)
        {
            IsRestricted = isRestricted;
        }

        public void SetIsRestricted(string restrectedValue)
        {
            IsRestricted = Value?.Equals(restrectedValue) ?? false;
        }

        public string NullOrValue()
        {
            if(IsNullOrWhiteSpace())
            {
                return null;
            }

            return Value;
        }

        public string EmptyOrValue()
        {
            return Value ?? string.Empty;
        }


        public string Get()
        {
            return Value;
        }

        public string GetTrimmed()
        {
            return Value?.Trim();
        }

        public T Get<T>()
          where T : struct
        {
            return TryCastValue<T>();
        }

        public T? GetNullable<T>()
            where T : struct
        {
            return TryCastValueNullable<T>();
        }



        public T? TryCastValueNullable<T>()
            where T : struct
        {
            if(IsNullOrWhiteSpace())
            {
                return null;
            }

            return TryCastValue<T>();
        }


        public T TryCastValue<T>()
             where T : struct
        {
            object safeValue = typeof(T).GetSafeObject(Value);

            return (T)safeValue;
        }

        public bool IsNull()
        {
            return Value == null;
        }

        public bool IsNullOrEmpty()
        {
            return string.IsNullOrEmpty(Value);
        }

        public bool IsNullOrWhiteSpace()
        {
            return string.IsNullOrWhiteSpace(Value);
        }
    }

}
