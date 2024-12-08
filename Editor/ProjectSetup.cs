using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

using static System.IO.Path;
using static UnityEditor.AssetDatabase;

public static class ProjectSetup
{
    #region ImportAssets

    [MenuItem("Tools/Setup/Import Essential Assets")]
    public static void ImportEssentials()
    {
        Assets.ImportAssets("DOTween HOTween v2.unitypackage", "Demigiant/Editor ExtensionsAnimation");
        Assets.ImportAssets("TimeScale Toolbar.unitypackage", "bl4st/Editor ExtensionsUtilities");
        Assets.ImportAssets("Hot Reload Edit Code Without Compiling.unitypackage", "The Naughty Cult/Editor ExtensionsUtilities");
        Assets.ImportAssets("Selection History.unitypackage", "Staggart Creations/Editor ExtensionsUtilities");
        Assets.ImportAssets("Editor Console Pro.unitypackage", "FlyingWorm/Editor ExtensionsSystem");
    }
    
    [MenuItem("Tools/Setup/Import Essential UI Assets")]
    public static void ImportUIEssentials()
    {
        Assets.ImportAssets("Text Animator for Unity.unitypackage", "Febucci Tools/ScriptingGUI");
        Assets.ImportAssets("Modern Procedural UI Kit.unitypackage", "Scrollbie Studio/ScriptingGUI");
    }
    
    [MenuItem("Tools/Setup/Import Essential Three D Assets")]
    public static void Import3DEssentials()
    {
        Assets.ImportAssets("Gridbox Prototype Materials.unitypackage", "Ciathyza/Textures Materials");
        Assets.ImportAssets("Robot Kyle URP.unitypackage", "Unity Technologies/3D ModelsCharactersRobots");
        Assets.ImportAssets("Easy Character Movement.unitypackage", "Oscar Gracin/Complete ProjectsSystems");
    }

    #endregion ImportAssets
    
    [MenuItem("Tools/Setup/Install Essential Packages")]
    public static void InstallPackages() {
        
        Packages.InstallPackages(new[] {
            "com.unity.2d.animation",
            "com.unity.2d.sprite",
            "git+https://github.com/adammyhre/Unity-Utils.git",
            // If necessary, import new Input System last as it requires a Unity Editor restart
            // "com.unity.inputsystem"
        });
    }
    
    [MenuItem("Tools/Setup/Create Folders")]
    public static void CreateFolders()
    {
        Folders.Create("_Project", "Animation", "Art", "Art/Materials", "Prefabs", "Scripts", "Audio", "Audio/SFX",
            "Audio/Music", "Plugins", "Scenes", "ScriptableObjects");
        Refresh();
        Folders.Move("_Project", "Scenes");
        Folders.Move("_Project", "Settings");
        Folders.Delete("TutorialInfo");
        Refresh();

        MoveAsset("Assets/InputSystem_Actions.inputactions", "Assets/_Project/Settings/InputSystem_Actions.inputactions");
        DeleteAsset("Assets/Readme.asset");
        Refresh();
        
        // Optional: Disable Domain Reload
        // EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
    }

    static class Assets
    {
        public static void ImportAssets(string asset, string folder)
        {
            string basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string assetFolder = Combine(basePath, "Unity/Asset Store-5.x");

            ImportPackage(Combine(assetFolder, folder, asset), false);
        }
    }
    
    static class Packages
    {
        static AddRequest request;
        static Queue<string> packagesToInstall = new Queue<string>();

        public static void InstallPackages(string[] packages) {
            foreach (var package in packages) {
                packagesToInstall.Enqueue(package);
            }

            if (packagesToInstall.Count > 0) {
                StartNextPackageInstallation();
            }
        }

        static async void StartNextPackageInstallation() {
            request = Client.Add(packagesToInstall.Dequeue());
            
            while (!request.IsCompleted) await Task.Delay(10);
            
            if (request.Status == StatusCode.Success) Debug.Log("Installed: " + request.Result.packageId);
            else if (request.Status >= StatusCode.Failure) Debug.LogError(request.Error.message);

            if (packagesToInstall.Count > 0) {
                await Task.Delay(1000);
                StartNextPackageInstallation();
            }
        }
    }
    
    static class Folders {
        public static void Create(string root, params string[] folders) {
            var fullpath = Combine(Application.dataPath, root);
            if (!Directory.Exists(fullpath)) {
                Directory.CreateDirectory(fullpath);
            }

            foreach (var folder in folders) {
                CreateSubFolders(fullpath, folder);
            }
        }
        
        static void CreateSubFolders(string rootPath, string folderHierarchy) {
            var folders = folderHierarchy.Split('/');
            var currentPath = rootPath;

            foreach (var folder in folders) {
                currentPath = Combine(currentPath, folder);
                if (!Directory.Exists(currentPath)) {
                    Directory.CreateDirectory(currentPath);
                }
            }
        }
        
        public static void Move(string newParent, string folderName) {
            var sourcePath = $"Assets/{folderName}";
            if (IsValidFolder(sourcePath)) {
                var destinationPath = $"Assets/{newParent}/{folderName}";
                var error = MoveAsset(sourcePath, destinationPath);

                if (!string.IsNullOrEmpty(error)) {
                    Debug.LogError($"Failed to move {folderName}: {error}");
                }
            }
        }
        
        public static void Delete(string folderName) {
            var pathToDelete = $"Assets/{folderName}";

            if (IsValidFolder(pathToDelete)) {
                DeleteAsset(pathToDelete);
            }
        }
    }
}
