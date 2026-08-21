using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] AudioClip persuasionEnteringSound;
    [SerializeField] AudioClip introBGM;
    [SerializeField] AudioClip map1_MainBGM;
    [SerializeField] AudioClip dream_MainBGM;
    [SerializeField] AudioClip guardBGM;
    AudioSource audioSource;
    AudioSource bgmAudioSource;
    Investigation.Inv_GameManager inv_GameManager;
    Investigation.Inv_PlayerCTRL inv_PlayerCTRL;
    SaveManager saveManager;
    public string currScene="";
    public string inv_Scene_ID="";
    public string per_Scene_ID="";
    string return_Inv_Scene_ID="";
    private Vector3? invSceneLastPos=null;
    private string autoInteractOnReturntoInv=null;
    public bool HasPendingAutoInteractionOnReturn => !string.IsNullOrEmpty(autoInteractOnReturntoInv);
    public List<string> sceneNames = new List<string>{"Start", "GamePlayScene", "Investigation", "GamePlayScene"};
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

        audioSource = GetComponent<AudioSource>();
        bgmAudioSource = transform.GetChild(0).GetComponent<AudioSource>();
        saveManager = GetComponent<SaveManager>();
        //currScene = "Start";
        //temporary
        currScene = "Investigation";
    }
    void Start()
    {
        GameStartFromMainScene();
    }
    public void GameStartFromMainScene()
    {
        if(saveManager.TryLoadGeneralSave("FinalMap", out object result))
        {
            StartInvestigation((string)result);
        }
        else {
            StartInvestigation("Map1_Intro");
        }
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
        print("LoadScene: "+id);
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
    IEnumerator ExitSceneCoroutine(bool saveProgress=true)
    {
        if(saveProgress) saveManager.SaveProgress();
        switch (currScene)
        {
            case "Investigation":
                if (inv_GameManager != null)
                {
                    yield return inv_GameManager.FadeScreen(true);
                    inv_GameManager.inputAction?.Player.Disable();
                }
                if (inv_PlayerCTRL != null)
                {
                    inv_PlayerCTRL.inputAction?.Player.Disable();
                    invSceneLastPos = inv_PlayerCTRL.gameObject.transform.position;
                    saveManager.SaveCharacterPosition(currScene, "Player", (Vector3)invSceneLastPos);
                }
                else Debug.LogWarning("PlayerCTRL not detected");
                
                break;
        }
        yield break;
    }
    public void StartInvestigation(string id = "", bool preservePlayerPosition = true)
    {
        print("LoadingInvestigationScene");
        LoadingMotion();
        StartCoroutine(StartInvestigationScene(id, preservePlayerPosition));
    }
    public void ChangeInvestigationMap(string id)
    {
        StartCoroutine(ChangeInvestigationMapAfterDialogue(id));
    }
    IEnumerator ChangeInvestigationMapAfterDialogue(string id)
    {
        // Let the current dialogue finish its button callbacks before reloading the scene.
        yield return null;
        Debug.Log("[ChiefManager] Changing investigation map to: " + id);
        StartInvestigation(id, false);
    }
    IEnumerator StartInvestigationScene(string id="", bool preservePlayerPosition = true)
    {
        //print("1:"+autoInteractOnReturntoInv);
        yield return StartCoroutine(ExitSceneCoroutine(false));
        if (!preservePlayerPosition) invSceneLastPos = null;
        if(id=="") id= return_Inv_Scene_ID;
        //temp
        if(id=="") id = "Map1";
        return_Inv_Scene_ID="";
        per_Scene_ID = "";
        inv_Scene_ID = id;

        //print("2:"+autoInteractOnReturntoInv);
        LoadScene(2);
        //print("3:"+autoInteractOnReturntoInv);
        yield return null;
        Inv_GameManager gm = FindFirstObjectByType<Inv_GameManager>();
        //print("Investigation scene loaded");
        //print("4:"+autoInteractOnReturntoInv);
        saveManager.OnInvestigationSceneStart();
        //print("5:"+autoInteractOnReturntoInv);

        inv_GameManager = GameObject.FindFirstObjectByType<Inv_GameManager>();
        inv_PlayerCTRL = GameObject.FindFirstObjectByType<Inv_PlayerCTRL>();

        yield return new WaitUntil(() => FindFirstObjectByType<Inv_InteractionObj>() != null);

        yield return new WaitUntil(() => FindFirstObjectByType<Inv_Interact>() != null);
        yield return null;
        //print("6:"+autoInteractOnReturntoInv);

        if(invSceneLastPos != null) {
            //print(invSceneLastPos);
            inv_PlayerCTRL.gameObject.transform.position = (Vector3)invSceneLastPos;
        }
        //print("7:"+autoInteractOnReturntoInv);
        if (!string.IsNullOrEmpty(autoInteractOnReturntoInv))
        {
            inv_GameManager.ForceInteract(autoInteractOnReturntoInv);
        }

        invSceneLastPos = null;
        autoInteractOnReturntoInv = null;

        switch(id){
            case "Map1_Intro":
                PlayBGM("Intro");
                break;
            case "Map1":
                if(invSceneLastPos != null) PlayBGM("Map1_Main");
                break;
        }
    }
    public void StartPersuasion(string id, string autoInteractionOnReturn, string returnInvestigationScene = null)
    {
        autoInteractOnReturntoInv = autoInteractionOnReturn;
        LoadingMotion();
        StartCoroutine(StartPersuasionScene(id, returnInvestigationScene));
    }
    IEnumerator StartPersuasionScene(string id, string returnInvestigationScene)
    {
        yield return StartCoroutine(ExitSceneCoroutine(true));
        return_Inv_Scene_ID = string.IsNullOrEmpty(returnInvestigationScene)
            ? inv_Scene_ID
            : returnInvestigationScene;
        inv_Scene_ID = "";
        per_Scene_ID = id;
        audioSource.PlayOneShot(persuasionEnteringSound);

        LoadScene(1);
        //yield return new WaitUntil(() => FindFirstObjectByType<something>() != null);
        //temp
        yield return null;
    }
    public void GameOver(string reason)
    {
        print("Game Over");
        LoadingMotion();
        StartCoroutine(GameOverScene(reason));
    }
    IEnumerator GameOverScene(string reason)
    {
        inv_PlayerCTRL.gameObject.SetActive(false);
        GameObject _gameOverPanel = Instantiate(gameOverPanel, FindFirstObjectByType<Canvas>().transform);
        _gameOverPanel.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = reason;
        _gameOverPanel.SetActive(true);
        print(_gameOverPanel);
        yield return null;
    }
    public void ResetGame()
    {
        SaveManager.ResetAllSaveData();
        LoadScene(0);
    }
    public void PlayBGM(string id){
        AudioClip clip=null;
        print(id);
        switch(id){
            case "Intro":
                clip = introBGM;
                break;
            case "Map1_Main":
                clip = map1_MainBGM;
                break;
            case "Dream_Main":
                clip = dream_MainBGM;
                break;
            case "Guard":
                clip = guardBGM;
                break;
        }
        if(clip==null){
            Debug.LogError("no clip");
            return;
        }
        if(clip == bgmAudioSource.resource) return;
        bgmAudioSource.resource = clip;
        bgmAudioSource.Play();
    }
}
