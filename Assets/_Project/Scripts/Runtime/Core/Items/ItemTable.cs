using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace PH.Core.Items
{
    public sealed class ItemTable
    {
        private readonly List<ItemDefinition> items = new List<ItemDefinition>();
        private readonly Dictionary<string, ItemDefinition> itemById = new Dictionary<string, ItemDefinition>();

        public IReadOnlyList<ItemDefinition> Items => items;

        public static ItemTable Load(TextAsset csvAsset)
        {
            ItemTable table = new ItemTable();

            if (csvAsset == null)
            {
                return table;
            }

            table.Parse(csvAsset.text);
            return table;
        }

        public bool TryGet(string itemId, out ItemDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                definition = null;
                return false;
            }

            return itemById.TryGetValue(itemId, out definition);
        }

        public List<ItemDefinition> GetSpawnCandidates(int absoluteFloor)
        {
            List<ItemDefinition> candidates = new List<ItemDefinition>();

            for (int i = 0; i < items.Count; i++)
            {
                ItemDefinition item = items[i];
                if (item != null && item.CanSpawnAtFloor(absoluteFloor))
                {
                    candidates.Add(item);
                }
            }

            return candidates;
        }

        private void Parse(string csv)
        {
            items.Clear();
            itemById.Clear();

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
                string itemId = GetString(row, header, "ItemId");
                if (string.IsNullOrWhiteSpace(itemId))
                {
                    continue;
                }

                ItemDefinition definition = ItemDefinition.Create(
                    itemId,
                    GetString(row, header, "ServerItemId"),
                    GetString(row, header, "TableVersion"),
                    GetBool(row, header, "Enabled", true),
                    GetString(row, header, "DisplayName"),
                    GetEnum(row, header, "ItemType", ItemType.Score),
                    GetIconKey(row, header),
                    GetInt(row, header, "MinFloor", 1),
                    GetInt(row, header, "MaxFloor", 0),
                    GetInt(row, header, "SpawnWeight", 0),
                    GetInt(row, header, "RequiredPassCount", 1),
                    GetFloat(row, header, "LifetimeSeconds", 8f),
                    GetEnum(row, header, "PassDirection", ItemPassDirection.Any),
                    GetString(row, header, "EffectKey"),
                    GetInt(row, header, "EffectValue", 0),
                    GetFloat(row, header, "EffectDurationSeconds", 0f),
                    GetBool(row, header, "AffectsScore", false),
                    GetBool(row, header, "AffectsProgression", false),
                    GetBool(row, header, "ServerValidated", false),
                    GetInt(row, header, "MaxAcquirePerRun", 0),
                    GetString(row, header, "Rarity"));

                items.Add(definition);
                itemById[definition.ItemId] = definition;
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

        private static string GetIconKey(string[] row, Dictionary<string, int> header)
        {
            string iconKey = GetString(row, header, "IconKey");
            return string.IsNullOrWhiteSpace(iconKey) ? GetString(row, header, "PrefabKey") : iconKey;
        }

        private static bool GetBool(string[] row, Dictionary<string, int> header, string key, bool fallback)
        {
            string value = GetString(row, header, key);
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            if (bool.TryParse(value, out bool result))
            {
                return result;
            }

            return value == "1";
        }

        private static float GetFloat(string[] row, Dictionary<string, int> header, string key, float fallback)
        {
            string value = GetString(row, header, key);
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result) ? result : fallback;
        }

        private static T GetEnum<T>(string[] row, Dictionary<string, int> header, string key, T fallback) where T : struct
        {
            string value = GetString(row, header, key);
            return Enum.TryParse(value, true, out T result) ? result : fallback;
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
