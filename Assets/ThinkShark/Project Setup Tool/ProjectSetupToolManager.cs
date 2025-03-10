using UnityEditor;
using UnityEngine;

namespace ThinkShark.ProjectSetupTool
{
    public class ProjectSetupToolManager : EditorWindow
    {
        private enum Tab
        {
            PackageInstaller,
            FolderManager,
            ScriptImporter
        }

        private Tab selectedTab = Tab.PackageInstaller; // Öffnet Folder Manager als ersten Tab

        private readonly PackageInstaller packageInstaller = new();
        private readonly FolderManager folderManager = new();
        private readonly ScriptImporter scriptImporter = new();

        [MenuItem("Tools/Project Setup Tool")]
        public static void ShowWindow() {
            ProjectSetupToolManager window = GetWindow<ProjectSetupToolManager>("Project Setup Tool");
            
            // Fenstermindestgröße festlegen
            window.minSize = new Vector2(500, 500); // Mindestgröße
        }

        private void OnEnable() {
            // Lädt Einstellungen, wenn das Fenster geöffnet wird
            packageInstaller.LoadPackageFolder();
            packageInstaller.LoadPackageManagerPackagesFromTxt();
            packageInstaller.LoadPackages(packageInstaller.PackageFolder);
            packageInstaller.LoadOrCreateEssentialPackagesFromTxt();
            folderManager.LoadFolderNames(packageInstaller);
            scriptImporter.LoadTemplateFolderFromTxt(packageInstaller);
            scriptImporter.LoadTargetFolderFromTxt(packageInstaller);
            scriptImporter.LoadTemplates(scriptImporter.TemplateFolder);
        }

        private void OnDisable() {
            // Speichert Einstellungen, wenn das Fenster geschlossen wird
            packageInstaller.SavePackageFolder();
            packageInstaller.SavePackageManagerPackagesToTxt();
            packageInstaller.SaveEssentialPackagesToTxt();
            folderManager.SaveFolderNames(packageInstaller);
            scriptImporter.SaveTemplateFolderToTxt(packageInstaller);
            scriptImporter.SaveTargetFolderToTxt(packageInstaller);
        }

        private void OnGUI() {
            // Tab-Interface
            selectedTab = (Tab)GUILayout.Toolbar((int)selectedTab, new[] {
                "Package Installer", "Folder Manager", "Script Importer"
            });

            // Inhalte basierend auf dem aktiven Tab
            switch (selectedTab) {
                case Tab.PackageInstaller:
                    packageInstaller.Draw();
                    break;
                case Tab.FolderManager:
                    folderManager.Draw();
                    break;
                case Tab.ScriptImporter:
                    scriptImporter.Draw();
                    break;
            }
        }
    }
}