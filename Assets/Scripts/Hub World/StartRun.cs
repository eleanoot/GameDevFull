﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartRun : MonoBehaviour
{
    [SerializeField]
    private float runTime;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Stats.ChosenTime = runTime;
        SceneManager.LoadScene("Runner");
    }
}