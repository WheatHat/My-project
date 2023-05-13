using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TwoDPlayerController : MonoBehaviour
{
    enum WallHit
    {
        None,
        Right,
        Left
    }

    float Speed = 6;
    float CurrentDePosition;
    readonly private float JumpForce = 500f;

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
    float wallJumpDuration = 0.3f;

    bool canDash = true;
    bool isDashing = false;
    Vector2 dashDirection = Vector2.zero;
    float dashStopWatch = 0;
    float dashDuration = 0.06f;
    float dashCoolDown = 2;

    Image DashUI;
    Text DashText;
    Transform canvas;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GameObject.Find("Canvas").transform;
        DashUI = canvas.Find("DashUI").Find("Image").GetComponent<Image>();
        DashText = canvas.Find("DashUI").Find("Image").Find("Text").GetComponent<Text>();

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
            else
            {
                dashDirection = new Vector2(0, 0);
            }

            if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                dashDirection = new Vector2(dashDirection.x, 1);
            }
            else
            {
                dashDirection = new Vector2(dashDirection.x, 0);
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
                rb.gravityScale = 3;
            }
        }
        else//is wallJumping
        {
            transform.position += new Vector3(CurrentDePosition * wallJumpDirection * 1.1f, CurrentDePosition * 0.9f, 0);
            wallJumpStopWatch += Time.deltaTime;
            if(wallJumpStopWatch >= wallJumpDuration)
            {
                rb.gravityScale = 3;
                wallJumpStopWatch = 0;
                CanMove = true;
            }
        }

        if (!canDash)
        {
            dashStopWatch += Time.deltaTime;

            float remainingTime = (dashCoolDown - dashStopWatch);
            float percentage = remainingTime / dashCoolDown;

            if(remainingTime < 1)
            {
                DashText.text = (remainingTime * 100).ToString("f0") + "<size=30>ms</size>";
            }
            else
            {
                DashText.text = remainingTime.ToString("f1");
            }

            if (dashStopWatch > dashCoolDown)
            {
                percentage = 0;
                DashText.text = "";

                dashStopWatch = 0;
                canDash = true;
            }

            DashUI.fillAmount = percentage;
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

    private void Flip()
    {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * -1, transform.localScale.z);
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