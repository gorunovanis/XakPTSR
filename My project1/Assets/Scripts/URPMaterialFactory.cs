using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class URPMaterialFactory
{
    public static Material Create(Color color, float smoothness = 0.5f)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        mat.SetFloat("_Smoothness", smoothness);
        return mat;
    }
}
