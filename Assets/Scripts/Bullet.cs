using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : EnemyAI {
	public float bulletSpeed = 20;
	public Vector2 direction;

	protected override void Move(Vector2 delta) {
		if (!PlayerController.playing) return;

		delta = direction * bulletSpeed * Time.deltaTime;
		float distance = delta.magnitude;

		if (distance > deadZone) {
			int count = rb.Cast(delta, contactFilter, hitBuffer, distance + padding);

			hitBufferList.Clear();
			for (int i = 0; i < count; i++) {
				hitBufferList.Add(hitBuffer[i]);
			}

			for (int i = 0; i < hitBufferList.Count; i++) {
				PhysicsController pc = hitBufferList[i].transform.GetComponent<PhysicsController>();
				if (pc != null) {
					pc.hit(gameObject);
				}

				Vector2 currNormal = hitBufferList[i].normal;

				if (currNormal.y > minGroundNormalY) {
					grounded = true;

					if (delta.y != 0)
					{
						groundNormal = currNormal;
						currNormal.x = 0;
					}
				}

				float dotProduct = Vector2.Dot(velocity, currNormal);

				if (dotProduct < 0) {
					velocity -= dotProduct * currNormal;
				}

				float modDistance = hitBufferList[i].distance - padding;
				distance = modDistance < distance ? modDistance : distance;
			}
		}

		rb.position += delta.normalized * distance;
	}

	public void Set(Vector2 direction, Vector3 pos) {
		if (direction.y < 0) direction.y = 0;
		this.direction = direction;
		transform.position = pos + new Vector3(direction.x, direction.y);
	}
}
