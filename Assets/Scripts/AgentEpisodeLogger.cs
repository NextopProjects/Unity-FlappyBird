using System;
using System.IO;
using UnityEngine;

public class AgentEpisodeLogger : MonoBehaviour
{
    public static AgentEpisodeLogger Instance { get; private set; }

    private string _filePath;
    private float _episodeStartTime;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _filePath = Path.Combine(Application.dataPath, $"agent_episode_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

        if (!File.Exists(_filePath))
        {
            File.WriteAllText(
                _filePath,
                "episode,steps,survival_time,score,epsilon\n"
            );
        }
    }

    // Episode 시작
    public void StartEpisode()
    {
        _episodeStartTime = Time.time;
    }

    // Episode 종료 시 기록
    public void LogEpisode(
        int episode,
        int steps,
        int score,
        float epsilon)
    {
        float survivalTime = Time.time - _episodeStartTime;

        string line =
            $"{episode},{steps},{survivalTime},{score},{epsilon}\n";

        File.AppendAllText(_filePath, line);
    }
}