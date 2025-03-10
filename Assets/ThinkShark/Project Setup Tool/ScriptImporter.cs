using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ThinkShark.ProjectSetupTool
{
    public class ScriptImporter
    {
        public string TemplateFolder;

        private string templateFolderName;
        private string targetFolder;
        private readonly List<string> scriptNames = new();      
        private readonly List<bool> templateSelections = new(); 
        private bool selectAll;                                 
        private bool scriptFolderLoaded;
        private bool targetFolderLoaded;
        private Vector2 scriptScroll;                           

        private PackageInstaller packageInstaller;

        // Zeichne die GUI
        public void Draw() {
            GUILayout.Label("<u>Script Importer<u>",
                new GUIStyle() { richText = true, normal = { textColor = Color.white } });

            GUILayout.Space(10);

            // Button zum Auswählen des Template-Ordners
            EditorGUILayout.LabelField("Template Folder Path:");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(string.IsNullOrEmpty(TemplateFolder) ? "No folder selected" : TemplateFolder,
                EditorStyles.textField, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Select Template Folder", GUILayout.Width(180))) {
                string initialPath = "C:/Unity/Assets/Scripts/";
                string selectedPath = EditorUtility.OpenFolderPanel("Select Template Folder", initialPath, "");
                if (!string.IsNullOrEmpty(selectedPath)) {
                    TemplateFolder = selectedPath;
                    templateFolderName = Path.GetFileName(TemplateFolder);  // Nur den letzten Teil des Pfades (Ordnername) extrahieren
                    LoadTemplates(TemplateFolder); // Lade Templates aus dem Ordner
                }
            }

            EditorGUILayout.EndHorizontal();

            // Zielordner-Auswahl
            GUILayout.Label("Target Folder Path:");
            EditorGUILayout.BeginHorizontal();
            targetFolder = EditorGUILayout.TextField(targetFolder);

            if (GUILayout.Button("Select Target Folder", GUILayout.Width(150))) {
                string defaultTargetPath = Path.Combine(Application.dataPath, "_Project/Scripts");
                string selectedTargetPath =
                    EditorUtility.OpenFolderPanel("Select Target Folder", defaultTargetPath, "");

                if (!string.IsNullOrEmpty(selectedTargetPath)) {
                    if (selectedTargetPath.StartsWith(Application.dataPath)) {
                        targetFolder = "Assets" + selectedTargetPath.Substring(Application.dataPath.Length);
                    }
                    else {
                        targetFolder = selectedTargetPath;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            if (scriptNames.Count != 0)
                GUILayout.Label($"<u>{templateFolderName}<u>",
                    new GUIStyle() { richText = true, normal = { textColor = Color.white } });
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (scriptNames.Count != 0) {
                GUILayout.BeginHorizontal();
                bool previousSelectAll = selectAll; // Speichert den vorherigen Status von Select All
                // Toggle für "Alle auswählen"
                selectAll = EditorGUILayout.Toggle(selectAll, GUILayout.Width(15));
                GUILayout.Label("Select All", EditorStyles.boldLabel);

                if (selectAll != previousSelectAll) {
                    for (int i = 0; i < templateSelections.Count; i++) {
                        templateSelections[i] = selectAll;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            GUILayout.BeginHorizontal();

            GUILayout.Space(10);
            
            GUILayout.BeginVertical();

            // Zeigt Liste der geladenen Templates an
            scriptScroll = EditorGUILayout.BeginScrollView(scriptScroll, GUILayout.ExpandHeight(true));
            for (int i = 0; i < scriptNames.Count; i++) {
                EditorGUILayout.BeginHorizontal();

                // Toggle zur Auswahl hinzufügen
                templateSelections[i] = EditorGUILayout.Toggle(templateSelections[i], GUILayout.Width(20));

                // Label mit dem Skript-Namen
                GUILayout.Label(scriptNames[i], GUILayout.ExpandWidth(true));

                // Button zum direkten Importieren
                if (GUILayout.Button("Import", GUILayout.Width(80))) {
                    string templatePath = Path.Combine(TemplateFolder, scriptNames[i] + ".txt");
                    string targetPath = Path.Combine(targetFolder, scriptNames[i]);
                    if (File.Exists(templatePath)) {
                        string templateContent = File.ReadAllText(templatePath);
                        Directory.CreateDirectory(targetFolder);
                        File.WriteAllText(targetPath, templateContent);
                        AssetDatabase.Refresh();
                    }
                    else {
                        Debug.LogError("Template file not found: " + templatePath);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            // Button zum Importieren der Skripte
            if (GUILayout.Button("Import Selected Scripts", GUILayout.Height(30))) {
                ImportSelectedScripts();
            }

            GUILayout.Space(20);
        }

        // Lade Templates aus einem Ordner
        public void LoadTemplates(string folderPath) {
            var templatePaths = Directory.GetFiles(folderPath, "*.txt");
            scriptNames.Clear();
            templateSelections.Clear();

            foreach (var templatePath in templatePaths) {
                string scriptName = Path.GetFileNameWithoutExtension(templatePath);
                scriptNames.Add(scriptName);
                templateSelections.Add(false); // Alle Templates initial auf "nicht ausgewählt"
            }
        }

        // Importiert die ausgewählten Skripte
        private void ImportSelectedScripts() {
            if (string.IsNullOrEmpty(TemplateFolder)) {
                Debug.LogError("No template folder selected. Please select a folder containing script templates.");
                return;
            }

            for (int i = 0; i < scriptNames.Count; i++) {
                if (templateSelections[i]) {
                    string templatePath = Path.Combine(TemplateFolder, scriptNames[i] + ".txt");
                    string targetPath = Path.Combine(targetFolder, scriptNames[i]);

                    if (!File.Exists(templatePath)) {
                        Debug.LogError("Template file not found: " + templatePath);
                        continue;
                    }

                    string templateContent = File.ReadAllText(templatePath);
                    Directory.CreateDirectory(targetFolder);
                    File.WriteAllText(targetPath, templateContent);
                }
            }
            
            AssetDatabase.Refresh();
        }

        public void SaveTemplateFolderToTxt(PackageInstaller installer) {
            if (string.IsNullOrEmpty(installer.PackageFolder)) {
                Debug.LogError("TemplateFolder is not set. Cannot save TemplateFolder.txt.");
                return;
            }

            // Pfad zur FolderNames.txt im PackageFolder
            string filePath = Path.Combine(installer.PackageFolder, "TemplateFolder.txt");

            try {
                File.WriteAllText(filePath, TemplateFolder);
            }
            catch (IOException ex) {
                Debug.LogError($"Error saving TemplateFolder.txt: {ex.Message}");
            }
        }

        public void LoadTemplateFolderFromTxt(PackageInstaller installer) {
            if (scriptFolderLoaded) return; // Verhindert doppeltes Laden

            if (string.IsNullOrEmpty(installer.PackageFolder)) {
                Debug.LogError("TemplateFolder is not set. Cannot load TemplateFolder.txt.");
                return;
            }

            // Pfad zur FolderNames.txt im PackageFolder
            string filePath = Path.Combine(installer.PackageFolder, "TemplateFolder.txt");

            // Prüfen, ob die Datei existiert
            if (File.Exists(filePath)) {
                try {
                    TemplateFolder = File.ReadAllText(filePath);  // Lade die Datei und speichere die Einträge in FolderNames
                    scriptFolderLoaded = true;
                }
                catch (IOException ex) {
                    Debug.LogError($"Error reading TemplateFolder.txt: {ex.Message}");
                }
            }
            else {
                Debug.LogWarning($"TemplateFolder.txt not found at {filePath}. Creating an empty file.");

                try {
                    // Erstelle eine neue, leere Datei
                    using (File.Create(filePath)) {
                    }
                    Debug.Log("Created new empty TemplateFolder.txt file.");
                }
                catch (IOException ex) {
                    Debug.LogError($"Error creating TemplateFolder.txt: {ex.Message}");
                }
            }
        }

        public void SaveTargetFolderToTxt(PackageInstaller installer) {
            string txtFilePath = Path.Combine(installer.PackageFolder, "ScriptTargetFolder.txt");

            try {
                File.WriteAllText(txtFilePath, targetFolder);
            }
            catch (IOException ex) {
                Debug.LogError("Error saving ScriptTargetFolder.txt: " + ex.Message);
            }
        }

        public void LoadTargetFolderFromTxt(PackageInstaller installer) {
            if (targetFolderLoaded) return;

            // Pfad zur Datei "PackageFolderName.txt"
            string txtFilePath = Path.Combine(installer.PackageFolder, "ScriptTargetFolder.txt");

            if (File.Exists(txtFilePath)) {
                try {
                    // Erste Zeile der Datei lesen
                    string folderPath =
                        File.ReadAllText(txtFilePath).Trim(); // Trim entfernt unnötige Leerzeichen/Zeilenumbrüche

                    if (!string.IsNullOrEmpty(folderPath)) {
                        targetFolder = folderPath; // Setze PackageFolder auf den Pfad aus der Datei
                        targetFolderLoaded = true; // Markiere den Folder als geladen
                    }
                    else {
                        Debug.LogWarning("ScriptTargetFolder.txt exists but is empty. Please set a valid folder path.");
                    }
                }
                catch (IOException ex) {
                    Debug.LogError("Error reading ScriptTargetFolder.txt: " + ex.Message);
                }
            }
            else {
                Debug.LogWarning("ScriptTargetFolder.txt not found in: " + targetFolder + ". Creating an empty file.");
                try {
                    // Erstelle eine leere Datei
                    using (File.Create(txtFilePath)) {
                        // Datei wird erstellt und geschlossen
                    }

                    Debug.Log("ScriptTargetFolder.txt file created successfully.");
                    targetFolder = ""; // Setze einen leeren Pfad
                }
                catch (IOException ex) {
                    Debug.LogError("Error creating ScriptTargetFolder.txt: " + ex.Message);
                }
            }
        }
    }
}