using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

//https://youtu.be/K1xZ-rycYY8 - Player Movement
public class Player_Movement2 : MonoBehaviour
{
    //HAHA this looks so stupid but necessary 
    public bool moveable = true;
    private RigidbodyConstraints2D storedConstraints;
	private float horizontal;
    public float Speed = 8f;
    public float jumpingPower = 6f;
    public bool isFacingRight = true;
    public float gravity = 3f;
    public bool isFalling = false;
    public bool AllowedToDash = false;
	//wallsliding mgmt
    private bool isWallsliding = false;
    public float wallslidingSpeed = 2f;
	//walljumping mgmt
	private bool isWallJumping = false;
    public float walljumpCD = 0.5f;
	private float wallJumpDriection = 0.2f;
	public float counterWallJump = 4;
	public float wallJumpDuration = 0.4f;
	public Vector2 wallJumpingPower = new Vector2 (8f, 16f);
    //Dashing
    public float DashPower = 10;
    public float DashDuration = 0.5f;
    private bool isDashing = false;
    public int DashCounter = 1;
    private int maxDashes;



    [SerializeField] private Rigidbody2D rb; //Player Rigidbody
    [SerializeField] private Transform groundCheck; //PlayersSubObject at feet's Current location
    [SerializeField] private LayerMask groundLayer; //Detecting a layer of the object they are currently on
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    private Animator mAnimator;
    private SpriteRenderer mSpriteRend;

    private void Start()
    {
        storedConstraints = rb.constraints;//saving the constraints
        maxDashes = DashCounter;    //setting the maximum amount of dashes per jump for the player
		mAnimator = GetComponent<Animator>(); //used for the animations
        mSpriteRend = GetComponent<SpriteRenderer>(); //only touch this in verry spesific cases. !!!
    }
    // Update is called once per frame
    void Update()
    {
        if (!moveable)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        }
        // Debug.Log(isWallsliding);   fuck it idk why you can wall jump tech its not saying your sliding while in mid air so idk  guess its a feature
        //gets the raw input of the horizontal input axis (a -1 , d 1)       
        horizontal = Input.GetAxisRaw("Horizontal");
        //Debug.Log(rb.velocity.y);
        if (!IsGrounded() && !isWallsliding && !isDashing && rb.velocity.y < 0 && !isFalling)
        {
            isFalling = true;
			mAnimator.SetBool("isFalling", true);
		}
        else
        {
            if (isFalling && IsGrounded() || isFalling && IsWalled())
            {
                isFalling = false;
                mAnimator.SetBool("isFalling", false);
            }
        }
		WallSlide(); //checks if wallsliding
		WallJump(); //allows for walljumping

		//maintaining Dashes
		if (IsGrounded() )
        {
            DashCounter = maxDashes;
        }
        bool Moving = mAnimator.GetCurrentAnimatorStateInfo(0).IsName("Moving");
        if (horizontal == 0 && !isDashing)
        {
            mAnimator.SetBool("Moving", false);
        }
        else if (horizontal != 0 && !isDashing)
        {
            mAnimator.SetBool("Moving", true);
        }
        if (Input.GetButtonDown("Fire1") && !isDashing && DashCounter > 0)
		{
            DashCounter--;
            DashPrime();
        }
        if (!isWallsliding)
        {
			Flip();
		}
		//handling jumps
		//How the player jumps 
		if (Input.GetButtonDown("Jump") && IsGrounded() && !isDashing && !isWallsliding && !isFalling)
		{
			//takes the rigidbodys velocity and changes it based on the current velocity and the paramater jumping power 
			rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
		}
		//to create a smaller jump if the button is released 
		if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f && !isDashing && !isWallsliding)
		{
			//as this is meant to decrease the jumping power its timed by a half
			rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
		}
    }
    //Runs every time something changes not on every frame
    void FixedUpdate()
    {
		if(!isWallJumping && !isDashing){
			rb.velocity = new Vector2(horizontal * Speed, rb.velocity.y);
		}
    }
    //revives the player, in otherword makes the able to move.
    public void revie()
    {
        rb.constraints = storedConstraints; 
    }
    //Used to flip the players sprite when moving left or right
    private void Flip()
    {
        if (!isWallsliding && !isDashing)
        {
            //checks if they have changed their movement and need to be flipped
            if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
            {
                isFacingRight = !isFacingRight;
                //physically flips the sprite
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
        }
    }
    //dangerious 

	private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void WallSlide()
    {
        if(IsWalled() && !IsGrounded() && horizontal !=0f)
        {
            //kicking the player out of a dash
            if(isDashing)
            {
                DashExit();
            }
            //wallJumpDriection = -transform.localScale.x;
            //wallsiding
            //locking the walljump direction.
            if (!isWallsliding)
            {
				wallJumpDriection = -transform.localScale.x;
			}
			mAnimator.SetBool("walled", true);
			isWallsliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallslidingSpeed, float.MaxValue));
        }
        else
        {
			mAnimator.SetBool("walled", false);
            Invoke(nameof(wallslideStop), 0.1f);
        }
    }
	private void wallslideStop()
	{
		isWallsliding = false;
	}

	private void WallJump(){
        //checking if it is possible, there are a couple of redundancies in here
        if (isWallsliding && !isWallJumping && !IsGrounded() && IsWalled()){
			isWallJumping = false;
			counterWallJump = wallJumpDuration;
			CancelInvoke(nameof(StopWallJumping));
		}
		else {
			counterWallJump -= Time.deltaTime;
		}
        //acutally jumping
        if (Input.GetButtonDown("Jump") && counterWallJump > 0f && isWallsliding) {
            Debug.Log("jumpDir " + wallJumpDriection + "  " );
			isWallJumping = true;
			rb.velocity = new Vector2( wallJumpDriection * wallJumpingPower.x, wallJumpingPower.y);
			counterWallJump = 0f;
            Flip();
		}

		Invoke(nameof(StopWallJumping), wallJumpDuration); // calls the stop walljumping after a delay (walljumpduration)
	}
	private void StopWallJumping(){
		isWallJumping = false;
        //wallJumpDriection = -transform.localScale.x;
	}
	private void DashPrime()
	{
        if (!isDashing && AllowedToDash)
        {
            var dir = transform.localScale.x;
			isDashing = true;
            mAnimator.SetBool("isDashing", true);
            rb.gravityScale = 0f;
            Debug.Log("Starting a dash");
            if (IsWalled())
            {
                dir = -dir;
            }
            rb.velocity = new Vector2(dir * DashPower, 0); // the dash itself
            Invoke(nameof(DashExit), DashDuration + 0.2f);
        }
    }
	private void DashExit()
	{
		rb.gravityScale = gravity;
        Debug.Log("is Dashing Exit");
        //mAnimator.ResetTrigger("isDashing");
		mAnimator.SetBool("isDashing", false);
        rb.velocity = new Vector2(0, 0);
        isDashing = false;
	}

	private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
}
