using UnityEngine;

public abstract class FlockHandler : MonoBehaviour
{
    public Flock flock;
    
    protected Vector3 center;
    protected ComputeBuffer positionsBuffer;
    
    private void OnValidate()
    {
        // NOTE: Cohesion and Alignment radius are a "fake" parameter: they're bound to the Agents view range.
        flock.cohesionParameters.radius = flock.agentViewRange;
        flock.alignmentParameters.radius = flock.agentViewRange;
        //NormalizeBehaviourWeights();
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.25f, 0.95f, 1f, 0.2f);
        center = flock.Origin.position;
        if (center == null) return;
        Gizmos.DrawSphere(center, flock.flockRadius);
        Gizmos.color = new Color(0.28f, 1f, 0.13f, 0.12f);
        Gizmos.DrawSphere(center, flock.boundingSphereParameters.radius);
    }

    protected virtual void OnEnable()
    {
        center = flock.Origin.position;
        InitializeData();
    }

    // Update is called once per frame
    protected void Update()
    {
        center = flock.Origin.position;
        UpdatePositionsAndVelocities();
        DrawAgents();
    }

    protected virtual void OnDisable()
    {
        DisposeData();
    }

    protected void DrawAgents()
    {
        flock.agentMaterial.SetBuffer(Shader.PropertyToID("_Positions"), positionsBuffer);
        flock.agentMaterial.SetFloat(Shader.PropertyToID("_Scale"), flock.agentScale);
        var bounds = new Bounds(center, Vector3.one * (flock.flockRadius * 10f));
        Graphics.DrawMeshInstancedProcedural(
            flock.agentMesh, 0, flock.agentMaterial, bounds, positionsBuffer.count);
    }

    protected abstract void InitializeData();
    protected abstract void DisposeData();
    protected abstract void UpdatePositionsAndVelocities();
}
