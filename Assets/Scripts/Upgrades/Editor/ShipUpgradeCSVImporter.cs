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
/// Format CSV attendu (en-tête sur la 1ère ligne, format "large" : une colonne = une stat) :
/// Level,&lt;Stat1&gt;,&lt;Stat1&gt;_Metal,&lt;Stat1&gt;_Electricity,&lt;Stat1&gt;_Uranium,&lt;Stat2&gt;,&lt;Stat2&gt;_Metal,...
///
/// Chaque stat est un bloc de 4 colonnes cote a cote : le nom de la stat (valeur brute,
/// sans suffixe) suivi de ses 3 colonnes de cout (_Metal, _Electricity, _Uranium).
/// L'ordre des blocs dans le fichier n'a pas d'importance, et le nombre de stats/blocs
/// est libre : ajouter une nouvelle stat = ajouter un nouveau bloc de 4 colonnes, rien
/// a changer ici.
/// </summary>
public static class ShipUpgradeCSVImporter
{
    private const string OutputFolder = "Assets/Data/ShipUpgrades";
    private static readonly string[] CostSuffixes = { "_Metal", "_Electricity", "_Uranium" };

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
    /// Une stat detectee dans l'en-tete : l'index de sa colonne de valeur, et les index
    /// (optionnels, -1 si absents) de ses 3 colonnes de cout.
    /// </summary>
    private class StatColumns
    {
        public string statName;
        public int valueIndex = -1;
        public int metalIndex = -1;
        public int electricityIndex = -1;
        public int uraniumIndex = -1;
    }

    /// <summary>
    /// Parse un fichier CSV local et crée/met à jour le ShipUpgradeData correspondant.
    /// Le nom du vaisseau et de l'asset généré = nom du fichier (sans extension).
    /// </summary>
    private static ShipUpgradeData ImportFile(string absolutePath)
    {
        string shipName = Path.GetFileNameWithoutExtension(absolutePath);
        List<string> lines = File.ReadAllLines(absolutePath)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        return ImportLines(shipName, lines);
    }

    /// <summary>
    /// Parse le contenu CSV brut (ex: téléchargé depuis un Google Sheet publié) et
    /// crée/met à jour le ShipUpgradeData correspondant au vaisseau donné.
    /// </summary>
    public static ShipUpgradeData ImportRawCsv(string shipName, string csvContent)
    {
        if (string.IsNullOrEmpty(csvContent))
        {
            Debug.LogWarning($"[ShipUpgradeCSVImporter] '{shipName}' : contenu CSV vide, ignoré.");
            return null;
        }

        List<string> lines = csvContent
            .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        return ImportLines(shipName, lines);
    }

    /// <summary>
    /// Coeur du parsing, partagé entre l'import fichier local et l'import URL.
    /// </summary>
    private static ShipUpgradeData ImportLines(string shipName, List<string> lines)
    {
        if (lines.Count < 2)
        {
            Debug.LogWarning($"[ShipUpgradeCSVImporter] '{shipName}' : CSV vide ou sans données, ignoré.");
            return null;
        }

        string[] header = SplitCsvLine(lines[0]);
        int idxLevel = FindColumn(header, "Level");
        if (idxLevel < 0)
        {
            Debug.LogError($"[ShipUpgradeCSVImporter] '{shipName}' : colonne 'Level' introuvable. Import annulé.");
            return null;
        }

        // On detecte les blocs de stats a partir des noms de colonnes : une colonne SANS
        // suffixe _Metal/_Electricity/_Uranium demarre une nouvelle stat (son nom = le nom
        // de la stat), les colonnes avec suffixe viennent completer le cout de cette stat,
        // quel que soit leur ordre dans le fichier.
        Dictionary<string, StatColumns> columnsByStat = new Dictionary<string, StatColumns>();
        List<string> statOrder = new List<string>();

        for (int i = 0; i < header.Length; i++)
        {
            if (i == idxLevel) continue;

            string colName = header[i].Trim();
            if (string.IsNullOrEmpty(colName)) continue;

            string matchedSuffix = CostSuffixes.FirstOrDefault(s => colName.EndsWith(s, StringComparison.OrdinalIgnoreCase));
            string statName = matchedSuffix != null ? colName.Substring(0, colName.Length - matchedSuffix.Length) : colName;

            if (!columnsByStat.TryGetValue(statName, out StatColumns cols))
            {
                cols = new StatColumns { statName = statName };
                columnsByStat[statName] = cols;
                statOrder.Add(statName);
            }

            if (matchedSuffix == null)
                cols.valueIndex = i;
            else if (matchedSuffix.Equals("_Metal", StringComparison.OrdinalIgnoreCase))
                cols.metalIndex = i;
            else if (matchedSuffix.Equals("_Electricity", StringComparison.OrdinalIgnoreCase))
                cols.electricityIndex = i;
            else if (matchedSuffix.Equals("_Uranium", StringComparison.OrdinalIgnoreCase))
                cols.uraniumIndex = i;
        }

        // Une stat sans colonne de valeur (que des colonnes de cout, en-tete malformé) n'a pas de sens : on l'ignore.
        statOrder = statOrder.Where(name => columnsByStat[name].valueIndex >= 0).ToList();
        if (statOrder.Count == 0)
        {
            Debug.LogError($"[ShipUpgradeCSVImporter] '{shipName}' : aucune colonne de stat valide trouvée. Import annulé.");
            return null;
        }

        Dictionary<string, StatUpgradeTrack> tracksByName = statOrder.ToDictionary(
            name => name,
            name => new StatUpgradeTrack { statName = name });

        for (int i = 1; i < lines.Count; i++)
        {
            string[] row = SplitCsvLine(lines[i]);
            if (row.Length == 0)
                continue;

            int level;
            try
            {
                level = ParseInt(row, idxLevel);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ShipUpgradeCSVImporter] '{shipName}' ligne {i + 1} ignorée (Level invalide) : {e.Message}");
                continue;
            }

            foreach (string statName in statOrder)
            {
                StatColumns cols = columnsByStat[statName];

                // Cellule de valeur vide = cette stat n'a pas de palier a ce niveau (stat pas
                // encore commencee, ou deja au maximum au-dela de son vrai plafond). On ne
                // cree AUCUNE entree pour ce (stat, level) plutot que d'inserer une valeur 0 :
                // c'est ce qui permet a la stat de s'arreter exactement a son dernier niveau reel
                // (ex: nbAttack et maxUnits de Charllemagne s'arretent au niveau 20, pas 100).
                if (!HasValue(row, cols.valueIndex))
                    continue;

                try
                {
                    tracksByName[statName].steps.Add(new StatUpgradeStep
                    {
                        level = level,
                        value = ParseFloat(row, cols.valueIndex),
                        costMetal = ParseInt(row, cols.metalIndex),
                        costElectricity = ParseInt(row, cols.electricityIndex),
                        costUranium = ParseInt(row, cols.uraniumIndex),
                    });
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ShipUpgradeCSVImporter] '{shipName}' ligne {i + 1}, stat '{statName}' ignorée : {e.Message}");
                }
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

        Debug.Log($"[ShipUpgradeCSVImporter] '{shipName}' : {parsedStats.Count} stats importées ({string.Join(", ", statOrder)}) -> {assetPath}");
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

    private static bool HasValue(string[] cols, int idx)
    {
        if (idx < 0 || idx >= cols.Length) return false;
        return !string.IsNullOrEmpty(cols[idx].Trim());
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