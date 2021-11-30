using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] CharacterController controller;

    [SerializeField] float moveSpeed = 4.0f;
    [SerializeField] float jumpHeight = 4.0f;
    [SerializeField] float gravity = -9.81f;

    [SerializeField] GameObject tongueTip;
    [SerializeField] float tongueExtendDuration = 1.0f;
    [SerializeField] float tongueDistanceMax = 2.0f;
    private float elapsedExtendTime;

    [SerializeField] LayerMask terrainMask;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance = 0.4f;

    Vector3 velocity;
    bool isGrounded;
    
    enum playerState {normal, extend, grapple, tumble, splat}
    private playerState currentState;


    private void Start()
    {
        currentState = playerState.normal;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            //Normal Player State
            case playerState.normal:

                isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, terrainMask);
                float x = Input.GetAxis("Horizontal");

                //Grounded Movement
                if (isGrounded && velocity.y < 0)
                {
                    velocity.y = -2f;
                    velocity.x = x * moveSpeed;
                }

                //Jump
                if ((Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Jump")) && isGrounded)
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
                }

                //Apply Gravity
                velocity.y += gravity * Time.deltaTime;

                if (Input.GetKeyDown(KeyCode.X))
                {
                    elapsedExtendTime = 0;
                    currentState = playerState.extend;
                }

                break;

            case playerState.extend:

                break;

            case playerState.grapple:

                break;

            case playerState.tumble:

                break;

            case playerState.splat:

                break;

            default:

                break;

        }
    }
        
    private void FixedUpdate()
    {
        //Move
        controller.Move(velocity * Time.deltaTime);
    }
}
