using UnityEngine;

namespace MiningTruckSim.Common
{
    /// <summary>Cria materiais URP (com fallback) em runtime para a arte procedural.</summary>
    public static class MaterialFactory
    {
        private static Shader _litShader;

        public static Material Create(Color color, float metallic = 0f, float smoothness = 0.3f)
        {
            _litShader ??= Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(_litShader);

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            else
            {
                mat.color = color;
            }

            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", metallic);
            }

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", smoothness);
            }

            return mat;
        }
    }
}
