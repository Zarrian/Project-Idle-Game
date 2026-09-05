using System;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Fenêtre d'import : à partir d'une URL de base "Publier sur le web" (document
/// entier, format CSV) et d'une liste de gid d'onglets, télécharge chaque onglet
/// vaisseau et réimporte automatiquement le ShipUpgradeData correspondant, sans
/// export CSV manuel ni Excel.
/// </summary>
public class ShipUpgradeGoogleSheetImporterWindow : EditorWindow    
{
    private const string PrefsKey = "ShipUpgradeCSVImporter.SheetEntries";

    [Serializable]
    private class SheetEntry
    {
        public string shipName;
        public string gid;
    }

    [Serializable]
    private class SheetEntryList
    {
        public string baseUrl;
        public List<SheetEntry> entries = new List<SheetEntry>();
    }

    private SheetEntryList data;
    private Vector2 scroll;

    [MenuItem("Tools/Upgrades/Importer depuis Google Sheets...")]
    public static void Open()
    {
        var window = GetWindow<ShipUpgradeGoogleSheetImporterWindow>("Import Google Sheets");
        window.minSize = new Vector2(460, 220);
        window.Show();
    }

    private void OnEnable()
    {
        LoadData();
    }

    private void LoadData()
    {
        string json = EditorPrefs.GetString(PrefsKey, string.Empty);
        data = string.IsNullOrEmpty(json)
            ? new SheetEntryList()
            : JsonUtility.FromJson<SheetEntryList>(json);
    }

    private void SaveData()
    {
        EditorPrefs.SetString(PrefsKey, JsonUtility.ToJson(data));
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("URL de base (Publier sur le web, Document entier, CSV)", EditorStyles.boldLabel);
        data.baseUrl = EditorGUILayout.TextField(data.baseUrl);

        EditorGUILayout.HelpBox(
            "Collez ici l'URL générée par Fichier > Partager > Publier sur le web " +
            "(elle se termine par /pub?output=csv). Pour chaque vaisseau, indiquez le nom " +
            "de l'onglet correspondant et son 'gid' (visible dans l'URL quand l'onglet est " +
            "sélectionné en mode édition, ex: #gid=1977872973).", MessageType.Info);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Vaisseaux / onglets", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        SheetEntry toRemove = null;
        foreach (SheetEntry entry in data.entries)
        {
            EditorGUILayout.BeginHorizontal("box");

            entry.shipName = EditorGUILayout.TextField("Vaisseau", entry.shipName, GUILayout.Width(220));
            entry.gid = EditorGUILayout.TextField("gid", entry.gid, GUILayout.Width(160));

            if (GUILayout.Button("Importer", GUILayout.Width(80)))
            {
                ImportEntry(entry);
            }

            if (GUILayout.Button("X", GUILayout.Width(24)))
            {
                toRemove = entry;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (toRemove != null)
        {
            data.entries.Remove(toRemove);
            SaveData();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("+ Ajouter un vaisseau"))
        {
            data.entries.Add(new SheetEntry());
            SaveData();
        }

        GUI.enabled = data.entries.Count > 0 && !string.IsNullOrWhiteSpace(data.baseUrl);
        if (GUILayout.Button("Importer tout", GUILayout.Height(24)))
        {
            ImportAll();
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        if (GUI.changed)
        {
            SaveData();
        }
    }

    private void ImportAll()
    {
        int count = 0;
        foreach (SheetEntry entry in data.entries)
        {
            if (ImportEntry(entry))
                count++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Import Google Sheets", $"{count}/{data.entries.Count} vaisseau(x) importé(s).", "OK");
    }

    private bool ImportEntry(SheetEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.shipName) || string.IsNullOrWhiteSpace(entry.gid))
        {
            Debug.LogWarning("[ShipUpgradeGoogleSheetImporterWindow] Entrée ignorée : nom de vaisseau ou gid manquant.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(data.baseUrl))
        {
            Debug.LogError("[ShipUpgradeGoogleSheetImporterWindow] URL de base manquante.");
            return false;
        }

        string url = BuildCsvUrl(data.baseUrl, entry.gid);

        string csv;
        try
        {
            using (var client = new WebClient())
            {
                csv = client.DownloadString(url);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[ShipUpgradeGoogleSheetImporterWindow] '{entry.shipName}' : échec du téléchargement ({e.Message}).");
            return false;
        }

        if (LooksLikeHtml(csv))
        {
            Debug.LogError(
                $"[ShipUpgradeGoogleSheetImporterWindow] '{entry.shipName}' : la réponse ne ressemble pas à du CSV " +
                "(vérifiez le gid et que le document est bien publié).");
            return false;
        }

        ShipUpgradeData asset = ShipUpgradeCSVImporter.ImportRawCsv(entry.shipName, csv);
        return asset != null;
    }

    /// <summary>
    /// Construit l'URL CSV d'un onglet précis à partir de l'URL de base publiée,
    /// en forçant les paramètres gid/output/single quel que soit ce qui était déjà présent.
    /// </summary>
    private static string BuildCsvUrl(string baseUrl, string gid)
    {
        string trimmed = baseUrl.Trim();
        int queryIndex = trimmed.IndexOf('?');
        string root = queryIndex >= 0 ? trimmed.Substring(0, queryIndex) : trimmed;
        return $"{root}?gid={gid}&single=true&output=csv";
    }

    private static bool LooksLikeHtml(string content)
    {
        if (string.IsNullOrEmpty(content))
            return false;
        string trimmed = content.TrimStart();
        return trimmed.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("<html", StringComparison.OrdinalIgnoreCase);
    }
}