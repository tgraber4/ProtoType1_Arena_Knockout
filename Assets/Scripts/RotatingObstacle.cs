using UnityEngine;

public class Rotator : MonoBehaviour
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    [Header("Rotation Settings")]
    [Tooltip("Choose the axis of rotation.")]
    public RotationAxis rotationAxis = RotationAxis.Y;

    [Tooltip("Speed of rotation in degrees per second.")]
    public float rotationSpeed = 50f;

    [Tooltip("Check for clockwise rotation, uncheck for counter-clockwise.")]
    public bool clockwise = true;

    void Update()
    {
        // Determine direction multiplier (+1 or -1)
        float direction = clockwise ? 1f : -1f;

        // Determine the axis vector based on user selection
        Vector3 axis = Vector3.zero;
        switch (rotationAxis)
        {
            case RotationAxis.X:
                axis = Vector3.right;
                break;
            case RotationAxis.Y:
                axis = Vector3.up;
                break;
            case RotationAxis.Z:
                axis = Vector3.forward;
                break;
        }

        // Apply rotation smoothly across all frames using Time.deltaTime
        transform.Rotate(axis * rotationSpeed * direction * Time.deltaTime);
    }
}