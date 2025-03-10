using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ThinkShark.ProjectSetupTool
{
    public class FolderManager
    {
        public List<string> FolderNames;
        private bool isEditing;
        private bool essentialFoldersLoaded;
        private Vector2 folderScroll;


        public void Draw() {
            GUILayout.Label("<u>Folder Manager<u>", new GUIStyle() {richText = true, normal = {textColor = Color.white}});

            GUILayout.Space(15);

            if (GUILayout.Button(isEditing ? "Save Changes" : "Edit Folder List", GUILayout.Width(150))) {
                isEditing = !isEditing; // Bearbeitungsmodus wechseln
            }

            GUILayout.Space(20);

            folderScroll = EditorGUILayout.BeginScrollView(folderScroll, GUILayout.ExpandHeight(true));
            if (isEditing) {
                // Bearbeitungsliste anzeigen
                for (int i = 0; i < FolderNames.Count; i++) {
                    EditorGUILayout.BeginHorizontal();
                    FolderNames[i] = EditorGUILayout.TextField(FolderNames[i]);
                    if (GUILayout.Button("Remove", GUILayout.Width(70))) {
                        FolderNames.RemoveAt(i);
                        i--;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                GUILayout.Space(10);
                
                // Button zum Hinzufügen neuer Ordner
                if (GUILayout.Button("Add New Folder")) {
                    FolderNames.Add("");
                }

                EditorGUILayout.EndScrollView();
                GUILayout.Space(10);
            }
            else {
                // Standard-Anzeige der Ordner
                foreach (var folderName in FolderNames) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(folderName);
                    if (GUILayout.Button("Create", GUILayout.Width(70))) {
                        CreateFolder(folderName);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }


            GUILayout.Space(5);

            if (GUILayout.Button("Create All Folders", GUILayout.Height(30))) {
                foreach (string folder in FolderNames) {
                    CreateFolder(folder);
                }
            }
            GUILayout.Space(20); // Platz schaffen
        }

        // Erstellt einen Ordner
        private void CreateFolder(string folderName) {
            string fullPath = Path.Combine(Application.dataPath, folderName);
            if (!Directory.Exists(fullPath)) {
                Directory.CreateDirectory(fullPath);
            }
            AssetDatabase.Refresh();
        }
        
        public void SaveFolderNames(PackageInstaller installer) {
            if (string.IsNullOrEmpty(installer.PackageFolder)) {
                Debug.LogError("PackageFolder is not set. Cannot save FolderNames.txt.");
                return;
            }

            // Pfad zur FolderNames.txt im PackageFolder
            string filePath = Path.Combine(installer.PackageFolder, "FolderNames.txt");

            try {
                // Schreibe alle FolderNames als Text (eine Zeile pro Ordner)
                File.WriteAllLines(filePath, FolderNames);
            } catch (IOException ex) {
                Debug.LogError($"Error saving FolderNames.txt: {ex.Message}");
            }
        }

        public void LoadFolderNames(PackageInstaller installer) {
            if (essentialFoldersLoaded) return; // Verhindert doppeltes Laden

            if (string.IsNullOrEmpty(installer.PackageFolder)) {
                Debug.LogError("PackageFolder is not set. Cannot load FolderNames.txt.");
                return;
            }

            // Pfad zur FolderNames.txt im PackageFolder
            string filePath = Path.Combine(installer.PackageFolder, "FolderNames.txt");

            // Prüfen, ob die Datei existiert
            if (File.Exists(filePath)) {
                try {
                    // Lädt die Datei und speichert die Einträge in FolderNames
                    FolderNames = new List<string>(File.ReadAllLines(filePath));
                    essentialFoldersLoaded = true;
                }
                catch (IOException ex) {
                    Debug.LogError($"Error reading FolderNames.txt: {ex.Message}");
                }
            }
            else {
                Debug.LogWarning($"FolderNames.txt not found at {filePath}. Creating an empty file.");

                try {
                    // Erstelle eine neue, leere Datei
                    using (File.Create(filePath)) {
                    }

                    FolderNames = new List<string>(); // Initialisiere eine leere Liste
                    Debug.Log("Created new empty FolderNames.txt file.");
                }
                catch (IOException ex) {
                    Debug.LogError($"Error creating FolderNames.txt: {ex.Message}");
                }
            }
        }
    }
}

