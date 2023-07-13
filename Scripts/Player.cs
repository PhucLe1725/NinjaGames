using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

public class Player : Character
{
	[SerializeField] private Rigidbody2D rb;

	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private float speed = 5;
	[SerializeField] private float jumpForce = 350;
	[SerializeField] private Kunai kunaiPrefab;
	[SerializeField] private Transform throwPoint;
	[SerializeField] private GameObject attackArea;

	private bool isGrounded = true;
	private bool isJumping = false;
	private bool isAttack = false;
	private bool isDeath = false;


	private float horizontal;
	

	private int coin = 0;

	private Vector3 savePoint;

    private void Awake()
    {
		coin = PlayerPrefs.GetInt("Coin", 0);
    }

    void Update()
	{
		if (isDeath)
		{
			return;
		}
		isGrounded = CheckGrounded();

		horizontal = Input.GetAxisRaw("Horizontal");
		//vertical = Input.GetAxisRaw("Vertical");

		if (isAttack)
		{
            rb.velocity = Vector2.zero;
            return;
		}

		if (isGrounded)
		{
			if (isJumping)
			{
				return;
			}

            //jump
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
				Jump();
            }

			//change anim run
            if (Mathf.Abs(horizontal) > 0.1f)
			{
				ChangeAnim("run");
			}

            //attack
            if (Input.GetKeyDown(KeyCode.C) && isGrounded)
            {
                Attack();
            }
            //throw
            if (Input.GetKeyDown(KeyCode.V) && isGrounded)
            {
                Throw();
            }
        }
        //check falling
        if (!isGrounded && rb.velocity.y < 0)
        {
            ChangeAnim("fall");
            isJumping = false;
        }

        //moving
        if (Mathf.Abs(horizontal) > 0.1f)
		{
			rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
			transform.rotation = Quaternion.Euler(new Vector3(0,horizontal > 0 ? 0 : 180, 0));
		}
		//idle
		else if (isGrounded)
		{
			ChangeAnim("idle");
			rb.velocity = Vector2.zero;
		}

	}

    public override void OnInit()
	{
		base.OnInit();

		isDeath = false;
		isAttack = false;

		transform.position = savePoint;
		ChangeAnim("idle");
		DeActiveAttack();
        SavePoint();
		UIManager.instance.SetCoin(0);
    }
    protected override void OnDeath()
    {
        base.OnDeath();
    }
    public override void OnDespawn()
    {
        base.OnDespawn();
        OnInit();
    }
    private bool CheckGrounded()
	{
		Debug.DrawLine(transform.position, transform.position + Vector3.down * 1.1f, Color.red);
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundLayer);

		//if(hit.collider != null)
		//{
		//    return true;
		//}
		//else
		//{
		//    return false;
		//}
		return hit.collider != null;
	}
	public void Attack()
	{
		ChangeAnim("attack");
		isAttack = true;
		Invoke(nameof(ResetAttack), 0.5f);
		ActiveAttack();
		Invoke(nameof(DeActiveAttack), 0.5f);
	}
	public void Throw()
	{
        ChangeAnim("throw");
        isAttack = true;
        Invoke(nameof(ResetAttack), 0.5f);

		Instantiate(kunaiPrefab, throwPoint.position, throwPoint.rotation);
    }
	private void ResetAttack()
	{
        isAttack = false;
		ChangeAnim(null);
	}
	public void Jump()
	{
        isJumping = true;
        ChangeAnim("jump");
        rb.AddForce(jumpForce * Vector2.up);
    }
    internal void SavePoint()
    {
        savePoint = transform.position;
    }
	private void ActiveAttack()
	{
		attackArea.SetActive(true);
	}
    private void DeActiveAttack()
    {
        attackArea.SetActive(false);
    }
	public void SetMove(float horizontal)
	{
		this.horizontal = horizontal;
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Coin")
		{
			coin++;
			PlayerPrefs.SetInt("coin", coin);
            UIManager.instance.SetCoin(coin);
            Destroy(collision.gameObject);
		}
		if (collision.tag == "DeathZone")
		{
			isDeath = true;
			ChangeAnim("die");

			Invoke(nameof(OnInit), 1f);
		}
    }
}
