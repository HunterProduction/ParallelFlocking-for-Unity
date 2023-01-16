using System;
using System.Collections;
using System.Collections.Generic;
using BufferSorter;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestSorter : MonoBehaviour
{
    private const int DIM = 5;
    public ComputeShader bitonicMergeSorter;
    
    /*
     * NOTES: This sorter provided by EmmetOT (repo: https://github.com/EmmetOT/BufferSorter)
     * is intended to work with int and uint, because its kernels use
     * InterlockedMax and InterlockedMin which works only with int and uint values.
     * Since sizeof(int) is the same of sizeof(float), tests shows that the sort works also for floats, but ONLY
     * FOR POSITIVE VALUES (NO NEGATIVE FLOATS)
     */
    private float[] _testArray = new float[DIM];
    private Sorter _sorter;
    private ComputeBuffer _buffer;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Starting Array:");
        for (int i = 0; i < DIM; i++)
        {
            _testArray[i] = Random.Range(0f, 1f);
            Debug.Log(_testArray[i]);
        }

        _buffer = new ComputeBuffer(DIM, sizeof(float));
        _buffer.SetData(_testArray);
        
        _sorter = new Sorter(bitonicMergeSorter);
        _sorter.Sort(_buffer);
        _buffer.GetData(_testArray);
        
        Debug.Log("Sorted Array:");
        for (int i = 0; i < DIM; i++)
        {
            Debug.Log(_testArray[i]);
        }
    }

    private void OnDisable()
    {
        _buffer.Dispose();
        _sorter.Dispose();
    }
}
