using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using SoulBind.Grid.Presentation;

namespace SoulBind.Editor
{
    public static class SoulBindSceneBootstrap
    {
        #region Scene Creation

        public static void CreateGameplayScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var grid = new GameObject("Grid");
            grid.AddComponent<GridView>();
            var interaction = grid.AddComponent<GridInteractionController>();
            grid.AddComponent<GridPrototypeHud>();

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(4f, 10f, 4f);
            camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var scenePath = "Assets/Scenes/SoulBindGameplay.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
            Debug.Log($"Created Soul Bind gameplay scene at {scenePath}");
        }

        #endregion
    }
}
