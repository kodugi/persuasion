using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Investigation;
using System;
using UnityEngine.SceneManagement;

public partial class ChiefManager : MonoBehaviour
{
    public static ChiefManager Instance { get; private set; }
    Investigation.Inv_GameManager inv_GameManager;
    Investigation.Inv_PlayerCTRL inv_PlayerCTRL;
    SaveManager saveManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveManager = GetComponent<SaveManager>();

        //temporary
        StartInvestigation();
    }
    public void StartInvestigation()
    {
        saveManager.OnInvestigationSceneStart();
        inv_GameManager = GameObject.FindFirstObjectByType<Inv_GameManager>();
        inv_PlayerCTRL = GameObject.FindFirstObjectByType<Inv_PlayerCTRL>();
    }
    public void StartPersuasion(string id)
    {
        inv_GameManager.inputAction.Player.Disable();
        inv_PlayerCTRL.inputAction.Player.Disable();
        SceneManager.LoadScene(0);
    }
}
