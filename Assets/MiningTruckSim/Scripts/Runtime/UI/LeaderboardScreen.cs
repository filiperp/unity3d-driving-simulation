using System.Text;
using MiningTruckSim.Net;
using UnityEngine;

namespace MiningTruckSim.UI
{
    /// <summary>
    /// Tela de leaderboard (critério 7): lê o ranking por pontuação total do backend e o
    /// exibe. Pode ser atualizada a qualquer momento via <see cref="Refresh"/> (ex.: após
    /// finalizar uma operação). IMGUI provisório.
    /// </summary>
    public sealed class LeaderboardScreen : MonoBehaviour
    {
        public MiningApiClient api;
        public bool refreshOnStart = true;

        private LeaderboardEntryDto[] _entries = System.Array.Empty<LeaderboardEntryDto>();
        private string _status = "";
        private Vector2 _scroll;
        private GUIStyle _title;

        private void Start()
        {
            if (refreshOnStart)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            if (api == null)
            {
                return;
            }

            _status = "Carregando…";
            api.GetLeaderboard(
                entries =>
                {
                    _entries = entries;
                    _status = _entries.Length == 0 ? "Sem partidas ainda." : "";
                },
                err => _status = "API offline. " + err);
        }

        private void OnGUI()
        {
            _title ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            const float w = 420f;
            const float h = 360f;
            var rect = new Rect(Screen.width - w - 20, 20, w, h);
            GUI.Box(rect, GUIContent.none);

            GUILayout.BeginArea(new Rect(rect.x + 16, rect.y + 12, rect.width - 32, rect.height - 24));
            GUILayout.Label("Leaderboard", _title);
            GUILayout.Space(6);

            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(250));
            var sb = new StringBuilder();
            int pos = 1;
            foreach (LeaderboardEntryDto e in _entries)
            {
                sb.AppendLine($"{pos,2}. {e.display_name,-16} {e.total_score,8:0}  ({e.mine})");
                pos++;
            }

            GUILayout.Label(sb.ToString());
            GUILayout.EndScrollView();

            GUILayout.Label(_status);
            if (GUILayout.Button("Atualizar"))
            {
                Refresh();
            }

            GUILayout.EndArea();
        }
    }
}
