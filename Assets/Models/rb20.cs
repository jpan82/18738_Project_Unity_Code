using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class CSVMovementPlayer : MonoBehaviour
{
    public string fileName = "f1_motion_dump/ALB_data.csv";
    private string filePath;

    void Start()
    {
        filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        StartCoroutine(PlayMovement());
    }
    IEnumerator PlayMovement()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("CSV File not found at " + filePath);
            yield break;
        }

        string[] lines = File.ReadAllLines(filePath);

        for (int i = 1; i < lines.Length - 1; i++) // Stop at Length - 1 to look ahead
        {
            // 1. Get Current Position
            string[] currentValues = lines[i].Split(',');
            Vector3 startPos = transform.position;

            // 2. Get Next Position (The Target)
            string[] nextValues = lines[i + 1].Split(',');
            float nextX = float.Parse(nextValues[1]);
            float nextZ = float.Parse(nextValues[2]);
            float nextY = float.Parse(nextValues[3]);
            Vector3 endPos = new Vector3(nextX, nextY, nextZ);

            // 3. Interpolate over the 0.1s interval
            float duration = 0.1f; 
            float elapsed = 0f;

            while (elapsed < duration)
            {
                // Calculate how far we are through the 0.1s window (0 to 1)
                float t = elapsed / duration;

                // Smoothly move the model
                transform.position = Vector3.Lerp(startPos, endPos, t);

                // Optional: Make the car face where it's going
                // if (startPos != endPos) 
                // {
                //     transform.forward = Vector3.Lerp(transform.forward, (endPos - startPos).normalized, t);
                // }

                elapsed += Time.deltaTime; // Time since last frame
                yield return null; // Wait for the very next frame (e.g., 1/60th of a second)
            }
        }
    }
}
