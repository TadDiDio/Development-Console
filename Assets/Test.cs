using UnityEngine;

public class Test : MonoBehaviour
{
    private void Update()
    {
        transform.position += Vector3.up * Mathf.Sin(Time.time) * Time.deltaTime;
        
        //PrintTest();
    }

    private void PrintTest()
    {
        if (Random.value < 0.05)
        {
            float chance = Random.value;
            if (chance > 0.66)
            {
                Debug.LogError("This is an error");
            }
            else if (chance > 0.33)
            {
                Debug.LogWarning("This is a warning");
            }
            else
            {
                print("This is a log");
            }
        }
    }
}
