﻿using System.Collections;
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
    [SerializeField] GameObject tongueBase;
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
        facing = 1;
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

            //Tongue Extention State
            case playerState.extend:

                //Freeze player movement
                velocity = Vector3.zero;
                
                elapsedExtendTime += Time.deltaTime;
                float percentComplete = elapsedExtendTime / tongueExtendDuration;

                Vector3 startPosition = new Vector3(transform.position.x, transform.position.y, 0);
                tongueEndPosition = new Vector3(transform.position.x + tongueDistanceMax * facing, transform.position.y + tongueDistanceMax, 0);

                //Lerp tongue tip extend position
                tongueTip.transform.position = Vector3.Lerp(startPosition, tongueEndPosition, percentComplete);

                //Stretch tongue base to tip
                Vector3 startPos = transform.position;
                Vector3 endPos = tongueTip.transform.position;

                //Math tongue stretch
                Vector3 tongueCenter = new Vector3(startPos.x + endPos.x, startPos.y + endPos.y) / 2f;
                float tongueXScale = Mathf.Abs(startPos.x - endPos.x);
                float tongueYScale = Mathf.Abs(startPos.y - endPos.y);
                float tongueTrueScale = Mathf.Sqrt((tongueXScale * tongueXScale) + (tongueYScale * tongueYScale)) / 2; //Scale is divided by 2 to adjust for cylinder height

                //Apply tongue stretch
                tongueBase.transform.position = tongueCenter;
                tongueBase.transform.localScale = new Vector3(0.1f, tongueTrueScale, 0.1f);
                tongueBase.transform.rotation = Quaternion.Euler(0, 0, -45 * facing);

                //Check if fully extended
                if (tongueTip.transform.position == tongueEndPosition)
                {
                    //Change State
                    currentState = playerState.normal;

                    //Reset Tongue
                    tongueTip.transform.position = transform.position;
                    tongueBase.transform.position = transform.position;
                    tongueBase.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    tongueBase.transform.rotation = Quaternion.Euler(0, 0, 0);
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