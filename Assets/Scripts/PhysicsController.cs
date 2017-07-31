using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsController : MonoBehaviour {

	protected Rigidbody2D rb;
	protected Vector2 velocity;
	protected float xSpeed;
	protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
	protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);

	protected float minGroundNormalY = 0.65f;
	protected bool grounded;
	protected Vector2 groundNormal;
	protected float gravity = 10;

	protected const float deadZone = 0.001f;
	protected const float padding = 0.01f;

	public ContactFilter2D contactFilter;

	void Start() {
		rb = GetComponent<Rigidbody2D>();
		contactFilter.useTriggers = false;
		contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
		contactFilter.useLayerMask = true;
		Begin();
	}

	protected virtual void Begin() {
	}

	void Update() {
	}

	void FixedUpdate() {
		grounded = false;

		velocity += Physics2D.gravity * Time.deltaTime * gravity;
		velocity.x = xSpeed;

		Vector2 delta = velocity * Time.deltaTime;

		Move(delta.y * Vector2.up);
		Move(delta.x * Vector2.right);
	}

	protected virtual void Move(Vector2 delta) {
		float distance = delta.magnitude;

		if (distance > deadZone) {
			int count = rb.Cast(delta, contactFilter, hitBuffer, distance+padding);

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

					if (delta.y != 0) {
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

		rb.position += delta.normalized*distance;
	}

	public virtual void hit(GameObject go) {
	}
}
