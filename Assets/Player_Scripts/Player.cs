using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//
public class Player : MonoBehaviour
	{
	private Animator animator;
	[SerializeField]
	Vector3 _startPosition;
	private void Start ()
	{
		animator = GetComponent<Animator>();
	}

	public void Awake()
	{
		GameManager.Instance.SetStartPosition(_startPosition); // to grab a position to reset to when the player dies without touching a checkpoint
	}
	//https://bergstrand-niklas.medium.com/setting-up-a-simple-game-manager-in-unity-24b080e9516c
	public void KillPlayer()
	{
		animator.SetBool("isDead", true);
		Invoke(nameof(murder), 0.6f);
	}
	private void murder()
	{
		transform.position = GameManager.Instance.StartPosition; //sends the player back to the last updated checkpoint/startpositon 
		animator.SetBool("isDead", false);
	}
}

