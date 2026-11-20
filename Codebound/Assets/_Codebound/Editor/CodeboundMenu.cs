using System.Linq;
using Codebound.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Codebound.EditorTools
{
    /// <summary>Editor helpers. "Codebound/Create Test Scene" creates and registers Scenes/TestWorld.unity.</summary>
    public static class CodeboundMenu
    {
        private const string ScenePath = "Assets/_Codebound/Scenes/TestWorld.unity";

        [MenuItem("Codebound/Create Test Scene")]
        public static void CreateTestScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            camGo.transform.position = new Vector3(0, 0, -10);
            new GameObject("Codebound").AddComponent<GameBootstrap>();
            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(s => s.path != ScenePath))
            {
                scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
            Debug.Log("[Codebound] Test scene created at " + ScenePath + ". Press Play.");
        }
    }
}
