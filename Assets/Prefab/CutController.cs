using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public void Pausegame()
    {
        Time.timeScale = 0;
    }

    public void Startgame()
    {
        Time.timeScale = 1;
    }
}
