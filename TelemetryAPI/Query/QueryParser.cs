using System;
using System.Text;
using System.Linq;

using Newtonsoft.Json.Linq;

namespace TelemetryAPI.Query
{
    /// <summary>
    /// Parses a filter query and produces SQL text to place under the WHERE clause
    /// </summary>
    public class QueryParser
    {

        public QuerySpec Root { get; private set; }
        public string TableName { get; private set; }

        public QueryParser(QuerySpec root, string tableName)
        {
            Root = root;
            TableName = tableName;
        }

        public string Parse()
        {
            StringBuilder sb = new StringBuilder();

            Parse(Root, sb, 0);

            return sb.ToString();
        }

        private void Parse(QuerySpec node, StringBuilder sb, int level)
        {
            switch (node.Type)
            {
                case "group":
                    if (level > 0) sb.Append("(");

                    for (int i = 0; i < node.Children.Count; i++)
                    {
                        var child = node.Children[i];
                        Parse(child, sb, level + 1);
                        if (i + 1 < node.Children.Count)
                        {
                            sb.Append($" {QueryOp.Map[node.Operator]} ");
                        }
                    }

                    if (level > 0) { sb.Append(")"); }
                    break;

                case "comparison":
                    sb.Append(FormatColumn(node.Column));

                    switch (node.Operator)
                    {
                        case QueryOp.In:
                            if (node.Values?.Length > 0)
                            {
                                sb.Append(" IN (");
                                sb.Append(String.Join(", ", node.Values.Select(val => FormatValue(val))));
                                sb.Append(") ");
                            }
                            else
                            {
                                throw new FormatException($"IN clause specified but 'values' field was empty.");
                            }
                            break;

                        case QueryOp.Btwn:
                            if (node.Values?.Length == 2)
                            {
                                sb.Append($" BETWEEN {FormatValue(node.Values[0])} AND {FormatValue(node.Values[1])} ");
                            }
                            else
                            {
                                throw new FormatException($"BETWEEN clause specified but 'values' field did not contain 2 values.");
                            }
                            break;

                        default:
                            sb.Append($" {QueryOp.Map[node.Operator]} {FormatValue(node.Value)}");
                            break;
                    }
                    break;
                default:
                    throw new FormatException($"Unknown node type -> '{Root.Type}'.");
            }
        }

        private string FormatValue(JValue item)
        {
            switch (item.Type)
            {
                case JTokenType.Integer:
                case JTokenType.Float:
                    return item.Value.ToString();
                default:
                    return $"\"{item.Value}\"";
            }
        }

        private string FormatColumn(string column)
        {
            return $"{TableName}.{column}";
        }
    }
}