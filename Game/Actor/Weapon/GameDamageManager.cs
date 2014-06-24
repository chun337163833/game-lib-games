using UnityEngine;
using System.Collections;

public class GameDamageManager : MonoBehaviour {
    public AudioClip[] HitSound;
    public GameObject Effect;
    public int HP = 100;

    private void Start() {

    }

    public void ApplyDamage(int damage) {
        if (HP < 0)
            return;
    
        if (HitSound.Length > 0) {
            
            audio.volume = (float)GameProfiles.Current.GetAudioEffectsVolume();

            //GameAudio.Play
            AudioSource.PlayClipAtPoint(HitSound[Random.Range(0, HitSound.Length)], transform.position,
                                        (float)GameProfiles.Current.GetAudioEffectsVolume());
        }
        HP -= damage;
        if (HP <= 0) {
            Dead();
        }
    }

    private void Dead() {
        if (Effect) {
            Instantiate(Effect, transform.position, transform.rotation);
        }
        Destroy(this.gameObject);
    }

}