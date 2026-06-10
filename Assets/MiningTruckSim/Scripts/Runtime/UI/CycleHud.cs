using MiningTruckSim.Operation;
using MiningTruckSim.Track;
using UnityEngine;

namespace MiningTruckSim.UI
{
    /// <summary>
    /// HUD provisório (IMGUI) do ciclo de operação: mostra a fase atual, o enchimento da
    /// caçamba e uma instrução contextual para o jogador (ir ao loading, estacionar,
    /// dirigir ao unload, levantar a báscula). Substituído por UI definitiva no polimento.
    /// </summary>
    public sealed class CycleHud : MonoBehaviour
    {
        public CycleDirector director;
        public RouteGuide guide;

        private GUIStyle _panel;

        private void OnGUI()
        {
            if (director == null)
            {
                return;
            }

            _panel ??= new GUIStyle(GUI.skin.box)
            {
                fontSize = 15,
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(12, 12, 10, 10)
            };

            OperationCycle c = director.Cycle;
            string text =
                $"Fase: {PhaseLabel(c.Phase)}\n" +
                $"Caçamba: {c.LoadFill * 100f:0}%\n";

            if (guide != null)
            {
                text += $"Trilho: {guide.Progress * 100f:0}%" +
                        (guide.IsOffTrack ? "  ⚠ FORA DO TRILHO" : "") + "\n" +
                        $"Penalidade: {guide.TotalPenalty:0}\n";
            }

            text += "\n" + Hint(c.Phase);

            float w = 340f;
            float h = guide != null ? 160f : 120f;
            GUI.Box(new Rect(Screen.width - w - 12, 12, w, h), text, _panel);
        }

        private static string PhaseLabel(CyclePhase phase) => phase switch
        {
            CyclePhase.Idle => "Indo para o carregamento",
            CyclePhase.ApproachingLoad => "Na área de loading",
            CyclePhase.Loading => "Carregando…",
            CyclePhase.Loaded => "Carregado — siga p/ descarga",
            CyclePhase.ApproachingUnload => "Na área de unload",
            CyclePhase.Unloading => "Descarregando…",
            CyclePhase.Done => "Ciclo concluído!",
            _ => phase.ToString()
        };

        private static string Hint(CyclePhase phase) => phase switch
        {
            CyclePhase.Idle => "→ Dirija até a plataforma azul (loading).",
            CyclePhase.ApproachingLoad => "→ Estacione alinhado no ponto e pare.",
            CyclePhase.Loading => "→ Aguarde a escavadeira encher a caçamba.",
            CyclePhase.Loaded => "→ Dirija até a plataforma laranja (unload).",
            CyclePhase.ApproachingUnload => "→ Estacione no ponto e pare.",
            CyclePhase.Unloading => "→ Pressione [B] para manter a báscula erguida.",
            CyclePhase.Done => "→ Operação do ciclo finalizada.",
            _ => string.Empty
        };
    }
}
