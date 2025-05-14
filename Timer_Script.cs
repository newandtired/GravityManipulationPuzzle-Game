using UnityEngine;
using TMPro;

public class Timer_Script : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI timertext;
    [SerializeField] TextMeshProUGUI Gameoverscreen;
    [SerializeField] Player_Movement playerMovement;

    [SerializeField] float elapsedTime;

    private bool hasTriggered = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Gameoverscreen.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timertext.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (!hasTriggered && elapsedTime <= 0f)
        {
            if (playerMovement != null && playerMovement.BoxCount == 5)
            {
                hasTriggered = true;
                
            }
            else
            {
                Time.timeScale = 0f;
                Gameoverscreen.enabled = true;
            }
        }

        if (playerMovement.flag == false) 
        {
            Gameoverscreen.enabled = true;
        }
    }
}
