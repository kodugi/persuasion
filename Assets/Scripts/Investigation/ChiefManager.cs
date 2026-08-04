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
    private Vector3? invSceneLastPos=null;
    private string autoInteractOnReturntoInv=null;
    public List<string> sceneNames = new List<string>{"Start", "Persuasion", "Investigation", "Persuasion"};
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
    void FixedUpdate()
    {
        if(currScene == "Persuasion")
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                print("Developer Option: Skipping the Puzzle");
                StartInvestigation();
            }
        }
    }
    void LoadScene(object id)
    {
        print(id);
        if (id is string sceneName)
        {
            currScene = sceneName;
            SceneManager.LoadScene(sceneNames.IndexOf(sceneName));
        }
        else if (id is int sceneIndex)
        {
            /*
            //temp
            if(sceneIndex == 2) currScene = "Investigation";
            else currScene = "Persuasion";*/
            currScene = sceneNames[sceneIndex];
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            throw new ArgumentException("id must be either string or int.");
        }
        //print(currScene);
    }
    void LoadingMotion()
    {
        
    }
    void ExitScene(bool saveProgress=true)
    {
        if(saveProgress) saveManager.SaveProgress();
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
                    invSceneLastPos = inv_PlayerCTRL.gameObject.transform.position;
                    print(invSceneLastPos);
                }
                else Debug.LogWarning("PlayerCTRL not detected");
                
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
        ExitScene(false);
        if(id=="") id= return_Inv_Scene_ID;
        //temp
        if(id=="") id = "Map1";
        return_Inv_Scene_ID="";
        per_Scene_ID = "";
        inv_Scene_ID = id;

        LoadScene(3);
        yield return new WaitUntil(() => FindFirstObjectByType<Inv_GameManager>() != null);
        //print("Investigation scene loaded");
        saveManager.OnInvestigationSceneStart();

        inv_GameManager = GameObject.FindFirstObjectByType<Inv_GameManager>();
        inv_PlayerCTRL = GameObject.FindFirstObjectByType<Inv_PlayerCTRL>();

        // Ensure the loaded progress dictionary is applied before interaction objects initialize their state.
        if (saveManager != null)
        {
            foreach (var obj in FindObjectsOfType<Inv_InteractionObj>())
            {
                obj.CheckState();
            }
        }

        if(invSceneLastPos != null) {
            //print(invSceneLastPos);
            inv_PlayerCTRL.gameObject.transform.position = (Vector3)invSceneLastPos;
        }
        if(autoInteractOnReturntoInv != null) inv_GameManager.ForceInteract(autoInteractOnReturntoInv);

        invSceneLastPos = null;
        autoInteractOnReturntoInv = null;
    }
    public void StartPersuasion(string id, string autoInteractionOnReturn)
    {
        autoInteractOnReturntoInv = autoInteractionOnReturn;
        LoadingMotion();
        StartCoroutine(StartPersuasionScene(id));
    }
    IEnumerator StartPersuasionScene(string id)
    {
        ExitScene(true);
        return_Inv_Scene_ID = inv_Scene_ID;
        inv_Scene_ID = "";
        per_Scene_ID = id;

        LoadScene(2);
        //yield return new WaitUntil(() => FindFirstObjectByType<something>() != null);
        //temp
        yield return null;
    }
}
