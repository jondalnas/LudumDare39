using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerController : PhysicsController {
	public float playerSpeed = 50;
	public float jumpSpeed = 50;
	public float maxHP = 10;
	public RectTransform healtBar;
	public GameObject inGameGUI;
	public Transform gun;
	[HideInInspector]
	public float hp = 5;
	[HideInInspector]
	public bool forceDirection;
	public Text finalChance;
	public AudioClip teleportSound;
	public AudioClip shootSound;
	public AudioClip attackSound;
	public AudioClip finalChanceSound;
	public GameObject lightLevel;
	public GameObject whiteScreen;

	public static bool playing;
	public static float kills;

	private float heldDown;
	private Animator anim;
	private bool dead;
	private bool gunRight;
	private float imunity;
	private float slowmotion;
	private float cooldown;
	private float swordCooldown;

	protected override void Begin() {
		anim = GetComponent<Animator>();
		hp = maxHP + Time.deltaTime;
	}

	void Update () {
		if (kills >= 100) win();

		if (Input.GetButtonDown("Restart")) {
			restart();
		}

		if (Input.GetButtonDown("Pause")) {
			playing = !playing;
		}

		if (!playing) return;

		if (slowmotion > 0) {
			slowmotion -= Time.deltaTime;
			Time.timeScale = 0.4f;
		} else {
			Time.timeScale = 1f;
		}

		imunity -= Time.deltaTime;

		//Helth calcuations
		if (hp > maxHP) hp = maxHP;
		if (hp <= 0) {
			hp = 0;
			die();
		}

		if (hp > 0 && dead) {
			dead = false;
			slowmotion = 0;
			finalChance.enabled = false;
		}

		healtBar.sizeDelta = new Vector2((hp) / (maxHP) * (200f - 23f) + 23f, 50);
		healtBar.anchoredPosition = new Vector2((hp) / (maxHP) * (200f - 23f) + 23f, -25);
		if (isDead()) {
			xSpeed = 0;
			anim.SetBool("Dead", true);
			playing = false;
			foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy")) {
				Destroy(go);
			}
			return;
		}
		hp -= Time.deltaTime;

		//Move
		xSpeed = playerSpeed * Input.GetAxis("Horizontal");

		if (xSpeed < -deadZone) {
			transform.localScale = new Vector3(-6, transform.localScale.y, transform.localScale.z);
			anim.SetBool("Walking", true);
		} else if (xSpeed > deadZone) {
			transform.localScale = new Vector3(6, transform.localScale.y, transform.localScale.z);
			anim.SetBool("Walking", true);
		} else {
			anim.SetBool("Walking", false);
		}
		if (forceDirection) {
			if (gunRight) {
				transform.localScale = new Vector3(6, transform.localScale.y, transform.localScale.z);
			} else {
				transform.localScale = new Vector3(-6, transform.localScale.y, transform.localScale.z);
			}
		}

		//Jump
		if (Input.GetButton("Jump") && (grounded||heldDown<10)) {
			heldDown++;
			velocity.y = jumpSpeed;
		} else if (!Input.GetButton("Jump") && !grounded) {
			heldDown = 10;
		}

		//Jump animation
		if (grounded) {
			heldDown = 0;
			anim.SetBool("InAir", false);
		} else {
			anim.SetBool("InAir", true);
		}

		//Sword
		if (Input.GetMouseButtonDown(0) && swordCooldown < 0) {
			swordCooldown = 0.5f;
			GetComponent<AudioSource>().clip = attackSound;
			GetComponent<AudioSource>().Play();
			anim.SetBool("Attack", true);
		} else anim.SetBool("Attack", false);

		//Gun
		if (Input.GetMouseButtonDown(1) && cooldown < 0) {
			GetComponent<AudioSource>().clip = shootSound;
			GetComponent<AudioSource>().Play();

			cooldown = 0.4f;

			Vector3 direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - gun.position;
			direction.Normalize();
			RaycastHit2D[] hitInfos = Physics2D.RaycastAll(gun.position, direction, 20);

			gunRight = direction.x > 0;

			GameObject line = new GameObject();
			line.transform.position = gun.position;
			line.AddComponent<LineRenderer>();
			LineRenderer lr = line.GetComponent<LineRenderer>();
			lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
			lr.startColor = Color.red;
			lr.endColor = Color.red;
			lr.startWidth = 0.1f;
			lr.endWidth = 0.1f;
			lr.SetPosition(0, gun.position);
			lr.SetPosition(1, direction.normalized * 20f+gun.position);
			GameObject.Destroy(line, 0.1f);

			for (int i = 0; i < hitInfos.Length; i++) {
				RaycastHit2D hitInfo = hitInfos[i];
				if (hitInfo && hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
					EnemyAI enemy = hitInfo.transform.GetComponent<EnemyAI>();
					if (enemy != null) {
						enemy.stunTimer = 2;
					}
				}
			}

			anim.SetBool("Shoot", true);
		} else {
			anim.SetBool("Shoot", false);
		}

		//Teleport
		if (Input.GetMouseButtonDown(2) && cooldown < 0) {
			cooldown = 0.4f;
			Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Transform collider = transform.Find("Teleport");
			collider.position = pos+transform.position;
			ContactFilter2D cf = new ContactFilter2D();
			cf.NoFilter();

			collider.GetComponent<Collider2D>().enabled = true;

			Collider2D[] cols = new Collider2D[16];

			if (collider.GetComponent<Collider2D>().OverlapCollider(cf, cols) == 0) {
				transform.position = pos;
				GetComponent<AudioSource>().clip = teleportSound;
				GetComponent<AudioSource>().Play();
			}
			collider.GetComponent<Collider2D>().enabled = false;

			if (slowmotion < 0.4f)
				slowmotion = 0.4f;
		}

		cooldown -= Time.deltaTime;
		swordCooldown -= Time.deltaTime;
	}

	public override void hit(GameObject go) {
		if (go.transform.CompareTag("Enemy")) {
			go.GetComponent<EnemyAI>().Kill(false);
			if (imunity > 0) return;
			imunity = 2;
			hp -= 2;
		}
	}

	void die() {
		if (slowmotion < 0.8f) {
			finalChance.enabled = false;
			finalChance.fontSize = 0;
		} else {
			if (!finalChance.enabled) {
				GetComponent<AudioSource>().clip = finalChanceSound;
				GetComponent<AudioSource>().Play(0);
			}

			finalChance.enabled = true;
			if (finalChance.fontSize < 58) finalChance.fontSize += 2;
		}
		if (dead) return;
		dead = true;
		healtBar.GetChild(0).GetComponent<ParticleSystem>().Stop();
		slowmotion = 1.25f;
	}

	public void restart() {
		anim.SetBool("Dead", false);
		anim.SetBool("Win", false);
		dead = false;
		healtBar.GetChild(0).GetComponent<ParticleSystem>().Play();
		finalChance.enabled = false;
		foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy")) {
			Destroy(go);
		}
		transform.position = Vector3.zero;
		hp = maxHP;
		slowmotion = 0;
		kills = 0;
		lightLevel.SetActive(false);
		whiteScreen.SetActive(false);
		playing = true;

		inGameGUI.SetActive(true);
	}

	public bool isDead() {
		return dead && slowmotion <= 0;
	}

	public void startGame() {
		restart();
		playing = true;
	}

	public void win() {
		xSpeed = 0;
		anim.SetBool("Win", true);
		playing = false;
		foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy")) {
			Destroy(go);
		}

		inGameGUI.SetActive(false);
	} 
}
