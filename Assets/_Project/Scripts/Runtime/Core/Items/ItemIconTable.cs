using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace PH.Core.Items
{
    public sealed class ItemIconTable
    {
        private readonly List<ItemIconDefinition> icons = new List<ItemIconDefinition>();
        private readonly Dictionary<string, ItemIconDefinition> iconByKey = new Dictionary<string, ItemIconDefinition>();

        public IReadOnlyList<ItemIconDefinition> Icons => icons;

        public static ItemIconTable Load(TextAsset csvAsset)
        {
            ItemIconTable table = new ItemIconTable();

            if (csvAsset == null)
            {
                return table;
            }

            table.Parse(csvAsset.text);
            return table;
        }

        public bool TryGet(string iconKey, out ItemIconDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(iconKey))
            {
                definition = null;
                return false;
            }

            return iconByKey.TryGetValue(iconKey, out definition);
        }

        private void Parse(string csv)
        {
            icons.Clear();
            iconByKey.Clear();

            if (string.IsNullOrWhiteSpace(csv))
            {
                return;
            }

            List<string[]> rows = ParseRows(csv);
            if (rows.Count <= 1)
            {
                return;
            }

            Dictionary<string, int> header = BuildHeader(rows[0]);

            for (int i = 1; i < rows.Count; i++)
            {
                string[] row = rows[i];
                string iconKey = GetString(row, header, "IconKey");
                if (string.IsNullOrWhiteSpace(iconKey))
                {
                    continue;
                }

                ItemIconDefinition definition = ItemIconDefinition.Create(
                    iconKey,
                    GetString(row, header, "LocalAddress"),
                    GetString(row, header, "FallbackIconKey"),
                    GetString(row, header, "RemotePath"),
                    GetString(row, header, "Hash"),
                    GetInt(row, header, "Version", 1));

                icons.Add(definition);
                iconByKey[definition.IconKey] = definition;
            }
        }

        private static Dictionary<string, int> BuildHeader(string[] row)
        {
            Dictionary<string, int> header = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < row.Length; i++)
            {
                string key = row[i]?.Trim();
                if (!string.IsNullOrWhiteSpace(key))
                {
                    header[key] = i;
                }
            }

            return header;
        }

        private static string GetString(string[] row, Dictionary<string, int> header, string key)
        {
            if (!header.TryGetValue(key, out int index) || index < 0 || index >= row.Length)
            {
                return string.Empty;
            }

            return row[index]?.Trim() ?? string.Empty;
        }

        private static int GetInt(string[] row, Dictionary<string, int> header, string key, int fallback)
        {
            string value = GetString(row, header, key);
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : fallback;
        }

        private static List<string[]> ParseRows(string csv)
        {
            List<string[]> rows = new List<string[]>();
            List<string> columns = new List<string>();
            StringBuilder value = new StringBuilder();
            bool isQuoted = false;

            for (int i = 0; i < csv.Length; i++)
            {
                char c = csv[i];

                if (c == '"')
                {
                    if (isQuoted && i + 1 < csv.Length && csv[i + 1] == '"')
                    {
                        value.Append('"');
                        i++;
                    }
                    else
                    {
                        isQuoted = !isQuoted;
                    }
                }
                else if (c == ',' && !isQuoted)
                {
                    columns.Add(value.ToString());
                    value.Length = 0;
                }
                else if ((c == '\n' || c == '\r') && !isQuoted)
                {
                    if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n')
                    {
                        i++;
                    }

                    columns.Add(value.ToString());
                    value.Length = 0;

                    if (!IsEmptyRow(columns))
                    {
                        rows.Add(columns.ToArray());
                    }

                    columns.Clear();
                }
                else
                {
                    value.Append(c);
                }
            }

            columns.Add(value.ToString());
            if (!IsEmptyRow(columns))
            {
                rows.Add(columns.ToArray());
            }

            return rows;
        }

        private static bool IsEmptyRow(List<string> columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(columns[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
