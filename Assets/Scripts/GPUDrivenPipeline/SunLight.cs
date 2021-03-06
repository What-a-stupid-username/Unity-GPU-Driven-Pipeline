﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using MPipeline;
[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class SunLight : MonoBehaviour
{
    public static SunLight current = null;
    public bool enableShadow = true;
    public int resolution;
    public float firstLevelDistance = 10;
    public float secondLevelDistance = 25;
    public float thirdLevelDistance = 55;
    public float farestDistance = 100;
    public Vector4 bias = new Vector4(0.01f, 0.02f, 0.04f, 0.08f);
    public Vector4 normalBias = new Vector4(0.001f, 0.002f, 0.003f, 0.005f);
    public Vector4 cascadeSoftValue = new Vector4(1.5f, 1.2f, 0.9f, 0.7f);
    [System.NonSerialized] public Camera cameraComponent;
    [System.NonSerialized] public OrthoCam shadCam;
    [System.NonSerialized] public Material shadowDepthMaterial;
    [System.NonSerialized] public RenderTexture shadowmapTexture;
    [System.NonSerialized] public NativeArray<AspectInfo> shadowFrustumPlanes;
    [System.NonSerialized] public Light light;
    public static Camera shadowCam;
    private void OnEnable()
    {
        if (current)
        {
            if (current != this)
            {
                Debug.Log("Sun Light Should be Singleton!");
                Destroy(light);
                Destroy(this);
                return;
            }
            else
                OnDisable();
        }
        light = GetComponent<Light>();
        current = this;
        light.enabled = false;
        if(!shadowCam)
        {
            shadowCam = GetComponent<Camera>();
            if(!shadowCam)
            {
                shadowCam = gameObject.AddComponent<Camera>();
            }
            shadowCam.enabled = false;
            shadowCam.hideFlags = HideFlags.HideInInspector;
            shadowCam.aspect = 1;
            
        }
        shadowmapTexture = new RenderTexture(new RenderTextureDescriptor
        {
            width = resolution,
            height = resolution,
            depthBufferBits = 16,
            colorFormat = RenderTextureFormat.RFloat,
            autoGenerateMips = false,
            bindMS = false,
            dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray,
            enableRandomWrite = false,
            memoryless = RenderTextureMemoryless.None,
            shadowSamplingMode = UnityEngine.Rendering.ShadowSamplingMode.None,
            msaaSamples = 1,
            sRGB = false,
            useMipMap = false,
            volumeDepth = 4,
            vrUsage = VRTextureUsage.None
        });
        cameraComponent = shadowCam;
        shadowmapTexture.filterMode = FilterMode.Point;
        shadowDepthMaterial = new Material(Shader.Find("Hidden/ShadowDepth"));
        shadowFrustumPlanes = new NativeArray<AspectInfo>(3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
    }

    private void Update()
    {
        shadCam.forward = transform.forward;
        shadCam.up = transform.up;
        shadCam.right = transform.right;
    }

    private void OnDisable()
    {
        if (current != this) return;
        current = null;
        if (shadowmapTexture)
        {
            shadowmapTexture.Release();
            DestroyImmediate(shadowmapTexture);
        }
        if(shadowDepthMaterial)
            DestroyImmediate(shadowDepthMaterial);
        if(shadowFrustumPlanes.IsCreated)
            shadowFrustumPlanes.Dispose();
    }
}
