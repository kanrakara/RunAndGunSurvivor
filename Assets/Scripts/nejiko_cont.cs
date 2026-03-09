using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NejikoController : MonoBehaviour
{
	CharacterController controller;
	Animator animator;
	
	Vector3 moveDirection = Vector3.zero;
	
	public float gravity;
	public float speedZ;
	public float speedJump;
	
	void Start()
	{
		// 必要なコンポーネントを自動取得
		controller = GetComponent<CharacterController>();
		animator = GetComponent<Animator>();
	}
	
	void Update()
	{
		if (controller.isGrounded)
		{
			if (Input.GetAxis("Vertical") > 0.0f)
			{
				moveDirection.z = Input.GetAxis("Vertical") * speedZ;
			}
			else
			{
				moveDirection.z = 0;
			}
			
			transform.Rotate(0, Input.GetAxis("Horizontal") * 3, 0);
			
			if (Input.GetButton("Jump"))
			{
				moveDirection.y = speedJump;
				animator.SetTrigger("Jump");
			}
		}
		
		// 重力分の力を毎フレーム追加
		moveDirection.y -= gravity * Time.deltaTime;
		
		// 移動実行
		Vector3 globalDirection = transform.TransformDirection(moveDirection);
		controller.Move(globalDirection * Time.deltaTime);
		
		// 移動後接地してたらY方向の速度はリセットする
		if (controller.isGrounded) moveDirection.y = 0;
		
		// 速度が０以上なら走っているフラグをtrueにする
		animator.SetBool("run", moveDirection.z > 0.0f);
	}
}

