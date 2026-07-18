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
    public string currScene="";
    public string inv_Scene_ID="";
    public string per_Scene_ID="";
    string return_Inv_Scene_ID="";
    public List<string> sceneNames = new List<string>{"Start", "Persuasion", "Investigation"};
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
        //currScene = "Start";
        //temporary
        currScene = "Investigation";
        StartInvestigation("Map1");
    }
    void LoadScene(string id)
    {
        currScene = id;
        SceneManager.LoadScene(sceneNames.IndexOf(id));
    }
    void LoadingMotion()
    {
        
    }
    void ExitScene()
    {
        switch (currScene)
        {
            case "Investigation":
                if (inv_GameManager != null)
                {
                    inv_GameManager.inputAction?.Player.Disable();
                }
                if (inv_PlayerCTRL != null)
                {
                    inv_PlayerCTRL.inputAction?.Player.Disable();
                }
                break;
        }
    }
    public void StartInvestigation(string id = "")
    {
        LoadingMotion();
        StartCoroutine(StartInvestigationScene(id));
    }
    IEnumerator StartInvestigationScene(string id="")
    {
        ExitScene();
        if(id=="") id= return_Inv_Scene_ID;
        return_Inv_Scene_ID="";
        per_Scene_ID = "";
        inv_Scene_ID = id;

        SceneManager.LoadScene(2);
        yield return new WaitUntil(() => FindFirstObjectByType<Inv_GameManager>() != null);
        saveManager.OnInvestigationSceneStart();

        inv_GameManager = GameObject.FindFirstObjectByType<Inv_GameManager>();
        inv_PlayerCTRL = GameObject.FindFirstObjectByType<Inv_PlayerCTRL>();
    }
    public void StartPersuasion(string id)
    {
        LoadingMotion();
        StartCoroutine(StartPersuasionScene(id));
    }
    IEnumerator StartPersuasionScene(string id)
    {
        ExitScene();
        return_Inv_Scene_ID = inv_Scene_ID;
        inv_Scene_ID = "";
        per_Scene_ID = id;

        SceneManager.LoadScene(3);
        //yield return new WaitUntil(() => FindFirstObjectByType<something>() != null);
        //temp
        yield return null;
    }
}
