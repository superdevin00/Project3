using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] CharacterController controller;

    [SerializeField] float moveSpeed = 4.0f;
    [SerializeField] float jumpHeight = 4.0f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float tongueDistanceMax = 2.0f;

    [SerializeField] LayerMask terrainMask;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance = 0.4f;

    Vector3 velocity;
    bool isGrounded;
    

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, terrainMask);
        float x = Input.GetAxis("Horizontal");
        //Vector3 move = transform.right * x;

        if (isGrounded && velocity.y <0)
        {
            velocity.y = -2f;
            velocity.x = x * moveSpeed;
        }

        
        
        //controller.Move(move * moveSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
    private void FixedUpdate()
    {

    }
}
