using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 3f;
    public float distance = 13f;

    private float startX;
    private int direction = -1;

    void Start()
    {
        startX = transform.position.x;
    }

    void Update()
    {
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);

        if (transform.position.x <= startX - distance) //VLAVO
        {
            direction = 1;
        }

        if (transform.position.x >= startX) //VPRAVO
        {
            direction = -1;
        }
    }
}