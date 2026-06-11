using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MiningTruckSim.Editor
{
    /// <summary>
    /// Build em linha de comando (S9), para CI/headless. Exemplo:
    ///
    ///   Unity -batchmode -quit -projectPath . \
    ///     -executeMethod MiningTruckSim.Editor.BuildScript.BuildLinux64 \
    ///     -buildOutput Builds/Linux/MiningTruckSim.x86_64
    ///
    /// As cenas incluídas são as marcadas em Build Settings; se nenhuma, usa todas as
    /// .unity sob Assets/ (útil enquanto a cena é montada por bootstrap procedural).
    /// </summary>
    public static class BuildScript
    {
        [MenuItem("MiningTruckSim/Build/Windows64")]
        public static void BuildWindows64() => Build(BuildTarget.StandaloneWindows64,
            "Builds/Windows/MiningTruckSim.exe");

        [MenuItem("MiningTruckSim/Build/Linux64")]
        public static void BuildLinux64() => Build(BuildTarget.StandaloneLinux64,
            "Builds/Linux/MiningTruckSim.x86_64");

        public static void Build(BuildTarget target, string defaultOutput)
        {
            string output = ArgValue("-buildOutput") ?? defaultOutput;

            var options = new BuildPlayerOptions
            {
                scenes = ResolveScenes(),
                locationPathName = output,
                target = target,
                options = BuildOptions.None
            };

            UnityEditor.Build.Reporting.BuildReport report =
                BuildPipeline.BuildPlayer(options);
            UnityEditor.Build.Reporting.BuildSummary summary = report.summary;

            Debug.Log($"[Build] {target} -> {output} : {summary.result} " +
                      $"({summary.totalSize} bytes, {summary.totalErrors} erros)");

            if (summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
        }

        private static string[] ResolveScenes()
        {
            string[] fromSettings = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (fromSettings.Length > 0)
            {
                return fromSettings;
            }

            // Fallback: todas as cenas do projeto.
            return AssetDatabase.FindAssets("t:Scene")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.StartsWith("Assets/"))
                .ToArray();
        }

        private static string ArgValue(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                {
                    return args[i + 1];
                }
            }

            return null;
        }
    }
}
