using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CharacterController controller;
    [SerializeField] GameObject artGroup;
    [SerializeField] LayerMask terrainMask;
    [SerializeField] LayerMask tongueMask;
    [SerializeField] Transform groundCheck;
    [SerializeField] Transform wallCheck;

    [Header("Movement Stats")]
    [SerializeField] float moveSpeed = 2.0f;
    [SerializeField] float airMoveSpeed = 0.05f;
    [SerializeField] float jumpHeight = 4.0f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float grappleAccel = 0.4f;
    [SerializeField] float slopeLimit = 50;
    [SerializeField] float slideFriction = 0.3f;

    [Header("Tongue")]
    [SerializeField] GameObject tongueTip;
    [SerializeField] GameObject tongueBase;
    [SerializeField] float tongueExtendDuration = 1.0f;
    [SerializeField] float tongueDistanceMax = 2.0f;
    private Vector3 tongueEndPosition; 
    private float elapsedExtendTime;

    [Header("Collision Variables")]
    [SerializeField] float groundDistance = 0.1f;
    [SerializeField] float wallDistance = 0.1f;

    Vector3 velocity;
    Vector3 tongueStop;
    bool isGrounded;
    bool isGroundSlope;
    bool isWallTouch;
    bool isTongueTouch;
    private int facing;
    Vector3 hitNormal;
    
    enum playerState {normal, extend, grapple, tumble, splat}
    private playerState currentState;
    private bool isToungeCollide;


    private void Start()
    {
        currentState = playerState.normal;
        facing = 1;
        isToungeCollide = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Reset scene
        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        //Turn art for facing value
        if (facing == 1)
        {
            artGroup.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (facing == -1)
        {
            artGroup.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        //Player State Machine
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
                    
                }
                else if (x < 0)
                {
                    facing = -1;
                 
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
                    isToungeCollide = false;
                    break;
                }

                break;

            //Tongue Extention State
            case playerState.extend:
                isToungeCollide = Physics.CheckSphere(tongueTip.transform.position, tongueTip.transform.localScale.x,terrainMask);

                if (!isToungeCollide)
                {
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
                        currentState = playerState.tumble;

                        //Reset Tongue
                        tongueTip.transform.position = transform.position;
                        tongueBase.transform.position = transform.position;
                        tongueBase.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        tongueBase.transform.rotation = Quaternion.Euler(0, 0, 0);
                    }
                }
                else
                {
                    tongueStop = tongueTip.transform.position;
                    currentState = playerState.grapple;
                }
                break;


            //Grapple State
            case playerState.grapple:
                isWallTouch = Physics.CheckSphere(wallCheck.position, wallDistance, terrainMask);
                isTongueTouch = Physics.CheckSphere(wallCheck.position, wallDistance, tongueMask);
                
                //When we are pulled to our tongue
                if (transform.position == tongueStop || isTongueTouch)
                {
                    //Change State
                    currentState = playerState.tumble;

                    //Reset Tongue
                    tongueTip.transform.position = transform.position;
                    tongueBase.transform.position = transform.position;
                    tongueBase.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    tongueBase.transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                //As we are pulling
                else
                {
                    velocity += new Vector3(grappleAccel * facing, grappleAccel, 0);
                    tongueTip.transform.position = tongueStop;

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

                }
                break;

            case playerState.tumble:

                isWallTouch = Physics.CheckSphere(wallCheck.position, wallDistance, terrainMask);
                //isGrounded = (Vector3.Angle(Vector3.up, hitNormal) <= slopeLimit);
                //bool isGroundSlope = true;
                
                /*foreach(var collider in Physics.OverlapSphere(groundCheck.position, groundDistance, terrainMask))
                {
                    if (collider.transform.rotation.eulerAngles != Vector3.zero)
                    {
                        isGroundSlope = true;
                    }
                    else
                    {
                        isGroundSlope = false;
                    }
                }
                */
                slopeLimit = 5;

                if (isWallTouch)
                {
                    velocity.x *= -0.6f;
                    facing = facing * -1;
                    wallDistance = 0.1f;
                }
                else
                {
                    wallDistance = 0.25f;
                }

                if (isGrounded && !isGroundSlope)
                {
                    slopeLimit = 50;
                    currentState = playerState.normal;
                }
                else
                {
                    velocity.y += gravity * Time.deltaTime;
                }
                break;

            case playerState.splat:

                break;

            default:

                break;

        }
    }
        
    private void FixedUpdate()
    {
        if (isGroundSlope && isGrounded)
        {
            velocity.x += (1f - hitNormal.y) * hitNormal.x * (1f - slideFriction);
            //inpRes.speed.z += (1f - hitNormal.y) * hitNormal.z * (1f - slideFriction);
        }
        //Move
        controller.Move(velocity * Time.deltaTime);
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, terrainMask);
        isGroundSlope = Vector3.Angle(Vector3.up, hitNormal) > slopeLimit;
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }

    public void setTongueColide(bool setCollide)
    {
        isToungeCollide = setCollide;
    }
}
