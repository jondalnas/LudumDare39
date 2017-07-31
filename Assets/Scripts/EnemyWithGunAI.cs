using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWithGunAI : EnemyAI {
	public float shootRange = 8f;
	public GameObject bullet;

	private float cooldown;

	public override void AIUpdate() {
		Vector2 direction = player.position - transform.position;
		if (direction.magnitude < shootRange * shootRange) {
			cooldown -= Time.deltaTime;
			if (cooldown > 0) return;
			cooldown = 2;
			Instantiate(bullet).GetComponent<Bullet>().Set(direction.normalized, transform.position);
		} else {
			cooldown = 0;
			xSpeed = speed * direction.x / Mathf.Abs(direction.x);
		}
	}
}
