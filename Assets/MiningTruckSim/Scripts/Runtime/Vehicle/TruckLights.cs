using System.Collections.Generic;
using UnityEngine;

namespace MiningTruckSim.Vehicle
{
    /// <summary>Liga/desliga os faróis com a tecla [L] (critério 5).</summary>
    public sealed class TruckLights : MonoBehaviour
    {
        public List<Light> headlights = new List<Light>();
        public KeyCode toggleKey = KeyCode.L;
        public bool On { get; private set; }

        private void Start() => Apply();

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                On = !On;
                Apply();
            }
        }

        private void Apply()
        {
            foreach (Light l in headlights)
            {
                if (l != null)
                {
                    l.enabled = On;
                }
            }
        }
    }
}
