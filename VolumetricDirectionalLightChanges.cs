using UnityEngine;

[System.Serializable]
public class VolumetricDirectionalLightChanges {
    public string Name;
    public float intensity = 1f;

    [Range(1, 64)]
    public int SampleCount = 8;
    [Range(0.0f, 1.0f)]
    public float ScatteringCoef = 0.5f;
    [Range(0.0f, 0.1f)]
    public float ExtinctionCoef = 0.01f;
    [Range(0.0f, 1.0f)]
    public float SkyboxExtinctionCoef = 0.9f;
    [Range(0.0f, 0.999f)]
    public float MieG = 0.1f;
    public bool HeightFog = false;
    [Range(0, 0.5f)]
    public float HeightScale = 0.10f;
    public float GroundLevel = 0;
    public bool Noise = false;
    public float NoiseScale = 0.015f;
    public float NoiseIntensity = 1.0f;
    public float NoiseIntensityOffset = 0.3f;
    public Vector2 NoiseVelocity = new Vector2(3.0f, 3.0f);

    [Tooltip("")]
    public float MaxRayLength = 400.0f;
}
