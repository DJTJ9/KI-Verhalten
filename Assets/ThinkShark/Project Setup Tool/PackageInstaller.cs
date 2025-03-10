using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ThinkShark.ProjectSetupTool
{
    public class PackageInstaller
    {
        public string PackageFolder;

        private string[] packagePaths;
        private List<string> packageNames = new();
        private List<string> packageManagerPackages = new();
        private List<string> essentialPackages = new();
        private readonly List<bool> packageURLsSelection = new();
        private readonly List<bool> packageSelection = new();
        private bool isEditing;
        private bool selectAllPackageURLs;
        private bool selectAllPackages;

        private bool essentialsLoaded;            
        private bool packageFolderLoaded;         
        private bool packageManagerPackagesLoaded;

        private string newPackageName = ""; // Platzhalter für neuen Paketnamen

        private Vector2 scroll;
        private Vector2 packageScroll;

        public void Draw() {
            GUILayout.Label("<u>Package Installer<u>", new GUIStyle() {richText = true, normal = {textColor = Color.white}});

            GUILayout.Space(10);
            
            GUILayout.Label("Package Folder:");
            GUILayout.BeginHorizontal();

            GUIStyle packageFolderButtonStyle = new GUIStyle(GUI.skin.button);
            packageFolderButtonStyle.normal.textColor = Color.red;

            if (!packageFolderLoaded) {
                GUILayout.Label(string.IsNullOrEmpty(PackageFolder) ? "No folder selected" : PackageFolder,
                    EditorStyles.textField, GUILayout.Width(260));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Select Package Folder", packageFolderButtonStyle, GUILayout.Width(175))) {
                    string selectedPath = EditorUtility.OpenFolderPanel("Select Package Folder", PackageFolder, "");
                    if (!string.IsNullOrEmpty(selectedPath)) {
                        PackageFolder = selectedPath;
                        SavePackageFolder();
                        LoadPackages(selectedPath);
                        LoadOrCreatePackageManagerPackagesTxt();
                        LoadOrCreateEssentialPackagesFromTxt();
                        packageFolderLoaded = true;
                    }
                }
                GUILayout.EndHorizontal();
            }
            else {
                GUILayout.Label(string.IsNullOrEmpty(PackageFolder) ? "No folder selected" : PackageFolder,
                    EditorStyles.textField, GUILayout.Width(260));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Select Package Folder", GUILayout.Width(175))) {
                    string selectedPath = EditorUtility.OpenFolderPanel("Select Package Folder", PackageFolder, "");
                    if (!string.IsNullOrEmpty(selectedPath)) {
                        PackageFolder = selectedPath;
                        SavePackageFolder();
                        LoadPackages(selectedPath);
                        LoadOrCreatePackageManagerPackagesTxt();
                        LoadOrCreateEssentialPackagesFromTxt();
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();

            GUIStyle essentialButtonStyle = new GUIStyle(GUI.skin.button);
            essentialButtonStyle.normal.textColor = Color.green;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Install Essentials", essentialButtonStyle, GUILayout.Height(30),
                    GUILayout.Width(250))) {
                foreach (var package in packageNames) {
                    // Check, ob das Package in der Liste "essentialPackages" ist
                    bool isEssential = essentialPackages.Contains(package);
                    if (isEssential) {
                        if (packagePaths != null)
                            AssetDatabase.ImportPackage(packagePaths[packageNames.IndexOf(package)], false);
                    }
                }
                foreach (var package in packageManagerPackages) {
                    bool isEssential = essentialPackages.Contains(package);

                    if (isEssential) {
                        Packages.InstallPackages(new[] { package });
                    }
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(isEditing ? "Save Changes" : "Edit Package URLs", GUILayout.Width(150))) {
                isEditing = !isEditing;  // Umschalten des Bearbeitungsmodus

                if (!isEditing) {
                    SavePackageManagerPackagesToTxt();
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (!isEditing) {
                GUILayout.BeginHorizontal();
                GUILayout.Label("<u>Package URLs<u>:", new GUIStyle() {richText = true, normal = {textColor = Color.white}});
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                // Toggle für "Alle auswählen"
                bool previousSelectAllPackageURLs = selectAllPackageURLs;
                selectAllPackageURLs = EditorGUILayout.Toggle(selectAllPackageURLs, GUILayout.Width(15));
                GUILayout.Label("Select All", EditorStyles.boldLabel);
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                

                GUILayout.Space(10);

                if (selectAllPackageURLs != previousSelectAllPackageURLs) // Nur wenn sich "Select All" ändert, aktualisiere die Liste
                {
                    for (int i = 0; i < packageURLsSelection.Count; i++) {
                        packageURLsSelection[i] = selectAllPackageURLs;
                    }
                }

                // Scrollbereich für die Checkbox-Liste
                if (packagePaths != null && packagePaths.Length > 0) {
                    GUILayout.BeginVertical();

                    scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(105));

                    // Bearbeitungsliste anzeigen
                    for (int i = 0; i < packageManagerPackages.Count; i++) {
                        EditorGUILayout.BeginHorizontal();

                        // Check, ob das Package in der Liste "essentialPackages" ist
                        bool isEssential = essentialPackages.Contains(packageManagerPackages[i]);

                        // GUIStyle für Label definieren
                        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
                        labelStyle.normal.textColor = isEssential ? Color.green : Color.white;

                        // Checkbox für Auswahl
                        packageURLsSelection[i] = EditorGUILayout.Toggle(packageURLsSelection[i], GUILayout.Width(20));

                        if (GUILayout.Button(packageManagerPackages[i], labelStyle, GUILayout.ExpandWidth(true))) {
                            if (!isEssential) {
                                essentialPackages.Add(packageManagerPackages[i]);
                                SaveEssentialPackagesToTxt(); // Speichert geänderte Essentials
                            }
                            else {
                                if (essentialPackages.Contains(packageManagerPackages[i])) {
                                    essentialPackages.Remove(packageManagerPackages[i]);
                                    SaveEssentialPackagesToTxt(); // Speichert geänderte Essentials
                                }
                            }
                        }

                        if (GUILayout.Button("Install", GUILayout.Width(100))) {
                            Packages.InstallPackages(new[] { packageManagerPackages[i] });
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndScrollView();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();
                }

                GUILayout.EndHorizontal();
            }
            else {
                GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));

                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(105));

                for (int i = 0; i < packageManagerPackages.Count; i++) {
                    EditorGUILayout.BeginHorizontal();

                    // Textfeld zur Bearbeitung eines Eintrags
                    packageManagerPackages[i] = EditorGUILayout.TextField(packageManagerPackages[i]);
                    
                    if (GUILayout.Button("Remove", GUILayout.Width(100))) {  // Button zum Entfernen eines Pakets
                        packageManagerPackages.RemoveAt(i);
                        SavePackageManagerPackagesToTxt(); 
                        i--; // Schleifenindex zurücksetzen, um Überspringen zu vermeiden
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
                GUILayout.EndHorizontal();

                GUILayout.Space(20);

                GUILayout.Label("New Package URL:");
                newPackageName = EditorGUILayout.TextField(newPackageName); // TextField für neue Paketnamen

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Add New Package URL", GUILayout.Width(150))) {
                    if (!string.IsNullOrEmpty(newPackageName)) // Stelle sicher, dass der String nicht leer ist
                    {
                        packageManagerPackages.Add(newPackageName); // Paket hinzufügen
                        SavePackageManagerPackagesToTxt();          // Änderungen in die Datei speichern
                        newPackageName = "";                        // Textfeld resetten
                    }
                    else {
                        Debug.LogWarning("Package name is empty. Cannot add an empty package.");
                    }
                }
                GUILayout.EndHorizontal();
            }


            GUILayout.Space(20);

            GUILayout.Label("<u>Packages:<u>", new GUIStyle() {richText = true, normal = {textColor = Color.white}});

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            // Toggle für "Alle auswählen"
            bool previousSelectAll = selectAllPackages; // Speichert den vorherigen Status von Select All
            selectAllPackages = EditorGUILayout.Toggle(selectAllPackages, GUILayout.Width(15));
            GUILayout.Label("Select All", EditorStyles.boldLabel);

            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();

            GUILayout.Space(10);

            if (selectAllPackages != previousSelectAll) // Nur wenn sich "Select All" ändert, aktualisiere die Liste
            {
                for (int i = 0; i < packageSelection.Count; i++) {
                    packageSelection[i] = selectAllPackages;
                }
            }

            // Scrollbereich für die Checkbox-Liste
            if (packagePaths != null && packagePaths.Length > 0) {
                GUILayout.BeginVertical();

                packageScroll = EditorGUILayout.BeginScrollView(packageScroll);

                for (int i = 0; i < packageNames.Count; i++) {
                    EditorGUILayout.BeginHorizontal();

                    // Check, ob das Package in der Liste "essentialPackages" ist
                    bool isEssential = essentialPackages.Contains(packageNames[i]);

                    // GUIStyle für Label definieren
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
                    labelStyle.normal.textColor = isEssential ? Color.green : Color.white;

                    // Checkbox für Auswahl
                    packageSelection[i] = EditorGUILayout.Toggle(packageSelection[i], GUILayout.Width(20));

                    // Package Name mit Farbe (grün, falls essential)
                    if (GUILayout.Button(packageNames[i], labelStyle, GUILayout.ExpandWidth(true))) {
                        if (!isEssential) {
                            essentialPackages.Add(packageNames[i]);
                            SaveEssentialPackagesToTxt(); // Speichere geänderte Essentials
                        }
                        else {
                            if (essentialPackages.Contains(packageNames[i])) {
                                essentialPackages.Remove(packageNames[i]);
                                SaveEssentialPackagesToTxt(); // Speichere geänderte Essentials
                            }
                        }
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Install", GUILayout.Width(100))) {  // Button um Package zu installieren
                        AssetDatabase.ImportPackage(packagePaths[i], false);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
            }
            
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (GUILayout.Button("Install Selected Packages", GUILayout.Height(30))) {
                foreach (var package in packageNames) {
                    if (packageSelection[packageNames.IndexOf(package)]) {
                        if (packagePaths != null)
                            AssetDatabase.ImportPackage(packagePaths[packageNames.IndexOf(package)], false);
                    }
                }

                foreach (var package in packageManagerPackages) {
                    if (packageURLsSelection[packageManagerPackages.IndexOf(package)]) {
                        Packages.InstallPackages(new[] { package });
                    }
                }
            }

            GUILayout.Space(20);
        }

        public void LoadPackages(string path) {
            packagePaths = Directory.GetFiles(path, "*.unitypackage", SearchOption.AllDirectories);
            packageNames.Clear();
            packageURLsSelection.Clear();
            packageSelection.Clear();

            foreach (string templatePath in packagePaths) {
                string scriptName = Path.GetFileNameWithoutExtension(templatePath);
                packageNames.Add(scriptName);
                packageURLsSelection.Add(false); // Initially unselected
                packageSelection.Add(false); // Initially unselected
            }
        }

        public void SaveEssentialPackagesToTxt() {
            string txtFilePath = Path.Combine(PackageFolder, "EssentialPackages.txt");

            try {
                File.WriteAllLines(txtFilePath, essentialPackages);
            }
            catch (IOException ex) {
                Debug.LogError("Error saving EssentialPackages.txt: " + ex.Message);
            }
        }

        public void LoadOrCreateEssentialPackagesFromTxt() {
            if (essentialsLoaded) return;

            string txtFilePath = Path.Combine(PackageFolder, "EssentialPackages.txt");

            if (File.Exists(txtFilePath)) // Prüfen, ob die Datei existiert
            {
                try {
                    // Inhalt der Datei lesen und in die Liste speichern
                    essentialPackages = new List<string>(File.ReadAllLines(txtFilePath));
                    essentialsLoaded = true; // Markiere die Liste als geladen
                }
                catch (IOException ex) {
                    Debug.LogError("Error reading EssentialPackages.txt: " + ex.Message);
                }
            }
            else {
                Debug.LogWarning("EssentialPackages.txt not found in: " + PackageFolder);
                try {
                    // Leere Datei erstellen
                    using (var stream = File.Create(txtFilePath)) {
                    }

                    Debug.Log("EssentialPackages.txt file created successfully.");
                }
                catch (IOException ex) {
                    Debug.LogError("Error creating EssentialPackages.txt: " + ex.Message);
                }
            }
        }

        public void SavePackageFolder() {
            EditorPrefs.SetString("PackageFolderPath", PackageFolder);
        }

        public void LoadPackageFolder() {
            if (packageFolderLoaded) return;

            if (EditorPrefs.HasKey("PackageFolderPath")) {
                PackageFolder = EditorPrefs.GetString("PackageFolderPath");
                packageFolderLoaded = true;
            }
            else {
                Debug.Log("No saved package folder path found. Using default.");
            }
        }

        public void SavePackageManagerPackagesToTxt() {
            string txtFilePath = Path.Combine(PackageFolder, "PackageManagerPackages.txt");

            try {
                File.WriteAllLines(txtFilePath, packageManagerPackages);
            }
            catch (IOException ex) {
                Debug.LogError("Error saving PackageManagerPackages.txt: " + ex.Message);
            }
        }

        public void LoadPackageManagerPackagesFromTxt() {
            if (packageManagerPackagesLoaded) return;
            
            string txtFilePath = Path.Combine(PackageFolder, "PackageManagerPackages.txt");

            if (File.Exists(txtFilePath)) // Prüfen, ob die Datei existiert
            {
                try {
                    // Inhalt der Datei lesen und in die Liste speichern
                    packageManagerPackages = new List<string>(File.ReadAllLines(txtFilePath));
                    packageManagerPackagesLoaded = true; // Markiere die Liste als geladen
                }
                catch (IOException ex) {
                    Debug.LogError("Error reading PackageManagerPackages.txt: " + ex.Message);
                }
            }
            else {
                Debug.LogWarning("PackageManagerPackages.txt not found in: " + PackageFolder);
            }
        }

        public void LoadOrCreatePackageManagerPackagesTxt() {
            string txtFilePath = Path.Combine(PackageFolder, "PackageManagerPackages.txt");

            if (File.Exists(txtFilePath)) // Prüfen, ob die Datei existiert
            {
                try {
                    // Inhalt der Datei lesen und in die Liste speichern
                    packageManagerPackages = new List<string>(File.ReadAllLines(txtFilePath));
                    packageManagerPackagesLoaded = true; // Markiere die Liste als geladen
                }
                catch (IOException ex) {
                    Debug.LogError("Error reading PackageManagerPackages.txt: " + ex.Message);
                }
            }
            else {
                Debug.LogWarning("PackageManagerPackages.txt not found in: " + PackageFolder);
                try {
                    // Leere Datei erstellen
                    using (var stream = File.Create(txtFilePath)) {
                    }

                    Debug.Log("PackageManagerPackages.txt file created successfully.");
                }
                catch (IOException ex) {
                    Debug.LogError("Error creating PackageManagerPackages.txt: " + ex.Message);
                }
            }
        }
    }

    static class Packages
    {
        static AddRequest Request;
        static Queue<string> PackagesToInstall = new();

        public static void InstallPackages(string[] packages) {
            foreach (var package in packages) {
                PackagesToInstall.Enqueue(package);
            }

            // Startet die Installation des ersten Pakets
            if (PackagesToInstall.Count > 0) {
                Request = Client.Add(PackagesToInstall.Dequeue());
                EditorApplication.update += Progress;
            }
        }

        static async void Progress() {
            if (Request.IsCompleted) {
                if (Request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + Request.Result.packageId);
                else if (Request.Status >= StatusCode.Failure)
                    Debug.Log(Request.Error.message);

                EditorApplication.update -= Progress;

                if (PackagesToInstall.Count > 0) {
                    await Task.Delay(1000);  //Delay bevor nächstes Paket installiert wird
                    Request = Client.Add(PackagesToInstall.Dequeue());
                    EditorApplication.update += Progress;
                }
            }
        }
    }
}