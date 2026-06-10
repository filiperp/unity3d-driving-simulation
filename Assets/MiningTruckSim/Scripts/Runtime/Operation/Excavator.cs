using UnityEngine;

namespace MiningTruckSim.Operation
{
    /// <summary>
    /// Escavadeira procedural com animação de carregamento (critério 2). Quando
    /// <see cref="Loading"/> está ativo, a lança/concha oscila num ciclo de escavar →
    /// girar → despejar, deixando claro visualmente que o caminhão está sendo carregado.
    /// A geometria fica sob um slot trocável pelos modelos comprados na Sprint 8.
    /// </summary>
    public sealed class Excavator : MonoBehaviour
    {
        [Tooltip("Articulação que gira a cabine/lança da escavadeira (eixo Y).")]
        public Transform swing;

        [Tooltip("Articulação da concha que sobe/desce (eixo X).")]
        public Transform boom;

        public float swingSpeed = 60f;     // graus/seg
        public float swingRange = 70f;     // amplitude do giro
        public float boomRange = 35f;      // amplitude da concha
        public float cycleSpeed = 1.2f;    // velocidade do ciclo de carregamento

        public bool Loading { get; set; }

        private float _t;
        private Quaternion _swingRest;
        private Quaternion _boomRest;

        private void Awake()
        {
            if (swing != null)
            {
                _swingRest = swing.localRotation;
            }

            if (boom != null)
            {
                _boomRest = boom.localRotation;
            }
        }

        private void Update()
        {
            if (!Loading)
            {
                return;
            }

            _t += Time.deltaTime * cycleSpeed;

            // Giro de ida-e-volta entre a frente de lavra e o caminhão.
            if (swing != null)
            {
                float yaw = Mathf.Sin(_t) * swingRange * 0.5f;
                swing.localRotation = _swingRest * Quaternion.Euler(0f, yaw, 0f);
            }

            // Concha mergulha e levanta em sincronia com o giro.
            if (boom != null)
            {
                float pitch = (Mathf.Cos(_t * 2f) * 0.5f + 0.5f) * boomRange;
                boom.localRotation = _boomRest * Quaternion.Euler(pitch, 0f, 0f);
            }
        }
    }
}
