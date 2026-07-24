using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace LootUp.Core.Items
{
    public enum ArtifactEffectType
    {
        None,
        ResultExperienceBonusPercent,
        ResultScoreBonusPercent,
        MoveSpeedPowerBonusPercent,
        MoveSpeedDurationBonusPercent,
        ScoreItemDoubleChancePercent,
        TimeItemDoubleChancePercent,
        CharacterSkillPowerBonusPercent,
        CharacterCoinChanceBonusPercent
    }

    public sealed class ArtifactDefinition
    {
        public ArtifactDefinition(string artifactId, string displayName, string theme, string iconPath)
        {
            ArtifactId = artifactId;
            DisplayName = displayName;
            Theme = theme;
            IconPath = iconPath;
        }

        public string ArtifactId { get; }
        public string DisplayName { get; }
        public string Theme { get; }
        public string IconPath { get; }
    }

    public sealed class ArtifactEffectDefinition
    {
        public ArtifactEffectDefinition(
            string effectId,
            string displayName,
            string description,
            ArtifactEffectType effectType,
            float valuePercent,
            int requiredCount,
            IReadOnlyList<string> candidateArtifactIds)
        {
            EffectId = effectId;
            DisplayName = displayName;
            Description = description;
            EffectType = effectType;
            ValuePercent = Mathf.Max(0f, valuePercent);
            RequiredCount = Mathf.Max(1, requiredCount);
            CandidateArtifactIds = candidateArtifactIds ?? Array.Empty<string>();
        }

        public string EffectId { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public ArtifactEffectType EffectType { get; }
        public float ValuePercent { get; }
        public int RequiredCount { get; }
        public IReadOnlyList<string> CandidateArtifactIds { get; }

        public int GetOwnedRequirementCount()
        {
            int ownedCount = 0;
            for (int i = 0; i < CandidateArtifactIds.Count; i++)
            {
                if (ItemCollectionManager.GetOwnedAmount(CandidateArtifactIds[i]) > 0)
                {
                    ownedCount++;
                }
            }

            return ownedCount;
        }

        public bool IsActive => GetOwnedRequirementCount() >= RequiredCount;
    }

    public sealed class ArtifactCatalog
    {
        private const string ArtifactResourcePath = "Data/Artifacts";
        private const string EffectResourcePath = "Data/ArtifactEffects";
        private static ArtifactCatalog instance;

        private readonly List<ArtifactDefinition> artifacts = new List<ArtifactDefinition>();
        private readonly List<ArtifactEffectDefinition> effects = new List<ArtifactEffectDefinition>();
        private readonly Dictionary<string, ArtifactDefinition> artifactById =
            new Dictionary<string, ArtifactDefinition>(StringComparer.Ordinal);

        public static ArtifactCatalog Instance => instance ??= Load();
        public IReadOnlyList<ArtifactDefinition> Artifacts => artifacts;
        public IReadOnlyList<ArtifactEffectDefinition> Effects => effects;

        public static void Reload()
        {
            instance = Load();
        }

        public bool IsSystemUnlocked()
        {
            for (int i = 0; i < artifacts.Count; i++)
            {
                if (ItemCollectionManager.GetOwnedAmount(artifacts[i].ArtifactId) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetOwnedArtifactCount()
        {
            int count = 0;
            for (int i = 0; i < artifacts.Count; i++)
            {
                if (ItemCollectionManager.GetOwnedAmount(artifacts[i].ArtifactId) > 0)
                {
                    count++;
                }
            }

            return count;
        }

        public bool TryGetArtifact(string artifactId, out ArtifactDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(artifactId))
            {
                definition = null;
                return false;
            }

            return artifactById.TryGetValue(artifactId, out definition);
        }

        private static ArtifactCatalog Load()
        {
            ArtifactCatalog catalog = new ArtifactCatalog();
            TextAsset artifactCsv = Resources.Load<TextAsset>(ArtifactResourcePath);
            TextAsset effectCsv = Resources.Load<TextAsset>(EffectResourcePath);
            catalog.ParseArtifacts(artifactCsv != null ? artifactCsv.text : string.Empty);
            catalog.ParseEffects(effectCsv != null ? effectCsv.text : string.Empty);
            return catalog;
        }

        private void ParseArtifacts(string csv)
        {
            List<string[]> rows = ParseRows(csv);
            for (int i = 1; i < rows.Count; i++)
            {
                string[] row = rows[i];
                if (row.Length < 4 || string.IsNullOrWhiteSpace(row[0]))
                {
                    continue;
                }

                ArtifactDefinition definition = new ArtifactDefinition(
                    row[0].Trim(),
                    row[1].Trim(),
                    row[2].Trim(),
                    row[3].Trim());
                artifacts.Add(definition);
                artifactById[definition.ArtifactId] = definition;
            }
        }

        private void ParseEffects(string csv)
        {
            List<string[]> rows = ParseRows(csv);
            for (int i = 1; i < rows.Count; i++)
            {
                string[] row = rows[i];
                if (row.Length < 6
                    || string.IsNullOrWhiteSpace(row[0])
                    || !Enum.TryParse(row[2].Trim(), true, out ArtifactEffectType effectType))
                {
                    continue;
                }

                float.TryParse(row[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float valuePercent);
                int.TryParse(row[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int requiredCount);
                string[] candidates = row[5].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                for (int candidateIndex = 0; candidateIndex < candidates.Length; candidateIndex++)
                {
                    candidates[candidateIndex] = candidates[candidateIndex].Trim();
                }

                effects.Add(new ArtifactEffectDefinition(
                    row[0].Trim(),
                    row[1].Trim(),
                    row.Length > 6 ? row[6].Trim() : string.Empty,
                    effectType,
                    valuePercent,
                    requiredCount,
                    candidates));
            }
        }

        private static List<string[]> ParseRows(string csv)
        {
            List<string[]> rows = new List<string[]>();
            if (string.IsNullOrWhiteSpace(csv))
            {
                return rows;
            }

            List<string> fields = new List<string>();
            System.Text.StringBuilder field = new System.Text.StringBuilder();
            bool insideQuotes = false;

            for (int i = 0; i < csv.Length; i++)
            {
                char character = csv[i];
                if (character == '"')
                {
                    if (insideQuotes && i + 1 < csv.Length && csv[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        insideQuotes = !insideQuotes;
                    }
                }
                else if (character == ',' && !insideQuotes)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else if ((character == '\n' || character == '\r') && !insideQuotes)
                {
                    if (character == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n')
                    {
                        i++;
                    }

                    fields.Add(field.ToString());
                    field.Clear();
                    if (fields.Count > 1 || !string.IsNullOrWhiteSpace(fields[0]))
                    {
                        rows.Add(fields.ToArray());
                    }

                    fields.Clear();
                }
                else
                {
                    field.Append(character);
                }
            }

            if (field.Length > 0 || fields.Count > 0)
            {
                fields.Add(field.ToString());
                rows.Add(fields.ToArray());
            }

            return rows;
        }
    }
}
