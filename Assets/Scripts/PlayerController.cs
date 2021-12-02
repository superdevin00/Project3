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
    [SerializeField] LayerMask pullRingMask;
    [SerializeField] Transform groundCheck;
    [SerializeField] Transform wallCheck;

    [Header("Feedback References")]
    [SerializeField] ParticleSystem landPart;
    [SerializeField] ParticleSystem tongueTouchPart;
    [SerializeField] AudioClip landSound;
    [SerializeField] AudioClip impactSound;
    [SerializeField] AudioClip extendSound;
    [SerializeField] AudioClip tongueImpactSound;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] float sfxVolume = 0.1f;

    [Header("Movement Stats")]
    [SerializeField] float moveSpeed = 2.0f;
    [SerializeField] float airMoveSpeed = 0.05f;
    [SerializeField] float jumpHeight = 4.0f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float grappleAccel = 0.4f;
    [SerializeField] float slopeLimit = 50;
    [SerializeField] float slideFriction = 0.3f;
    [SerializeField] float minimumBounceVelocityX = 3.0f;
    [SerializeField] float minimumBounceVelocityY = 7.0f;
    [SerializeField] float grappleMomentumMultiplierX = 1.0f;
    [SerializeField] float grappleMomentumMultiplierY = 1.0f;

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
    [SerializeField] float tongueDistance = 0.1f;

    Vector3 velocity;
    Vector3 tongueStop;
    bool isGrounded;
    bool isGroundSlope;
    bool isWallTouch;
    bool isTongueTouch;
    bool slopeStop;
    bool isWallNormal;
    bool isCeilingNormal;
    float lastYValue;
    float tumbleRotate = 0;
    private int facing;
    Vector3 hitNormal;
    
    public enum playerState {normal, extend, grapple, tumble, splat}
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        //Turn art for facing value
        if (facing == 1)
        {
            artGroup.transform.rotation = Quaternion.Euler(0, 0, 0);
            wallCheck.localPosition = new Vector3(0.2f,0.0f,0);
        }
        else if (facing == -1)
        {
            artGroup.transform.rotation = Quaternion.Euler(0, 180, 0);
            wallCheck.localPosition = new Vector3(-0.2f, 0.0f, 0);
        }

        //Player State Machine
        switch (currentState)
        {
            # region Normal Player State
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
                    AudioSource.PlayClipAtPoint(jumpSound, transform.position, sfxVolume);
                    velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
                }

                //Apply Gravity
                velocity.y += gravity * Time.deltaTime;

                //Tongue control
                if (Input.GetKeyDown(KeyCode.X))
                {
                    elapsedExtendTime = 0;
                    currentState = playerState.extend;
                    AudioSource.PlayClipAtPoint(extendSound, transform.position, sfxVolume);
                    isToungeCollide = false;
                    break;
                }

                break;
            #endregion

            #region Tongue Extention State
            case playerState.extend:
                

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
                        if (isGrounded)
                        {
                            currentState = playerState.normal;
                        }
                        else
                        {
                            currentState = playerState.tumble;
                        }

                        //Reset Tongue
                        isWallTouch = false;
                        isTongueTouch = false;
                        tongueTip.transform.position = transform.position;
                        tongueBase.transform.position = transform.position;
                        tongueBase.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        tongueBase.transform.rotation = Quaternion.Euler(0, 0, 0);
                    }
                }
                else //When we touch the wall with our tongue
                {
                    tongueStop = tongueTip.transform.position;
                    AudioSource.PlayClipAtPoint(tongueImpactSound, transform.position, sfxVolume);
                    Instantiate(tongueTouchPart, tongueStop, Quaternion.Euler(0, 0, 0));
                    currentState = playerState.grapple;
                    isWallTouch = false;
                    isTongueTouch = false;
                }

                isToungeCollide = Physics.CheckSphere(tongueTip.transform.position, tongueTip.transform.localScale.x, terrainMask) || Physics.CheckSphere(tongueTip.transform.position, tongueTip.transform.localScale.x, pullRingMask);
                break;
            #endregion

            #region Grapple State
            case playerState.grapple:
                
                
                //When we are pulled to our tongue or we release early
                if (transform.position == tongueStop || isTongueTouch || !Input.GetKey(KeyCode.X))
                {
                    //Change State
                    currentState = playerState.tumble;
                    slopeStop = true;

                    //Reset Tongue
                    tongueTip.transform.position = transform.position;
                    tongueBase.transform.position = transform.position;
                    tongueBase.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    tongueBase.transform.rotation = Quaternion.Euler(0, 0, 0);

                    //Velocity Multiplier
                    velocity.x = velocity.x * grappleMomentumMultiplierX;
                    velocity.y = velocity.y * grappleMomentumMultiplierY;

                    //Minimum Speed
                    if (Mathf.Abs(velocity.x) < minimumBounceVelocityX)
                    {
                        velocity.x = minimumBounceVelocityX * facing;
                    }
                    if (velocity.y < minimumBounceVelocityY)
                    {
                        velocity.y = minimumBounceVelocityY;
                    }
                    isGrounded = false;
                    break;
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

                //isWallTouch = Physics.CheckSphere(wallCheck.position, wallDistance, terrainMask);
                isTongueTouch = Physics.CheckSphere(wallCheck.position, tongueDistance, tongueMask);

                break;
            #endregion

            #region Tumble State
            case playerState.tumble:

                

                tumbleRotate += 5.0f ;

                artGroup.transform.Rotate(0, 0, -tumbleRotate);

                slopeLimit = 5;

                //Check for bounces
                if (isWallTouch && isWallNormal && !isGroundSlope && !isGrounded)//Wall bounce
                {
                    velocity.x *= -0.6f;
                    facing = facing * -1;
                    wallDistance = 0.0f;
                    AudioSource.PlayClipAtPoint(impactSound, transform.position, sfxVolume);
                }
                else if (isWallTouch && isCeilingNormal && velocity.y > 0)//Ceiling bounce
                {
                    velocity.y *= -0.5f;
                    AudioSource.PlayClipAtPoint(impactSound, transform.position, sfxVolume);
                }
                else
                {
                    wallDistance = 0.15f;
                }
                
                //Check if grounded
                if (isGrounded && !isGroundSlope && velocity.y < 0 && isWallTouch) //Land on solid ground
                {
                    AudioSource.PlayClipAtPoint(landSound, transform.position, sfxVolume);
                    Instantiate(landPart, groundCheck.transform.position, Quaternion.Euler(-90, 0, 0));
                    slopeLimit = 50;
                    currentState = playerState.normal;
                    tumbleRotate = 0;
                    artGroup.transform.rotation = Quaternion.Euler(0.0f, 0.0f, tumbleRotate);
                }
                else if(isGrounded && isGroundSlope) //Land on slope
                {
                    if (slopeStop)
                    {
                        velocity.x = 0;
                        lastYValue = transform.position.y;
                        slopeStop = false;
                    }
                    else
                    {
                        lastYValue = transform.position.y;
                    }
                    
                }
                else
                {
                    velocity.y += gravity * Time.deltaTime;
                }
                isWallTouch = false;
                //isWallTouch = Physics.CheckSphere(wallCheck.position, wallDistance, terrainMask);

                break;
            #endregion

            case playerState.splat:

                break;

            default:

                break;

        }
    }
        
    private void FixedUpdate()
    {
        if (isGroundSlope && isGrounded) //Slide Down slope
        {
            velocity.x += (1f - hitNormal.y) * hitNormal.x * (1f - slideFriction);
        }
        //Move
        controller.Move(velocity * Time.deltaTime);
      
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, terrainMask);
        isGroundSlope = Vector3.Angle(Vector3.up, hitNormal) > slopeLimit && Vector3.Angle(Vector3.up, hitNormal) < 90;

        if (Vector3.Angle(Vector3.up, hitNormal) >= 80 && Vector3.Angle(Vector3.up, hitNormal) <= 100) //Hit wall
        {
            isWallNormal = true;
            isCeilingNormal = false;
        }
        else if (Vector3.Angle(Vector3.up, hitNormal) >= 170 && Vector3.Angle(Vector3.up, hitNormal) <= 190) //Hit Ceiling
        {
            isCeilingNormal = true;
            isWallNormal = false;
        }



        Debug.Log(Vector3.Angle(Vector3.up, hitNormal));
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        isWallTouch = true;
        hitNormal = hit.normal;

    }

    public void setTongueColide(bool setCollide)
    {
        isToungeCollide = setCollide;
    }

    public void setCurrentState(playerState state)
    {
        currentState = state;
    }
}
