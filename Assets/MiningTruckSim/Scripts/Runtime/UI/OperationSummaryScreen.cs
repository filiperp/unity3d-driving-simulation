using System.Text;
using MiningTruckSim.Config;
using MiningTruckSim.Scoring;
using UnityEngine;

namespace MiningTruckSim.UI
{
    /// <summary>
    /// Resumo final da operação após os N ciclos (critério 8): mina, ciclos, pontuação
    /// total agregada, média de tempo na faixa perfeita e a lista de ciclos. É a tela que
    /// antecede o envio do resultado ao backend (critério 7, Sprint 7). IMGUI provisório.
    /// </summary>
    public sealed class OperationSummaryScreen : MonoBehaviour
    {
        private bool _visible;
        private OperationProgress _progress;
        private GUIStyle _title;
        private Vector2 _scroll;

        public bool Visible => _visible;

        public void Show(OperationProgress progress)
        {
            _progress = progress;
            _visible = true;
        }

        public void Hide() => _visible = false;

        private void OnGUI()
        {
            if (!_visible || _progress == null)
            {
                return;
            }

            _title ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            const float w = 480f;
            const float h = 380f;
            var rect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(rect, GUIContent.none);

            GUILayout.BeginArea(new Rect(rect.x + 20, rect.y + 16, rect.width - 40, rect.height - 32));
            GUILayout.Label("Operação concluída", _title);
            GUILayout.Space(8);

            GUILayout.Label($"Mina: {_progress.Config.Mine.Name}");
            GUILayout.Label($"Ciclos: {_progress.CompletedCycles}/{_progress.TotalCycles}");
            GUILayout.Label($"Pontuação total: {_progress.TotalScore:0}");
            GUILayout.Label($"Tempo médio na faixa perfeita: {_progress.AverageBandRatio * 100f:0}%");
            GUILayout.Space(8);

            GUILayout.Label("Ciclos:");
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(150));
            var sb = new StringBuilder();
            foreach (CycleScore r in _progress.Results)
            {
                sb.AppendLine(
                    $"  Ciclo {r.CycleIndex + 1}: {r.Final:0} pts  [{r.Rating}]  " +
                    $"faixa {r.BandRatio * 100f:0}%  trilho {r.OffTrackEvents}x");
            }

            GUILayout.Label(sb.ToString());
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Fechar", GUILayout.Height(34)))
            {
                Hide();
            }

            GUILayout.EndArea();
        }
    }
}
