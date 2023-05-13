using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoDPlayerController : MonoBehaviour
{
    enum WallHit
    {
        None,
        Right,
        Left
    }

    float Speed = 5;
    float CurrentDePosition;
    readonly private float JumpForce = 300f;

    Rigidbody2D rb;
    BoxCollider2D BoxColl;
    float distanceToTheGround = 0;
    float distanceToSides = 0;
    float SizeOfObjVert = 0;
    float SizeOfObjHori = 0;

    bool HasDoubleJumped = false;
    bool HasJumped = false;
    bool Right, Left, Up;

    bool onWall = false;

    int wallJumpDirection = 1;
    bool CanMove = true;
    float wallJumpStopWatch = 0;
    float wallJumpDuration = 0.45f;

    bool canDash = true;
    bool isDashing = false;
    Vector2 dashDirection = Vector2.zero;
    float dashStopWatch = 0;
    float dashDuration = 0.06f;
    float dashCoolDown = 2;

    // Start is called before the first frame update
    void Start()
    {
        ResetJump();

        rb = GetComponent<Rigidbody2D>();
        BoxColl = GetComponent<BoxCollider2D>();

        SizeOfObjVert = BoxColl.bounds.extents.y;
        SizeOfObjHori = BoxColl.bounds.extents.x;

        distanceToTheGround = SizeOfObjVert + 0.02f;
        distanceToSides = SizeOfObjHori;
    }

    // Update is called once per frame
    void Update()
    {

        CurrentDePosition = Speed * Time.deltaTime;

        #region Taking inputs for basic movement

        Right = Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.RightArrow);

        Left = Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.LeftArrow);

        Up = Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow);

        if(Input.GetKeyDown(KeyCode.X) && canDash)
        {
            isDashing = true;
            CanMove = false;
            canDash = false;

            if(Left)
            {
                dashDirection = new Vector2(-1, 0);
            }
            else if(Right)
            {
                dashDirection = new Vector2(1, 0);
            }
            if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                dashDirection += new Vector2(0, 1);
            }
        }
        #endregion

        #region Basic movement

        if(CanMove)
        {
            if (Right)
            {
                if (!Left)
                {
                    //Debug.Log("right");
                    if (!onWall)
                        transform.position += new Vector3(CurrentDePosition, 0, 0);
                }
                else
                {
                    //Debug.Log("Nothing");
                }
            }
            else if (Left)
            {
                //Debug.Log("left");
                if (!onWall)
                    transform.position += new Vector3(-CurrentDePosition, 0, 0);
            }
            else
            {
                //Debug.Log("Nothing");
            }

            if (Up)
            {
                if (onWall)
                {
                    //wall jump
                    if (OnTheWall() == WallHit.Right)
                    {
                        wallJumpDirection = -1;
                        WallJump();
                    }
                    else
                    {
                        wallJumpDirection = 1;
                        WallJump();
                    }
                }
                else
                {
                    if (!HasJumped)
                    {
                        Jump();
                    }
                    else
                    {
                        if (!HasDoubleJumped)
                        {
                            Jump();
                        }
                        else
                        {
                            Debug.Log("Can't jump!");
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Not jumping.");
            }
        }
        else if(isDashing)
        {
            transform.position += ((Vector3)dashDirection) * Time.deltaTime * 100;

            dashStopWatch += Time.deltaTime;
            if(dashStopWatch >= dashDuration)
            {
                CanMove = true;
                isDashing = false;
                dashStopWatch = 0;
            }
        }
        else//is wallJumping
        {
            transform.position += new Vector3(CurrentDePosition * wallJumpDirection, CurrentDePosition, 0);
            wallJumpStopWatch += Time.deltaTime;
            if(wallJumpStopWatch >= wallJumpDuration)
            {
                rb.gravityScale = 1;
                wallJumpStopWatch = 0;
                CanMove = true;
            }
        }

        if (!canDash)
        {
            dashStopWatch += Time.deltaTime;
            if (dashStopWatch > dashCoolDown)
            {
                dashStopWatch = 0;
                canDash = true;
            }
        }
        #endregion

        if (Left || Right)//wall slide detection
        {
            WallHit wallhit = OnTheWall();

            if (!OnTheGround() && (wallhit == WallHit.Right || wallhit == WallHit.Left))
            {
                onWall = true;

                if (rb.velocity.y < -1)
                {
                    rb.velocity = new Vector2(rb.velocity.x, -1);
                }
            }
            else
            {
                onWall = false;
            }
        }
        else
        {
            onWall = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (OnTheGround())
        {
            ResetJump();
        }
    }

    void Jump()
    {
        rb.velocity = Vector3.zero;
        rb.AddForce(new Vector2(0, JumpForce));

        if (HasJumped)
        {
            HasDoubleJumped = true;
        }
        HasJumped = true;
    }

    void WallJump()
    {
        rb.gravityScale = 0;

        rb.velocity = Vector3.zero;
        CanMove = false;

        HasDoubleJumped = true;
        HasJumped = true;
    }

    void ResetJump()
    {
        HasJumped = false;
        HasDoubleJumped = false;
    }

    WallHit OnTheWall()
    {
        int LayerMask = 128; //0000000100
        WallHit hit;

        if(Physics2D.BoxCast(transform.position, new Vector2(SizeOfObjHori, 0.02f), 0, Vector2.right, distanceToTheGround, LayerMask))
        {
            hit = WallHit.Right;
        } 
        else if(Physics2D.BoxCast(transform.position, new Vector2(SizeOfObjHori, 0.02f), 0, Vector2.left, distanceToTheGround, LayerMask))
        {
            hit = WallHit.Left;
        }
        else
        {
            hit = WallHit.None;
        }

        return hit;
    }

    bool OnTheGround()
    {
        int LayerMask = 64; //0000001000

        bool hit = Physics2D.BoxCast(transform.position, new Vector2(0.02f , SizeOfObjVert), 0, Vector2.down, distanceToSides, LayerMask);

        return hit;
    }
}