using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
	public GameObject[] enemies;
	public float levelWidth;

	private float leftScreen;
	private float rightScreen;
	private float timer;

	void Start () {
		Spawn();
		leftScreen = Camera.main.transform.position.x + Camera.main.ScreenToWorldPoint(new Vector3(0, 0)).x;
		rightScreen = Camera.main.transform.position.x + Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0)).x;
	}
	
	void Update () {
		if (!PlayerController.playing) return;

		timer -= Time.deltaTime;
		if (timer < 0) {
			Spawn();
			timer = 2f;
		}
	}

	void Spawn() {
		if (GameObject.FindGameObjectsWithTag("Enemy").Length > 20) return;

		GameObject enemy = enemies[Random.Range(0, enemies.Length)];
		enemy.transform.position = new Vector3(Random.Range(-levelWidth / 8.0f + Camera.main.transform.position.x, levelWidth / 8.0f + Camera.main.transform.position.x), 0);
		int spawn = Random.Range(1, 8);
		for (int i = 0; i < spawn; i++) {
			enemy.transform.position = new Vector2(enemy.transform.position.x+Random.Range(-2f, 2f), enemy.GetComponent<FlyingEnemy>()!=null? Random.Range(1, 10) : 0);
			if (enemy.transform.position.x < leftScreen || enemy.transform.position.x > rightScreen) Instantiate(enemy);
		}
	}
}
