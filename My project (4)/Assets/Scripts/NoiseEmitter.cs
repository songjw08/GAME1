using UnityEngine;
using System;

public class NoiseEmitter : MonoBehaviour
{
    public static event Action<Vector3, float> OnNoise; // position, radius

    public void EmitNoise(float radius)
    {
        OnNoise?.Invoke(transform.position, radius);
    }
}
