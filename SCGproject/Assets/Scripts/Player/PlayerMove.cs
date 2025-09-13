using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float maxSpeed;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    public Animator animator;
    private bool start = false;
    private bool starting = false;
    public bool noPower = false;
    public player_power playerPower;
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator.SetBool("isWalking", false);
        animator.SetBool("isSleep", false);
        animator.SetBool("isPhone", false);
    }

    void Update()
    {
        if (Input.GetButtonUp("Horizontal")){
            rigid.velocity = new Vector2(rigid.velocity.normalized.x*0.5f, rigid.velocity.y);
        }
    }

    void FixedUpdate()
    {
        //Move Speed
        float h = Input.GetAxisRaw("Horizontal");
        if(h != 0 && starting == false) {
            starting = true;
            animator.SetBool("start_trigger", true);
            StartCoroutine(startwait());
        }
        if(noPower == true) {
            //침대로 이동
            h = -this.transform.position.x;
            if(h > 0.1f) {
                h = 1;
            }
            else if(h < -0.1f) {
                h = -1;
            }
            else if(Mathf.Abs(h) < 0.1f) {
                h = 0;
            }
        }
        if(animator.GetBool("isSleep") == true) {
            playerPower.IncreasePower(1);
            spriteRenderer.flipX = false;
        }
        if(start == false || animator.GetBool("isSleep") == true || animator.GetBool("isPhone") == true) {
            return;
        }

        // 이동 방식 변경: velocity 직접 설정
        rigid.velocity = new Vector2(h * maxSpeed, rigid.velocity.y);

        //스프라이트 방향 전환
        if (h > 0)
            spriteRenderer.flipX = true;
        else if (h < 0)
            spriteRenderer.flipX = false;
            
        //걷기애니메이션
        animator.SetBool("isWalking", h != 0.0f);
    }

    //시작시 일어나기 애니메이션 시간동안 대기
    IEnumerator startwait()
    {
        yield return new WaitForSeconds(1.0f);
        start = true;
    }
}
