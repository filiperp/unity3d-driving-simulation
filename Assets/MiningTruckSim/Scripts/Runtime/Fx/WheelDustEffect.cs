using MiningTruckSim.Vehicle;
using UnityEngine;

namespace MiningTruckSim.Fx
{
    /// <summary>
    /// Poeira procedural levantada pelas rodas (polimento visual, S9). Cria um
    /// <see cref="ParticleSystem"/> em runtime (sem asset) e modula a emissão pela
    /// velocidade do caminhão: parado não emite, em movimento levanta poeira. Pensado
    /// para ficar sob o slot de FX e ser desativado/trocado quando os modelos chegarem.
    /// </summary>
    public sealed class WheelDustEffect : MonoBehaviour
    {
        public TruckController truck;

        [Tooltip("Velocidade (km/h) a partir da qual a poeira começa a aparecer.")]
        public float minSpeedKmh = 5f;

        [Tooltip("Velocidade (km/h) em que a emissão chega ao máximo.")]
        public float maxSpeedKmh = 40f;

        public float maxEmissionRate = 60f;
        public Color dustColor = new Color(0.55f, 0.48f, 0.38f, 0.5f);

        private ParticleSystem _ps;
        private ParticleSystem.EmissionModule _emission;

        private void Awake()
        {
            BuildSystem();
        }

        private void Update()
        {
            if (truck == null)
            {
                return;
            }

            float speed = truck.SpeedKmh;
            float t = Mathf.InverseLerp(minSpeedKmh, maxSpeedKmh, speed);
            // Só emite com o caminhão de fato em contato/rodando.
            _emission.rateOverTime = t * maxEmissionRate;
        }

        private void BuildSystem()
        {
            _ps = gameObject.GetComponent<ParticleSystem>();
            if (_ps == null)
            {
                _ps = gameObject.AddComponent<ParticleSystem>();
            }

            // O sistema precisa estar parado antes de configurar os módulos.
            _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = _ps.main;
            main.startLifetime = 1.6f;
            main.startSpeed = 0.6f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.6f, 1.6f);
            main.startColor = dustColor;
            main.gravityModifier = -0.04f; // sobe levemente
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 200;

            _emission = _ps.emission;
            _emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = _ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.5f;

            // Esmaece com o tempo de vida.
            ParticleSystem.ColorOverLifetimeModule col = _ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(dustColor, 0f), new GradientColorKey(dustColor, 1f) },
                new[] { new GradientAlphaKey(dustColor.a, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            // Renderer com material URP particles/unlit (fallback se ausente).
            var renderer = _ps.GetComponent<ParticleSystemRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                            ?? Shader.Find("Sprites/Default");
            if (shader != null)
            {
                renderer.material = new Material(shader);
            }

            _ps.Play();
        }
    }
}
