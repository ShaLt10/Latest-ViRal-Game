using System;
using System.Collections;
using System.Collections.Generic;
using Game.Utility;
using UnityEngine;

[RequireComponent(typeof(PlayerInteract))]
public class PlayerController : MonoBehaviour
{
    public Analog joystick;
    public float moveSpeed = 5f;
    public bool is3D = false; // Set true if 3D
    [SerializeField]
    private Animator animator;
    [SerializeField]
    Transform graph;
    [SerializeField]
    private Rigidbody2D rb;
    private static int Movex = Animator.StringToHash("MoveX");
    private static int Movey = Animator.StringToHash("MoveY");
    private static int Speed = Animator.StringToHash("Speed");
    private static int Facex = Animator.StringToHash("FaceX");
    private static int Facey = Animator.StringToHash("FaceY");
    private PlayerInteract interact;

    [SerializeField]
    RuntimeAnimatorController Raline;

    [SerializeField]
    RuntimeAnimatorController Gavi;
    
    private void Start()
    {
        interact = GetComponent<PlayerInteract>();
        
        // FIXED: Gunakan CharacterManager singleton yang sudah ada
        if (CharacterManager.Instance != null)
        {
            if (CharacterManager.Instance.GetPlayerName() == StringContainer.Raline)
            {
                animator.runtimeAnimatorController = Raline;
            }
            else
            {
                animator.runtimeAnimatorController = Gavi;
            }
        }
        else
        {
            Debug.LogWarning("CharacterManager.Instance is null! Using default Raline animator.");
            animator.runtimeAnimatorController = Raline;
        }
    }

    void Update()
    {
        Vector2 input = joystick.Direction;
        Vector3 move = is3D ? new Vector3(input.x, 0, input.y) : new Vector3(input.x, input.y, 0);
        //rb.velocity = move * moveSpeed * Time.deltaTime;
        transform.Translate(move * moveSpeed * Time.deltaTime);
        animator.SetFloat(Movex, move.x);
        animator.SetFloat(Movey, move.y);
        animator.SetFloat(Speed, Vector3.Magnitude(move)); 

        if (move.x < 0)
        {
            interact.SetFace(Face.Left);
            ResetFace();
            animator.SetFloat(Facex, -1);
            if (move.y < move.x)
            {
                interact.SetFace(Face.Down);
                ResetFace();
                animator.SetFloat(Facey, -1);

            }
            else if (move.y > Mathf.Abs(move.x))
            {
                interact.SetFace(Face.Up);
                ResetFace();
                animator.SetFloat(Facey, 1);
            }
        }
        else if (move.x > 0)
        {
            ResetFace();
            animator.SetFloat(Facex, 1);
            interact.SetFace(Face.Right);
            if (move.y < move.x*-1)
            {
                interact.SetFace(Face.Down);
                ResetFace();
                animator.SetFloat(Facey, -1);

            }
            else if (move.y > move.x)
            {
                interact.SetFace(Face.Up);
                ResetFace();
                animator.SetFloat(Facey, 1);
            }
        }

        if (move.x < 0)
        {
            graph.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            graph.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    private void ResetFace()
    {
        animator.SetFloat(Facex, 0);
        animator.SetFloat(Facey, 0);
    }
}