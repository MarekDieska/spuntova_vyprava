using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth targetHealth;
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform heartContainer;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;
    private List<Image> spawnedHearts = new();

    private void OnEnable() => targetHealth.OnHealthChangedAction += UpdateHearts;
    private void OnDisable() => targetHealth.OnHealthChangedAction -= UpdateHearts;

    private void UpdateHearts(int currentHp, int maxHp, int delta)
    {
        // Add hearts if maxHp increased
        while (spawnedHearts.Count < maxHp)
        {
            GameObject newHeart = Instantiate(heartPrefab, heartContainer);
            spawnedHearts.Add(newHeart.GetComponent<Image>());
        }

        // Remove hearts if maxHp decreased
        while (spawnedHearts.Count > maxHp)
        {
            int lastIndex = spawnedHearts.Count - 1;
            Destroy(spawnedHearts[lastIndex].gameObject);
            spawnedHearts.RemoveAt(lastIndex);
        }
        for (int i = 0; i < spawnedHearts.Count; i++)
        {
            spawnedHearts[i].sprite = i < currentHp ? fullHeart : emptyHeart;
        }
    }
}
