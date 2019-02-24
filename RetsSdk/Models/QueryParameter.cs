namespace CrestApps.RetsSdk.Models
{
    public class QueryParameter
    {
        public string FieldName { get; set; }
        public string Value { get; set; }

        public QueryParameter()
        {

        }

        public QueryParameter(string fieldName, string value)
        {
            FieldName = fieldName;
            Value = value;
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(FieldName))
            {
                return string.Empty;
            }

            return string.Format("({0}={1})", FieldName, Value);
        }
    }
}
