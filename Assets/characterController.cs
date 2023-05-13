using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class characterController : MonoBehaviour
{
    // Start is called before the first frame update
    enum WallHit
    {
        None,
        Right,
        Left
    }

    enum Skills
    {
        Jump,
        Dash,
        Move
    }

    Skills CurrentSkill = Skills.Move;

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

    bool Right, Left, Up;//Controlls

    bool onWall = false;

    int wallJumpDirection = 1;//1 right, -1 left
    float wallJumpStopWatch = 0;
    float wallJumpDuration = 0.3f;
    bool isWallJumping = false;

    bool CanMove = true;

    bool canDash = true;
    bool isDashing = false;

    Vector2 dashDirection = Vector2.zero;
    float dashStopWatch = 0;
    float dashDuration = 0.18f;
    float dashCoolDown = 2;

    Image DashUI;
    Text DashText;
    Transform canvas;

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

        DashText.text = "";
        DashUI.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {

        #region Wall detection
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
        #endregion

        CurrentDePosition = Speed * Time.deltaTime;

        Right = Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.RightArrow);

        Left = Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.LeftArrow);

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (CurrentSkill == Skills.Dash)
            {
                if (canDash)
                {
                    rb.gravityScale = 0;
                    isDashing = true;
                    CanMove = false;
                    canDash = false;

                    if (Left)
                    {
                        dashDirection = new Vector2(-1, 0);
                    }
                    else if (Right)
                    {
                        dashDirection = new Vector2(1, 0);
                    }
                    else
                    {
                        dashDirection = new Vector2(0, 0);
                    }

                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                    {
                        dashDirection = new Vector2(dashDirection.x, 1);
                    }
                    else
                    {
                        dashDirection = new Vector2(dashDirection.x, 0);
                    }
                }
            }
            else if (CurrentSkill == Skills.Jump)
            {
                if (onWall)
                {
                    //wall jump
                    if (OnTheWall() == WallHit.Right)
                    {
                        wallJumpDirection = -1;
                        isWallJumping = true;
                        WallJump();
                    }
                    else
                    {
                        wallJumpDirection = 1;
                        isWallJumping = true;
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
                    }
                }
            }
            //else//moving
            {
                //just activate moving
            }
        }

        if (CanMove && CurrentSkill == Skills.Move)
        {
            if (Right)
            {
                if (!Left)
                {
                    if (!onWall)
                        transform.position += new Vector3(CurrentDePosition, 0, 0);
                }
            }
            else if (Left)
            {
                if (!onWall)
                    transform.position += new Vector3(-CurrentDePosition, 0, 0);
            }


        }
        else if (isDashing)
        {
            if (dashDirection.x != 0 && dashDirection.y != 0)
            {
                transform.position += ((Vector3)dashDirection) * Time.deltaTime * 35;
            }
            else
            {
                transform.position += ((Vector3)dashDirection) * Time.deltaTime * 50;
            }

            dashStopWatch += Time.deltaTime;
            if (dashStopWatch >= dashDuration)
            {
                CanMove = true;
                isDashing = false;
                dashStopWatch = 0;
                rb.gravityScale = 3;
            }
        }
        else if (isWallJumping)//iswallJumping
        {
            transform.position += new Vector3(CurrentDePosition * wallJumpDirection * 1.1f, CurrentDePosition * 0.9f, 0);
            wallJumpStopWatch += Time.deltaTime;
            if (wallJumpStopWatch >= wallJumpDuration)
            {
                rb.gravityScale = 3;
                wallJumpStopWatch = 0;
                CanMove = true;
                isWallJumping = false;
            }
        }

        if (!canDash)
        {
            dashStopWatch += Time.deltaTime;

            float remainingTime = (dashCoolDown - dashStopWatch);
            float percentage = remainingTime / dashCoolDown;

            if (remainingTime < 1)
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

        if (Input.GetKeyDown(KeyCode.Z))
        {
            PrevSkill();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            NextSkill();
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

        if (Physics2D.BoxCast(transform.position, new Vector2(SizeOfObjHori, 0.02f), 0, Vector2.right, distanceToTheGround, LayerMask))
        {
            hit = WallHit.Right;
        }
        else if (Physics2D.BoxCast(transform.position, new Vector2(SizeOfObjHori, 0.02f), 0, Vector2.left, distanceToTheGround, LayerMask))
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

        bool hit = Physics2D.BoxCast(transform.position, new Vector2(0.02f, SizeOfObjVert), 0, Vector2.down, distanceToSides, LayerMask);

        return hit;
    }

    void NextSkill()
    {
        if (CurrentSkill == Skills.Move)
        {
            CurrentSkill = Skills.Jump;
        }
        else if (CurrentSkill == Skills.Jump)
        {
            CurrentSkill = Skills.Dash;
        }
        else if (CurrentSkill == Skills.Dash)
        {
            CurrentSkill = Skills.Move;
        }
    }
    void PrevSkill()
    {
        if (CurrentSkill == Skills.Move)
        {
            CurrentSkill = Skills.Dash;
        }
        else if (CurrentSkill == Skills.Jump)
        {
            CurrentSkill = Skills.Move;
        }
        else if (CurrentSkill == Skills.Dash)
        {
            CurrentSkill = Skills.Jump;
        }
    }
}