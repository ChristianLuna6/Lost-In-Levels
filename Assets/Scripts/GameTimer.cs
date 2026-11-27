using System.Collections.Generic;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    static GameTimer instance;
    public static GameTimer Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("GameTimer");
                instance = go.AddComponent<GameTimer>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    public float Elapsed { get; private set; }
    public float[] levelTimes = new float[3];
    bool running = true;
    float mark;
    const int MaxRecords = 5;

    void Update()
    {
        if (running) Elapsed += Time.deltaTime;
    }

    public void ResetTimer()
    {
        Elapsed = 0f;
        mark = 0f;
        running = true;
        for (int i = 0; i < levelTimes.Length; i++) levelTimes[i] = 0f;
    }

    public void Stop() { running = false; }

    public void FinishLevel(int index)
    {
        if (index < 0 || index >= levelTimes.Length) return;
        levelTimes[index] = Elapsed - mark;
        mark = Elapsed;
    }

    public static string Format(float t)
    {
        int minutes = (int)(t / 60f);
        int seconds = (int)(t % 60f);
        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    public static List<float> LoadRecords()
    {
        var list = new List<float>();
        for (int i = 0; i < MaxRecords; i++)
        {
            float v = PlayerPrefs.GetFloat("Record" + i, -1f);
            if (v > 0f) list.Add(v);
        }
        return list;
    }

    // Guarda el tiempo y devuelve su posición en la tabla (-1 si no entró)
    public static int SaveRecord(float time)
    {
        var list = LoadRecords();
        list.Add(time);
        list.Sort();
        if (list.Count > MaxRecords) list.RemoveRange(MaxRecords, list.Count - MaxRecords);
        for (int i = 0; i < MaxRecords; i++)
        {
            if (i < list.Count) PlayerPrefs.SetFloat("Record" + i, list[i]);
            else PlayerPrefs.DeleteKey("Record" + i);
        }
        PlayerPrefs.Save();
        return list.IndexOf(time);
    }
}
