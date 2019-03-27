using System;
using System.Collections.Generic;
using System.Text;

using TelemetryAPI.Query;
using Newtonsoft.Json.Linq;

namespace TelemetryRetriever
{
    /// <summary>
    /// Helper class for building queries compatible with the query api
    /// </summary>
    class QBuilder
    {
        const string COMPARISON_TYPE = "comparison";
        const string GROUP_TYPE = "group";

        /// <summary>
        /// Creates a comparison node in a query (e.g. a = "b")
        /// </summary>
        /// <param name="column"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static QuerySpec CreateComparisonNode(string column, string op, object value)
        {
            return new QuerySpec() { Type = COMPARISON_TYPE, Column = column, Operator = op, Value = new JValue(value) };
        }

        /// <summary>
        /// Creates a comparison node in a query -> a = "b"
        /// </summary>
        /// <param name="column"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static QuerySpec CreateComparisonNode(string column, string op, params object[] values)
        {
            var qn = new QuerySpec() { Type = COMPARISON_TYPE, Column = column, Operator = op, Values = new JValue[values.Length] };

            for (int i = 0; i < values.Length; i++)
            {
                qn.Values[i] = new JValue(values[i]);
            }

            return qn;
        }

        /// <summary>
        /// Creates a grouping node which can include other grouping nodes or comparison nodes -> ((a = "b" OR c = 1) AND d = 2) 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static QuerySpec CreateGroupNode(string op, params QuerySpec[] nodes)
        {
            return new QuerySpec() { Type = GROUP_TYPE, Operator = op, Children = new List<QuerySpec>(nodes) };
        }

        /// <summary>
        /// Column equals value
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static QuerySpec Eq(string column, object value)
        {
            return CreateComparisonNode(column, QueryOp.Eq, value);
        }

        /// <summary>
        /// Column greater than or equals value
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static QuerySpec Gt(string column, object value)
        {
            return CreateComparisonNode(column, QueryOp.Gt, value);
        }

        /// <summary>
        /// Column greater than value
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static QuerySpec Gte(string column, object value)
        {
            return CreateComparisonNode(column, QueryOp.Gte, value);
        }

        /// <summary>
        /// Column less than value
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static QuerySpec Lt(string column, object value)
        {
            return CreateComparisonNode(column, QueryOp.Lt, value);
        }

        /// <summary>
        /// Column less than or equals value
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static QuerySpec Lte(string column, object value)
        {
            return CreateComparisonNode(column, QueryOp.Lte, value);
        }

        /// <summary>
        /// Column does not equal value
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static QuerySpec Neq(string column, object value)
        {
            return CreateComparisonNode(column, QueryOp.Neq, value);
        }

        /// <summary>
        /// Column in values
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static QuerySpec In(string column, params object[] values)
        {
            return CreateComparisonNode(column, QueryOp.In, values);
        }

        /// <summary>
        /// Column between values
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static QuerySpec Btwn(string column, object first, object last)
        {
            return CreateComparisonNode(column, QueryOp.Btwn, first, last);
        }

        /// <summary>
        /// And grouping of all child nodes
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static QuerySpec And(params QuerySpec[] nodes)
        {
            return CreateGroupNode(QueryOp.And, nodes);
        }

        /// <summary>
        /// Or grouping of all child nodes
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static QuerySpec Or(params QuerySpec[] nodes)
        {
            return CreateGroupNode(QueryOp.Or, nodes);
        }
    }
}
