using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] CharacterController controller;

    [SerializeField] float moveSpeed = 2.0f;
    [SerializeField] float airMoveSpeed = 0.05f;
    [SerializeField] float jumpHeight = 4.0f;
    [SerializeField] float gravity = -9.81f;

    [SerializeField] GameObject tongueTip;
    [SerializeField] float tongueExtendDuration = 1.0f;
    [SerializeField] float tongueDistanceMax = 2.0f;
    private Vector3 tongueEndPosition; 
    private float elapsedExtendTime;

    [SerializeField] LayerMask terrainMask;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance = 0.4f;
    [SerializeField] GameObject artGroup;

    Vector3 velocity;
    bool isGrounded;
    private int facing;
    
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
                //Air movement
                else if (!isGrounded)
                {
                    velocity.x += x * airMoveSpeed;
                    velocity.x = Mathf.Clamp(velocity.x, -moveSpeed, moveSpeed);
                }

                //Adjust Facing
                if (x > 0)
                {
                    facing = 1;
                    artGroup.transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                else if (x < 0)
                {
                    facing = -1;
                    artGroup.transform.rotation = Quaternion.Euler(0, 180, 0);
                }

                //Jump
                if ((Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("Jump")) && isGrounded)
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
                }

                //Apply Gravity
                velocity.y += gravity * Time.deltaTime;

                if (Input.GetKeyDown(KeyCode.X))
                {
                    elapsedExtendTime = 0;
                    currentState = playerState.extend;
                    break;
                }

                break;

            case playerState.extend:

                velocity = Vector3.zero;
                
                elapsedExtendTime += Time.deltaTime;
                float percentComplete = elapsedExtendTime / tongueExtendDuration;

                Vector3 startPosition = new Vector3(transform.position.x, transform.position.y, 0);
                tongueEndPosition = new Vector3(transform.position.x + tongueDistanceMax * facing, transform.position.y + tongueDistanceMax, 0);

                tongueTip.transform.position = Vector3.Lerp(startPosition, tongueEndPosition, percentComplete);

                if (tongueTip.transform.position == tongueEndPosition)
                {
                    currentState = playerState.normal;
                    tongueTip.transform.position = transform.position;
                }

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
