using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class TestComputeShaderHandler : MonoBehaviour
{
    [Range(1, 1024)]
    public int numElements = 5;
    public float scale = 1f;
    public float speed = 1f;
    public ComputeShader computeShader;

    public Mesh mesh;
    public Material material;
    
    private ComputeBuffer d_positionsBuffer;
    private int k_kernelIndex;

    private static readonly int
        IDpositionsBuffer = Shader.PropertyToID("_Positions"),
        IDnumElements = Shader.PropertyToID("_NumElements"),
        IDtime = Shader.PropertyToID("_Time"),
        IDspeed = Shader.PropertyToID("_Speed");

    private Vector3[] temp = new Vector3[5];

    void OnEnable()
    {
        d_positionsBuffer = new ComputeBuffer(numElements, 3*sizeof(float));
        k_kernelIndex = computeShader.FindKernel("CSMain");
    }

    private void OnDisable()
    {
        d_positionsBuffer.Release();
        d_positionsBuffer = null;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGPU();
    }

    private void UpdateGPU()
    {
        computeShader.SetBuffer(k_kernelIndex, IDpositionsBuffer, d_positionsBuffer);
        computeShader.SetInt(IDnumElements, numElements);
        computeShader.SetFloat(IDtime, Time.time);
        computeShader.SetFloat(IDspeed, speed);

        int groups = numElements > 64 ? (int)Math.Ceiling(numElements / 64f) : 1;
        computeShader.Dispatch(k_kernelIndex, groups, 1, 1);

        
        material.SetBuffer(IDpositionsBuffer, d_positionsBuffer);
        material.SetFloat(Shader.PropertyToID("_Scale"), scale);
        var bounds = new Bounds(Vector3.zero, Vector3.one * 100);
        Graphics.DrawMeshInstancedProcedural(
            mesh, 0, material, bounds, d_positionsBuffer.count);
    }
}
