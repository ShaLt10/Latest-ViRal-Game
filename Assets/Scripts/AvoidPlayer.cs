using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidPlayer : MonoBehaviour
{
    public Transform player;
    public float avoidRadius = 2f;
    public float returnSpeed = 1.5f;
    public float avoidSpeed = 2f;

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < avoidRadius)
        {
            // Move away from player
            Vector2 awayFromPlayer = (Vector2)(transform.position - player.position).normalized;
            transform.position += (Vector3)(awayFromPlayer * avoidSpeed * Time.deltaTime);
        }
        else
        {
            // Return to original position
            Vector2 toOrigin = (Vector2)(originalPosition - transform.position);
            if (toOrigin.magnitude > 0.1f) // small threshold
            {
                transform.position += (Vector3)(toOrigin.normalized * returnSpeed * Time.deltaTime);
            }
        }
    }
}
