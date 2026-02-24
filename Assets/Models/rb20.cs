using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class CSVMovementPlayer : MonoBehaviour
{
    public string fileName = "f1_motion_dump/ALB_data.csv";
    public LayerMask groundLayer = ~0;   // all layers; narrow this in the Inspector if needed
    public float groundOffset = 0f;      // raise model slightly above ground if needed
    public Vector3 rotationOffset = Vector3.zero; // fix model axis mismatch (e.g. set X to -90 if car faces down)

    private string filePath;
    private Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Y is handled by ground raycast, not gravity
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        StartCoroutine(PlayMovement());
    }

    void FixedUpdate() {
        // Raycast straight down to find the ground (works on slopes)
        RaycastHit hit;
        if (Physics.Raycast(rb.position + Vector3.up * 5f, Vector3.down, out hit, 20f, groundLayer)) {
            float targetY = hit.point.y + groundOffset;
            // Smoothly snap Y to the terrain surface
            float newY = Mathf.Lerp(rb.position.y, targetY, Time.fixedDeltaTime * 20f);
            rb.MovePosition(new Vector3(rb.position.x, newY, rb.position.z));
        }

        // Rotate car to face movement direction (yaw only)
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.sqrMagnitude > 0.001f) {
            // 1. Get the direction the car is moving
            Quaternion lookRot = Quaternion.LookRotation(flatVel.normalized, Vector3.up);
            
            // 2. Apply ONLY the -90 degree X rotation offset
            // We multiply lookRot by the Euler offset to rotate the model locally
            Quaternion targetRot = lookRot * Quaternion.Euler(-90f, 0f, 0f);
            
            // 3. Smoothly rotate the Rigidbody
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 15f));
        }
    }

    IEnumerator PlayMovement() {
        yield return new WaitForSeconds(3f);
        string[] lines = File.ReadAllLines(filePath);

        // Teleport to the starting XZ position
        string[] firstValues = lines[1].Split(',');
        rb.position = new Vector3(float.Parse(firstValues[1]), rb.position.y, float.Parse(firstValues[2]));

        float stepDuration = 0.1f; // seconds between CSV rows

        for (int i = 1; i < lines.Length - 1; i++) {
            string[] curValues  = lines[i].Split(',');
            string[] nextValues = lines[i + 1].Split(',');

            float vx = (float.Parse(nextValues[1]) - float.Parse(curValues[1])) / stepDuration;
            float vz = (float.Parse(nextValues[2]) - float.Parse(curValues[2])) / stepDuration;

            // Only drive XZ; Y is managed by FixedUpdate ground snap
            rb.velocity = new Vector3(vx, 0f, vz);

            yield return new WaitForSeconds(stepDuration);
        }

        rb.velocity = Vector3.zero;
    }
}
