using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBody : MonoBehaviour
{
    public Vector2 initialVelocity;
    public float angularVelocity;
    public float mass;
    public Color color;

    [HideInInspector] public Vector2 velocity;

    private void Awake()
    {
        velocity = initialVelocity;

        angularVelocity = Random.Range(-10f, 10f);
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public Vector2 GetVelocity()
    {
        return velocity;
    }

    public void SetInitialVelocity(Vector2 velocity)
    {
        this.initialVelocity = velocity;
    }

    public void SetPosition(Vector2 position)
    {
        transform.position = position;
    }
}
