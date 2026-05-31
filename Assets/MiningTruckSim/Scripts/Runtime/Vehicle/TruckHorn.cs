using UnityEngine;

namespace MiningTruckSim.Vehicle
{
    /// <summary>Buzina (tecla [H]) com clipe de áudio gerado proceduralmente (critério 5).</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class TruckHorn : MonoBehaviour
    {
        public KeyCode hornKey = KeyCode.H;

        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.clip = HornClip.Generate();
            _source.loop = true;
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
        }

        private void Update()
        {
            if (Input.GetKeyDown(hornKey))
            {
                _source.Play();
            }
            else if (Input.GetKeyUp(hornKey))
            {
                _source.Stop();
            }
        }
    }

    /// <summary>Gera um clipe de buzina (duas senoides) sem precisar de asset de áudio.</summary>
    public static class HornClip
    {
        public static AudioClip Generate(float freq = 210f, float seconds = 1f, int sampleRate = 44100)
        {
            int samples = (int)(seconds * sampleRate);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t)
                             + 0.5f * Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t);
                data[i] = 0.35f * wave;
            }

            AudioClip clip = AudioClip.Create("TruckHorn", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
