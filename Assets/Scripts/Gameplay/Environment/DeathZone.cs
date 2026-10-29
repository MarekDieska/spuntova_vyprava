
using UnityEngine;
using System.Collections;

public class DeathZone : MonoBehaviour
{
    private Transform resetPosition;
    public PlayerHealth health;
    private bool isRespawning = false;

    void Start()
    {
        resetPosition = transform.Find("ResetPosition"); //Kazda deathzone ma child objekt "ResetPosition" na tu poziciu sa spawne hrac po pade
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Player")
        {
            StartCoroutine(Respawn(other.gameObject));
        }
    }

    IEnumerator Respawn(GameObject player)
    {
        player.transform.position = resetPosition.position;

        if (!isRespawning)
        {
            isRespawning = true;

            health.HPDown();

            SpriteRenderer sprite = player.GetComponent<SpriteRenderer>();

            if (sprite != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    sprite.enabled = false;
                    yield return new WaitForSeconds(0.1f);

                    sprite.enabled = true;
                    yield return new WaitForSeconds(0.1f);
                }
            }

            isRespawning = false;
        }
    }
}
