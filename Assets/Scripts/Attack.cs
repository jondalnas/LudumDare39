using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour {
	public AudioClip killedSound;

	void FixedUpdate() {
		if (!PlayerController.playing) return;

		ContactFilter2D cf = new ContactFilter2D();
		cf.NoFilter();

		Collider2D[] cols = new Collider2D[32];
		transform.GetComponent<Collider2D>().OverlapCollider(cf, cols);

		for (int i = 0; i < cols.Length; i++) {
			Collider2D col = cols[i];
			if (col == null) return;

			if (col.transform.CompareTag("Enemy")) {
				GetComponent<AudioSource>().clip = killedSound;
				GetComponent<AudioSource>().Play();
				col.gameObject.GetComponent<EnemyAI>().Kill(true);
				PlayerController.kills++;
			}
		}
	}
}
