using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField]
    private LayerMask layer;

    [SerializeField]
    private Face face;

    [SerializeField]
    private Button Button;

    private void Awake()
    {
        Button.onClick.AddListener(CheckInteract);
    }

    private void OnDisable()
    {
        Button.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
           CheckInteract();
        }
    }


    private void CheckInteract()
    {
        RaycastHit2D ray = new();

        switch (face)
        {
            case Face.Left:
                ray = Physics2D.BoxCast(transform.position + Vector3.left, new Vector2(2, 2), 0, Vector2.left, 2, layer);
                break;

            case Face.Right:
                ray = Physics2D.BoxCast(transform.position + Vector3.right, new Vector2(2, 2), 0, Vector2.right, 2, layer);
                // ray = Physics2D.Raycast(transform.position, Vector2.right, 2, layer);
                break;

            case Face.Up:
                ray = Physics2D.BoxCast(transform.position + Vector3.up, new Vector2(2, 2), 0, Vector2.up, 2, layer);
                //ray = Physics2D.Raycast(transform.position, Vector2.up, 2, layer);
                break;

            case Face.Down:
                ray = Physics2D.BoxCast(transform.position + Vector3.down, new Vector2(2, 2), 0, Vector2.down, 2, layer);
                //ray = Physics2D.Raycast(transform.position, Vector2.down, 2, layer);
                break;
        }

        if (ray.collider != null)
        {

            if (ray.collider.TryGetComponent(out IInteract interact))
            {
                interact.Interaction();
            }
        }
    }

    public void SetFace(Face face)
    {
        this.face = face;
    }

    void OnDrawGizmos()
    {
        switch (face)
        {
            case Face.Left:
                Gizmos.DrawCube(transform.position + Vector3.left, new Vector2(2, 2));
                break;

            case Face.Right:
                Gizmos.DrawCube(transform.position + Vector3.right, new Vector2(2, 2));
                // ray = Physics2D.Raycast(transform.position, Vector2.right, 2, layer);
                break;

            case Face.Up:
                Gizmos.DrawCube(transform.position + Vector3.up, new Vector2(2, 2));
                //ray = Physics2D.Raycast(transform.position, Vector2.up, 2, layer);
                break;

            case Face.Down:
                Gizmos.DrawCube(transform.position + Vector3.down, new Vector2(2, 2));
                //ray = Physics2D.Raycast(transform.position, Vector2.down, 2, layer);
                break;
        }
    }
}


public enum Face
{ 
Up,
Down,
Left,
Right,
}
