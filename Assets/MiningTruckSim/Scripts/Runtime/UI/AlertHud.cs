using MiningTruckSim.Alerts;
using UnityEngine;

namespace MiningTruckSim.UI
{
    /// <summary>
    /// HUD dos alertas ativos (critério 9): lista cada alerta com seu título, o
    /// procedimento de conserto e uma barra de progresso do reparo. IMGUI provisório.
    /// </summary>
    public sealed class AlertHud : MonoBehaviour
    {
        public AlertSystem alertSystem;

        private GUIStyle _box;
        private GUIStyle _title;
        private Texture2D _barBg;
        private Texture2D _barFill;

        private void OnGUI()
        {
            if (alertSystem == null || alertSystem.ActiveAlerts.Count == 0)
            {
                return;
            }

            _box ??= new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(12, 12, 8, 8),
                wordWrap = true
            };
            _title ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.4f, 0.3f) }
            };
            _barBg ??= SolidTex(new Color(0f, 0f, 0f, 0.5f));
            _barFill ??= SolidTex(new Color(0.3f, 0.9f, 0.4f, 0.9f));

            const float w = 360f;
            float y = 140f;
            foreach (ActiveAlert alert in alertSystem.ActiveAlerts)
            {
                var rect = new Rect(12, y, w, 92);
                GUI.Box(rect, GUIContent.none, _box);
                GUI.Label(new Rect(rect.x + 10, rect.y + 6, w - 20, 22),
                    "⚠ " + alert.Title, _title);
                GUI.Label(new Rect(rect.x + 10, rect.y + 30, w - 20, 38), alert.Instruction, _box);

                // Barra de progresso do reparo.
                var barRect = new Rect(rect.x + 10, rect.yMax - 16, w - 20, 8);
                GUI.DrawTexture(barRect, _barBg);
                var fill = new Rect(barRect.x, barRect.y, barRect.width * alert.RepairProgress01, barRect.height);
                GUI.DrawTexture(fill, _barFill);

                y += 100f;
            }
        }

        private static Texture2D SolidTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }
    }
}
