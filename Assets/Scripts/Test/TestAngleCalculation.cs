using UnityEngine;

public class TestAngleCalculation : MonoBehaviour
{
    public Transform other;

    private Transform _transform;
    // Start is called before the first frame update
    void Start()
    {
           
    }

    // Update is called once per frame
    void Update()
    {
        _transform = transform;
        var distance = other.position - _transform.position;
        var right = _transform.right;
        var cosAngleBetween = Vector3.Dot(right, distance) / (right.magnitude * distance.magnitude);
        Debug.Log($"Cos(angle) = {cosAngleBetween}, angle = {Mathf.Acos(cosAngleBetween)*Mathf.Rad2Deg}");
    }
}
