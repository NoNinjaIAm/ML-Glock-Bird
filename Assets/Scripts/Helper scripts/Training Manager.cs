using UnityEngine;
using System.IO;

public class TrainingManager : MonoBehaviour
{
    string filePath = "";
    public static int num_epsiodes = 0;
    private int prev_num_episodes = 0;
    public int saveEvery = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        filePath = Application.dataPath + "/episode_count.txt";
    }

    // Update is called once per frame
    void Update()
    {
        if (num_epsiodes > prev_num_episodes)
        {
            
            if (num_epsiodes % saveEvery == 0)
            {
                print("Updating episode count file");
                File.WriteAllText(filePath, "Number of games ran in training: " + num_epsiodes.ToString());
            }

            prev_num_episodes = num_epsiodes;
        }
    }
}
