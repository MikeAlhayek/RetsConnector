using CrestApps.RetsSdk.Models.Enums;
using System.Collections.Generic;
using System.Linq;

namespace CrestApps.RetsSdk.Models
{
    public class QueryParameterGroup
    {
        public QueryParameterLogicalOperator LogicalOperator { get; set; }
        public List<QueryParameter> Parameters { get; set; }

        public QueryParameterGroup Group { get; set; }

        public QueryParameterGroup()
        {
            Parameters = new List<QueryParameter>();
        }

        public void AddParameter(params QueryParameter[] parameters)
        {
            if (parameters == null)
            {
                return;
            }

            foreach (var parameter in parameters)
            {
                Parameters.Add(parameter);
            }
        }


        public override string ToString()
        {
            if(Parameters == null || !Parameters.Any())
            {
                return null;
            }

            string glue = ",";
            if(LogicalOperator == QueryParameterLogicalOperator.Or)
            {
                glue = "|";
            }

            var parameters = Parameters.Select(x => x.ToString()).Where(x => !string.IsNullOrWhiteSpace(x));

            string subGroup = Group?.ToString();
            if (!string.IsNullOrWhiteSpace(subGroup))
            {
                return string.Format("({0}{1}{2})", string.Join(glue, parameters), glue, subGroup);
            }

            return string.Format("({0})", string.Join(glue, parameters));
        }
    }
}
