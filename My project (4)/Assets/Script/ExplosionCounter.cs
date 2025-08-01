using TMPro;
using Akila.FPSFramework;
using UnityEngine;

public class ExplosionCounter : MonoBehaviour
{
    public int explosionCount = 0;
    public int maxCount = 6;
    public TMP_Text Count;

    private void OnEnable()
    {
        Explosive[] explosives = FindObjectsOfType<Explosive>();
        foreach (var explosive in explosives)
        {
            explosive.onExplode += OnExplosiveExploded;
        }
    }

    private void OnDisable()
    {
        Explosive[] explosives = FindObjectsOfType<Explosive>();
        foreach (var explosive in explosives)
        {
            explosive.onExplode -= OnExplosiveExploded;
        }
    }

    private void OnExplosiveExploded()
    {
        if (explosionCount >= maxCount) return;

        explosionCount++;
        Debug.Log($"Æø¹ß ¹ß»ý! ÇöÀç±îÁö Æø¹ß È½¼ö: {explosionCount} / {maxCount}");

        if (Count != null )
            Count.text = explosionCount+ "/" +maxCount;




    }
}