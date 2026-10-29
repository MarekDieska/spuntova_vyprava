
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int hp = 3;

    public Sprite fullHeart;
    public Sprite emptyHeart;
    public Image[] hearts;

    void Start()
    {
        UpdateHearts();
    }

    public void HPDown()
    {
        if (hp <= 0)
            return;

        hp--;
        UpdateHearts();

        if (hp == 0)
        {
            Debug.Log("Tu pridať smrť");
        }
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < hp ? fullHeart : emptyHeart;
        }
    }
}
