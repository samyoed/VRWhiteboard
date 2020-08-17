using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetup : MonoBehaviour
{
    public static GameSetup GS;
    public Transform[] spawnPoints;
    public GameObject ground;

    private void OnEnable()
    {
        if(GameSetup.GS == null)
            GameSetup.GS = this;

        ground.SetActive(true);
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(.5f);
    }
}
