using MiningTruckSim.Config;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiningTruckSim.UI
{
    /// <summary>
    /// Menu de configuração da operação (critério 8): escolher entre as 2 minas (fácil/
    /// difícil) e o número de ciclos (N, configurável). Grava em <see cref="OperationContext"/>
    /// e carrega a cena de operação. IMGUI provisório (trocado no polimento).
    /// </summary>
    public sealed class OperationSetupMenu : MonoBehaviour
    {
        [Tooltip("Nome da cena de operação a carregar. Se vazio, apenas grava o contexto.")]
        public string operationSceneName = "";

        private int _mineIndex;
        private int _cycles = MineCatalog.Easy.RecommendedCycles;
        private GUIStyle _title;

        private void OnGUI()
        {
            _title ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            const float w = 460f;
            const float h = 320f;
            var rect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(rect, GUIContent.none);

            GUILayout.BeginArea(new Rect(rect.x + 20, rect.y + 16, rect.width - 40, rect.height - 32));
            GUILayout.Label("Configurar operação", _title);
            GUILayout.Space(12);

            GUILayout.Label("Mina:");
            string[] names = { MineCatalog.Easy.Name, MineCatalog.Hard.Name };
            _mineIndex = GUILayout.SelectionGrid(_mineIndex, names, 1);

            MineConfig mine = _mineIndex == 1 ? MineCatalog.Hard : MineCatalog.Easy;
            GUILayout.Space(6);
            GUILayout.Label(
                $"Rota ~{mine.RouteLengthM:0} m · {mine.CurveCount} curvas · " +
                $"trilho ±{mine.OffTrackToleranceM:0} m · alertas {mine.AlertsPerMinute:0.00}/min");

            GUILayout.Space(12);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Ciclos (N): {_cycles}", GUILayout.Width(120));
            if (GUILayout.Button("-", GUILayout.Width(36)) && _cycles > 1)
            {
                _cycles--;
            }

            if (GUILayout.Button("+", GUILayout.Width(36)) && _cycles < 20)
            {
                _cycles++;
            }

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Iniciar operação", GUILayout.Height(40)))
            {
                StartOperation(mine);
            }

            GUILayout.EndArea();
        }

        private void StartOperation(MineConfig mine)
        {
            OperationContext.Config = new OperationConfig(mine, _cycles).Validated();
            if (!string.IsNullOrEmpty(operationSceneName))
            {
                SceneManager.LoadScene(operationSceneName);
            }
        }
    }
}
