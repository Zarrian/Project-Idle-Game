using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Outil d'import : lit un fichier CSV (exporté depuis Google Sheets/Excel,
/// pas besoin de plugin) et génère/actualise automatiquement un
/// ShipUpgradeData (un ScriptableObject par vaisseau/CSV).
///
/// Format CSV attendu (en-tête sur la 1ère ligne, ordre des colonnes libre) :
/// Level,StatName,Value,Cost_Metal,Cost_Electricity,Cost_Uranium
/// </summary>
public static class ShipUpgradeCSVImporter
{
    private const string OutputFolder = "Assets/Data/ShipUpgrades";

    [MenuItem("Tools/Upgrades/Importer un CSV...")]
    public static void ImportSingleFile()
    {
        string path = EditorUtility.OpenFilePanel("Choisir un CSV de vaisseau", Application.dataPath, "csv");
        if (string.IsNullOrEmpty(path))
            return;

        ShipUpgradeData asset = ImportFile(path);
        if (asset != null)
        {
            EditorUtility.DisplayDialog("Import CSV",
                $"'{asset.shipName}' importé avec succès ({asset.stats.Count} stats).", "OK");
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }

    [MenuItem("Tools/Upgrades/Importer tous les CSV d'un dossier...")]
    public static void ImportFolder()
    {
        string folder = EditorUtility.OpenFolderPanel("Choisir le dossier contenant les CSV", Application.dataPath, "");
        if (string.IsNullOrEmpty(folder))
            return;

        string[] files = Directory.GetFiles(folder, "*.csv", SearchOption.TopDirectoryOnly);
        if (files.Length == 0)
        {
            EditorUtility.DisplayDialog("Import CSV", "Aucun fichier .csv trouvé dans ce dossier.", "OK");
            return;
        }

        int count = 0;
        foreach (string file in files)
        {
            if (ImportFile(file) != null)
                count++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Import CSV", $"{count}/{files.Length} fichier(s) importé(s).", "OK");
    }

    /// <summary>
    /// Parse un fichier CSV et crée/met à jour le ShipUpgradeData correspondant.
    /// Le nom du vaisseau et de l'asset généré = nom du fichier (sans extension).
    /// </summary>
    private static ShipUpgradeData ImportFile(string absolutePath)
    {
        string shipName = Path.GetFileNameWithoutExtension(absolutePath);
        List<string> lines = File.ReadAllLines(absolutePath)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        if (lines.Count < 2)
        {
            Debug.LogWarning($"[ShipUpgradeCSVImporter] '{shipName}' : CSV vide ou sans données, ignoré.");
            return null;
        }

        // En-tête : on retrouve l'index de chaque colonne par son nom pour
        // rester robuste si l'ordre des colonnes change dans le sheet.
        string[] header = SplitCsvLine(lines[0]);
        int idxLevel = FindColumn(header, "Level");
        int idxStatName = FindColumn(header, "StatName");
        int idxValue = FindColumn(header, "Value");
        int idxCostMetal = FindColumn(header, "Cost_Metal");
        int idxCostElectricity = FindColumn(header, "Cost_Electricity");
        int idxCostUranium = FindColumn(header, "Cost_Uranium");

        if (idxLevel < 0 || idxStatName < 0 || idxValue < 0)
        {
            Debug.LogError($"[ShipUpgradeCSVImporter] '{shipName}' : colonnes obligatoires manquantes (Level, StatName, Value). Import annulé.");
            return null;
        }

        // On regroupe les lignes par StatName : une Track par catégorie de
        // stat (HP, Damage, CD Attack, Attack...), chacune avec ses paliers
        // triés par niveau, au lieu d'une seule liste plate de 20+ lignes.
        Dictionary<string, StatUpgradeTrack> tracksByName = new Dictionary<string, StatUpgradeTrack>();
        List<string> statOrder = new List<string>();

        for (int i = 1; i < lines.Count; i++)
        {
            string[] cols = SplitCsvLine(lines[i]);
            if (cols.Length == 0)
                continue;

            try
            {
                string statName = ParseString(cols, idxStatName);
                if (string.IsNullOrEmpty(statName))
                    continue;

                if (!tracksByName.TryGetValue(statName, out StatUpgradeTrack track))
                {
                    track = new StatUpgradeTrack { statName = statName };
                    tracksByName[statName] = track;
                    statOrder.Add(statName);
                }

                track.steps.Add(new StatUpgradeStep
                {
                    level = ParseInt(cols, idxLevel),
                    value = ParseFloat(cols, idxValue),
                    costMetal = ParseInt(cols, idxCostMetal),
                    costElectricity = ParseInt(cols, idxCostElectricity),
                    costUranium = ParseInt(cols, idxCostUranium),
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ShipUpgradeCSVImporter] '{shipName}' ligne {i + 1} ignorée : {e.Message}");
            }
        }

        List<StatUpgradeTrack> parsedStats = new List<StatUpgradeTrack>();
        foreach (string statName in statOrder)
        {
            StatUpgradeTrack track = tracksByName[statName];
            track.steps.Sort((a, b) => a.level.CompareTo(b.level));
            parsedStats.Add(track);
        }

        if (!Directory.Exists(OutputFolder))
        {
            Directory.CreateDirectory(OutputFolder);
        }

        string assetPath = $"{OutputFolder}/{shipName}.asset";
        ShipUpgradeData asset = AssetDatabase.LoadAssetAtPath<ShipUpgradeData>(assetPath);
        bool isNew = asset == null;
        if (isNew)
        {
            asset = ScriptableObject.CreateInstance<ShipUpgradeData>();
        }

        asset.shipName = shipName;
        asset.stats = parsedStats;

        if (isNew)
        {
            AssetDatabase.CreateAsset(asset, assetPath);
        }
        else
        {
            EditorUtility.SetDirty(asset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ShipUpgradeCSVImporter] '{shipName}' : {parsedStats.Count} catégories de stats importées -> {assetPath}");
        return asset;
    }

    private static int FindColumn(string[] header, string name)
    {
        for (int i = 0; i < header.Length; i++)
        {
            if (string.Equals(header[i].Trim(), name, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    private static string ParseString(string[] cols, int idx)
    {
        if (idx < 0 || idx >= cols.Length) return string.Empty;
        return cols[idx].Trim();
    }

    private static int ParseInt(string[] cols, int idx)
    {
        if (idx < 0 || idx >= cols.Length) return 0;
        string s = cols[idx].Trim();
        if (string.IsNullOrEmpty(s)) return 0;
        return int.Parse(s, CultureInfo.InvariantCulture);
    }

    private static float ParseFloat(string[] cols, int idx)
    {
        if (idx < 0 || idx >= cols.Length) return 0f;
        string s = cols[idx].Trim();
        if (string.IsNullOrEmpty(s)) return 0f;
        return float.Parse(s, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Découpe une ligne CSV en gérant les champs entre guillemets
    /// (au cas où un export Excel/Sheets ajoute des quotes autour des valeurs).
    /// </summary>
    private static string[] SplitCsvLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        System.Text.StringBuilder current = new System.Text.StringBuilder();

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result.ToArray();
    }
}
