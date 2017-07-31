using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : PhysicsController {
	protected Transform player;

	public float speed = 40;
	[HideInInspector]
	public float stunTimer;

	protected override void Begin() {
		player = GameObject.Find("Player").transform;
	}
	
	void Update () {
		if (!PlayerController.playing) return;

		stunTimer -= Time.deltaTime;
		if (stunTimer >= 0) {
			xSpeed = 0;
		} else {
			AIUpdate();
		}
	}

	public virtual void AIUpdate() {
		Vector2 direction = player.position - transform.position;

		if (direction.x < 0) GetComponent<SpriteRenderer>().flipX = true;
		if (direction.x > 0) GetComponent<SpriteRenderer>().flipX = false;

		xSpeed = speed * direction.x / Mathf.Abs(direction.x);
	}

	public void Kill(bool byHand) {
		if (byHand) {
			player.GetComponent<PlayerController>().hp += 2;
		}
		Destroy(gameObject);
	}
}
