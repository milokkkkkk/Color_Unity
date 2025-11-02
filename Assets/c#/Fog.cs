using UnityEngine;
[ExecuteInEditMode]
public class Fog : MonoBehaviour
{
    [Header("是否启用雾效")]
    public bool enable;
    [Header("雾效颜色")]
    public Color fogColor;
    [Header("雾气基准高度")]
    public float fogHeight;
    [Header("雾气密度")]
    [Range(0,1)]public float fogDensity;
    [Header("雾气随高度衰减系数")]
    [Min(0f)]public float fogFalloff;
    [Header("雾气开始距离")]
    public float fogStartDis;
    [Header("光线散射指数")]
    public float fogInscatteringExp;
    [Header("雾效渐变距离")]
    public float fogGradientDis;
    private static readonly int FogColor = Shader.PropertyToID("_FogColor");
    private static readonly int FogGlobalDensity = Shader.PropertyToID("_FogGlobalDensity");
    private static readonly int FogFallOff = Shader.PropertyToID("_FogFallOff");
    private static readonly int FogHeight = Shader.PropertyToID("_FogHeight");
    private static readonly int FogStartDis = Shader.PropertyToID("_FogStartDis");
    private static readonly int FogInscatteringExp = Shader.PropertyToID("_FogInscatteringExp");
    private static readonly int FogGradientDis = Shader.PropertyToID("_FogGradientDis");

    void OnValidate()
    {
        Shader.SetGlobalColor(FogColor, fogColor);
        Shader.SetGlobalFloat(FogGlobalDensity, fogDensity);
        Shader.SetGlobalFloat(FogFallOff, fogFalloff);
        Shader.SetGlobalFloat(FogHeight, fogHeight);
        Shader.SetGlobalFloat(FogStartDis, fogStartDis);
        Shader.SetGlobalFloat(FogInscatteringExp, fogInscatteringExp);
        Shader.SetGlobalFloat(FogGradientDis,fogGradientDis);
        if (enable)
        {
            Shader.EnableKeyword("_FOG_ON");
            Shader.DisableKeyword("_FOG_OFF");
        }
        else
        {
            Shader.DisableKeyword("_FOG_ON");
            Shader.EnableKeyword("_FOG_OFF");
        }
    }
}