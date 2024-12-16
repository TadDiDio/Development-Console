using UnityEngine;

public class Mover : MonoBehaviour
{
    private void Update()
    {
        transform.position += Vector3.up * Mathf.Sin(Time.time) * Time.deltaTime;
    }
}
