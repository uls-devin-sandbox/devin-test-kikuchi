using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TodoApp.E2E.Tests
{
    /// <summary>
    /// INSERT 文のみをサポートした簡易 SQL テストデータパーサーです。
    /// </summary>
    public static class SqlTestDataParser
    {
        /// <summary>
        /// SQL ファイルから INSERT 文を解析し、行データのリストを返します。
        /// </summary>
        public static List<Dictionary<string, string>> ParseFile(string path)
        {
            var text = File.ReadAllText(path);
            return Parse(text);
        }

        /// <summary>
        /// SQL テキストから INSERT 文を解析し、行データのリストを返します。
        /// </summary>
        public static List<Dictionary<string, string>> Parse(string sqlText)
        {
            var rows = new List<Dictionary<string, string>>();
            if (string.IsNullOrWhiteSpace(sqlText))
            {
                return rows;
            }

            var statements = Regex.Split(sqlText, "(?<=;)")
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            foreach (var statement in statements)
            {
                rows.AddRange(ParseInsertStatement(statement));
            }

            return rows;
        }

        private static List<Dictionary<string, string>> ParseInsertStatement(string statement)
        {
            var rows = new List<Dictionary<string, string>>();

            var pattern = new Regex(
                @"INSERT\s+INTO\s+(?<table>\w+)\s*\(\s*(?<columns>[^)]+)\)\s*VALUES\s*(?<values>[^;]+)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var match = pattern.Match(statement);
            if (!match.Success)
            {
                return rows;
            }

            var columns = ParseColumnList(match.Groups["columns"].Value);
            var valuesPart = match.Groups["values"].Value.Trim();

            var valueGroups = SplitValueTuples(valuesPart);
            foreach (var valueGroup in valueGroups)
            {
                var values = ParseValueList(valueGroup);
                if (values.Count != columns.Count)
                {
                    continue;
                }

                var row = new Dictionary<string, string>();
                for (int i = 0; i < columns.Count; i++)
                {
                    row[columns[i]] = values[i];
                }
                rows.Add(row);
            }

            return rows;
        }

        private static List<string> ParseColumnList(string columnList)
        {
            return columnList
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        private static List<string> SplitValueTuples(string valuesPart)
        {
            var groups = new List<string>();
            var current = new StringBuilder();
            int depth = 0;
            bool inString = false;

            foreach (char c in valuesPart)
            {
                if (c == '\'')
                {
                    inString = !inString;
                    current.Append(c);
                    continue;
                }

                if (inString)
                {
                    current.Append(c);
                    continue;
                }

                if (c == '(')
                {
                    depth++;
                    if (depth == 1)
                    {
                        current.Clear();
                        continue;
                    }
                }
                else if (c == ')')
                {
                    depth--;
                    if (depth == 0)
                    {
                        groups.Add(current.ToString().Trim());
                        current.Clear();
                        continue;
                    }
                }

                current.Append(c);
            }

            return groups;
        }

        private static List<string> ParseValueList(string valueList)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            bool inString = false;

            foreach (char c in valueList)
            {
                if (c == '\'')
                {
                    if (inString)
                    {
                        values.Add(current.ToString());
                        current.Clear();
                        inString = false;
                    }
                    else
                    {
                        inString = true;
                    }
                    continue;
                }

                if (inString)
                {
                    current.Append(c);
                    continue;
                }

                if (c == ',')
                {
                    if (current.Length > 0)
                    {
                        values.Add(current.ToString().Trim());
                        current.Clear();
                    }
                    continue;
                }

                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                current.Append(c);
            }

            if (current.Length > 0)
            {
                values.Add(current.ToString().Trim());
            }

            return values;
        }
    }
}
