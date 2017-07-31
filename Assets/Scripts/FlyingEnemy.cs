using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : EnemyAI {
	private float overHeadTimer;
	private Vector3 targetOffset;
	private Vector3 targetOffset0;
	private Vector3 targetOffset1;

	protected override void Move(Vector2 delta) {
		if (targetOffset == Vector3.zero) {
			targetOffset0 = new Vector3(Random.Range(1f, 2f), Random.Range(1f, 5f));
			targetOffset1 = new Vector3(Random.Range(-2f, 1f), targetOffset0.y);

			targetOffset = targetOffset0;
		}

		if (GetComponent<Collider2D>().OverlapPoint(player.transform.position + targetOffset)) {
			if (targetOffset == targetOffset0) {
				targetOffset = targetOffset1;
			} else {
				targetOffset = targetOffset0;

			}
		}

		overHeadTimer += Time.deltaTime;
		if (overHeadTimer > Random.Range(2.5f, 5.5f)) targetOffset = new Vector3(0, 0.01f);

		if (stunTimer >= 0) return;
		Vector2 direction = player.transform.position + targetOffset - transform.position;
		delta = speed * direction.normalized * Time.deltaTime;

		float distance = delta.magnitude;

		if (distance > deadZone) {
			int count = rb.Cast(delta, hitBuffer, distance + padding);

			hitBufferList.Clear();
			for (int i = 0; i < count; i++) {
				hitBufferList.Add(hitBuffer[i]);
			}

			for (int i = 0; i < hitBufferList.Count; i++) {
				PhysicsController pc = hitBufferList[i].transform.GetComponent<PhysicsController>();
				if (pc != null) pc.hit(gameObject);

				float modDistance = hitBufferList[i].distance - padding;
				distance = modDistance < distance ? modDistance : distance;
			}
		}

		rb.position += delta.normalized * distance;
	}

	public override void AIUpdate() {
	}
}
