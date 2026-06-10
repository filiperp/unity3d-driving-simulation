using MiningTruckSim.Scoring;
using UnityEngine;

namespace MiningTruckSim.UI
{
    /// <summary>
    /// Tela de resultado de ciclo (critério 6). Exibida quando um <see cref="CycleScore"/>
    /// é apresentado via <see cref="Show"/>: pontuação final, classificação, tempo na
    /// faixa perfeita e penalidades. HUD provisório (IMGUI), trocado na fase de polimento.
    /// </summary>
    public sealed class CycleResultScreen : MonoBehaviour
    {
        private bool _visible;
        private CycleScore _score;
        private GUIStyle _panel;
        private GUIStyle _title;

        public bool Visible => _visible;

        public void Show(CycleScore score)
        {
            _score = score;
            _visible = true;
        }

        public void Hide() => _visible = false;

        private void OnGUI()
        {
            if (!_visible)
            {
                return;
            }

            _panel ??= new GUIStyle(GUI.skin.box)
            {
                fontSize = 16,
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(16, 16, 14, 14)
            };
            _title ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            const float w = 420f;
            const float h = 250f;
            var rect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);

            GUI.Box(rect, GUIContent.none, _panel);
            GUI.Label(new Rect(rect.x, rect.y + 12, rect.width, 34),
                $"Ciclo {_score.CycleIndex + 1} — Rating {_score.Rating}", _title);

            string body =
                $"Pontuação final: {_score.Final:0}\n" +
                $"Base: {_score.BaseScore:0}   Penalidades: -{_score.Penalties:0}\n" +
                $"Tempo na faixa perfeita: {_score.BandRatio * 100f:0}%\n" +
                $"Saídas do trilho: {_score.OffTrackEvents}\n" +
                $"Alertas resolvidos: {_score.AlertsHandled}\n" +
                $"Carga transportada: {_score.LoadTonnes:0} t";
            GUI.Label(new Rect(rect.x + 20, rect.y + 56, rect.width - 40, 150), body);

            if (GUI.Button(new Rect(rect.x + rect.width * 0.5f - 70, rect.yMax - 46, 140, 32),
                    "Continuar"))
            {
                Hide();
            }
        }
    }
}
