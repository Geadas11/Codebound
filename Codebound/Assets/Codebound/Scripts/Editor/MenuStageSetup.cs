using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Codebound.EditorTools
{
    /// <summary>
    /// One-off, idempotent setup for the 3D main-menu stage: the game draws with URP's 2D Renderer, which does not light
    /// 3D meshes, so the menu camera uses a second renderer (Universal, Forward+) added to the pipeline asset.
    /// Also lets spot lights cast shadows and turns on soft shadows (the 2D renderer ignores both).
    /// Run: Codebound/Setup menu 3D stage, or MenuStageSetup.Run() from script eval.
    /// </summary>
    public static class MenuStageSetup
    {
        public const string RendererPath = "Assets/Settings/MenuRenderer3D.asset";

        [MenuItem("Codebound/Setup menu 3D stage")]
        public static string Run()
        {
            var pipeline = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset
                           ?? QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            if (pipeline == null) return "no URP asset";

            var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (data == null)
            {
                var create = typeof(UniversalRenderPipelineAsset).GetMethod("CreateRendererAsset", BindingFlags.NonPublic | BindingFlags.Static);
                data = (UniversalRendererData)create.Invoke(null, new object[] { RendererPath, RendererType.UniversalRenderer, false, "Renderer" });
            }
            data.renderingMode = RenderingMode.ForwardPlus;
            EditorUtility.SetDirty(data);

            var so = new SerializedObject(pipeline);
            var list = so.FindProperty("m_RendererDataList");
            int index = -1;
            for (int i = 0; i < list.arraySize; i++) if (list.GetArrayElementAtIndex(i).objectReferenceValue == data) index = i;
            if (index < 0)
            {
                index = list.arraySize;
                list.InsertArrayElementAtIndex(index);
                list.GetArrayElementAtIndex(index).objectReferenceValue = data;
            }
            so.FindProperty("m_AdditionalLightShadowsSupported").boolValue = true;
            so.FindProperty("m_SoftShadowsSupported").boolValue = true;
            so.FindProperty("m_AdditionalLightsPerObjectLimit").intValue = 8;
            so.FindProperty("m_ShadowDistance").floatValue = 70;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(pipeline);
            AssetDatabase.SaveAssets();
            return "menu renderer index " + index + "; " + ConfigureFactoryImport();
        }

        public const string FactoryPath = "Assets/Codebound/Resources/MenuStage/menu_factory.fbx";

        /// <summary>The factory model (Tools/art/menu_stage.py): lights on, cameras off, no animation, materials from the file.</summary>
        public static string ConfigureFactoryImport()
        {
            var imp = AssetImporter.GetAtPath(FactoryPath) as ModelImporter;
            if (imp == null) return "factory model not found";
            imp.importLights = true; imp.importCameras = false; imp.importAnimation = false;
            imp.useFileScale = true; imp.globalScale = 1;
            imp.importNormals = ModelImporterNormals.Import;
            imp.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            imp.isReadable = false;
            imp.SaveAndReimport();
            return "factory import configured";
        }
    }
}
