using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;   // puntos extra donde pueden aparecer
    public GameObject[] cans;         // las 7 latas de la escena
    public GameObject[] waters;       // las botellas de agua de almendras

    [Range(0f, 1f)]
    public float originalLayoutChance = 0.3f;    // probabilidad de dejar todo donde lo pusiste
    public bool includeOriginalPositions = true;

    void Awake()
    {
        int active = Mathf.Min(DifficultyManager.WaterCount, waters.Length);
        for (int i = active; i < waters.Length; i++)
            if (waters[i] != null) waters[i].SetActive(false);

        // Partida clásica: todo se queda donde lo pusiste
        if (Random.value < originalLayoutChance) return;

        var positions = new List<Vector3>();
        foreach (var p in spawnPoints)
            if (p != null) positions.Add(p.position);

        if (includeOriginalPositions)
        {
            foreach (var c in cans)
                if (c != null) positions.Add(c.transform.position);
            for (int i = 0; i < active; i++)
                if (waters[i] != null) positions.Add(waters[i].transform.position);
        }

        Shuffle(positions);

        int used = 0;
        foreach (var c in cans)
            Place(c, positions, ref used);
        for (int i = 0; i < active; i++)
            Place(waters[i], positions, ref used);

        if (positions.Count < cans.Length + active)
            Debug.LogWarning("Hay menos lugares posibles que objetos. Agrega más SpawnPoints.");
    }

    void Place(GameObject item, List<Vector3> positions, ref int used)
    {
        if (item == null || used >= positions.Count) return;
        item.transform.position = positions[used];
        used++;
    }

    static void Shuffle(List<Vector3> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}
