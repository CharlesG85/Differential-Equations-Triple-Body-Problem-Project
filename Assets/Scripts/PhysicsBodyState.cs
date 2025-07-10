using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBodyState
{
    public float mass;
    public Color color;

    private Vector2 targetPosition;
    private Vector2 targetVelocity;
    private Vector2 velocity;
    private Vector2 position;

    public void CloneState(PhysicsBodyState state)
    {
        position = state.position;
        velocity = state.velocity;
        mass = state.mass;
    }

    public void CloneState(PhysicsBody state)
    {
        // Copy any properties of the PhysicsBody
        mass = state.mass;
        velocity = state.initialVelocity;
        position = state.transform.position;
        color = state.color;
    }

    public void UpdatePositionAndVelocity()
    {
        position = targetPosition;
        velocity = targetVelocity;
    }

    public Vector2 GetPosition()
    {
        return position;
    }

    public Vector2 GetVelocity()
    {
        return velocity;
    }

    public void SetTargetPosition(Vector2 position)
    {
        targetPosition = position;
    }

    public void SetTargetVelocity(Vector2 velocity)
    {
        targetVelocity = velocity;
    }

    public void SetVelocity(Vector2 velocity)
    {
        this.velocity = velocity;
    }

    public void SetPosition(Vector2 position)
    {
        this.position = position;
    }
}
