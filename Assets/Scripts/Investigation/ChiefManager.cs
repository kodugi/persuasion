using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Investigation;
using System;
using UnityEngine.SceneManagement;

public partial class ChiefManager : MonoBehaviour
{
    public const string IntroBGMId = "Intro";
    public const string MainBGMId = "Map1_Main";
    public const string DreamBGMId = "Dream_Main";
    public const string JumpScareSoundId = "JumpScare";
    public const string GameOverSoundId = "GameOver";
    public const string LaughterSoundId = "Laughter";
    public const string SoulPlaceSoundId = "SoulPlace";
    public const string EyeSoundId = "Eye";
    public const string BigEyeSoundId = "BigEye";
    public const string GlitchSoundId = "Glitch";

    [SerializeField] private string initialMapId;
    public static ChiefManager Instance { get; private set; }
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] AudioClip persuasionEnteringSound;
    [SerializeField] AudioClip introBGM;
    [SerializeField] AudioClip map1_MainBGM;
    [SerializeField] AudioClip dream_MainBGM;
    [SerializeField] AudioClip guardBGM;
    [Header("Persuasion Sound Effects")]
    [SerializeField] AudioClip jumpScareSound;
    [SerializeField] AudioClip gameOverSound;
    [SerializeField] AudioClip[] laughterSounds = new AudioClip[3];
    [SerializeField] AudioClip soulPlaceSound;
    [SerializeField] AudioClip eyeSound;
    [SerializeField] AudioClip bigEyeSound;
    [SerializeField] AudioClip glitchSound;
    [SerializeField, Range(0f, 1f)] float soundEffectVolume = 0.75f;
    [SerializeField, Range(0f, 1f)] float jumpScareVolume = 1f;
    [SerializeField, Min(0f)] float jumpScareDuration = 3f;
    AudioSource audioSource;
    AudioSource bgmAudioSource;
    AudioSource loopingEffectAudioSource;
    Coroutine laughterCoroutine;
    Coroutine jumpScareStopCoroutine;
    bool audioLockedForJumpScare;
    bool isReturningToStartScene;
    string lastBGMId;
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
        ConfigureAudioSource(audioSource, false);
        ConfigureAudioSource(bgmAudioSource, true);

        loopingEffectAudioSource = gameObject.AddComponent<AudioSource>();
        ConfigureAudioSource(loopingEffectAudioSource, true);
        loopingEffectAudioSource.playOnAwake = false;

        saveManager = GetComponent<SaveManager>();
        //currScene = "Start";
        //temporary
        currScene = "Investigation";
    }
    void Start()
    {
        switch (currScene)
        {
            case "Start":
                Destroy(gameObject);
                break;
            case "Investigation":
                GameStartFromMainScene();
                break;
        }
    }
    public void GameStartFromMainScene()
    {
        Debug.LogWarning("UniconTempCodeisRemaining");
        if (onlyPuzzles)
        {
            StartInvestigation("Map_Unicon_Temp");
            return;
        }

        if(saveManager.TryLoadGeneralSave("FinalMap", out object result))
        {
            StartInvestigation((string)result);
        }
        else {
            StartInvestigation(initialMapId);
        }
    }
    void FixedUpdate()
    {
        TempSkipCheckerOnUpdate();
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
        string sourceInvestigationId = inv_Scene_ID;

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

                    if (!string.IsNullOrEmpty(sourceInvestigationId))
                    {
                        saveManager.SaveCharacterPosition(
                            sourceInvestigationId,
                            "Player",
                            (Vector3)invSceneLastPos
                        );
                    }
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
        saveManager.PrepareForInvestigationSceneLoad();

        //temp
        Debug.LogWarning("UniconTempCodeisRemaining");
        if(onlyPuzzles) LoadScene(3);
        else LoadScene(2);

        //print("3:"+autoInteractOnReturntoInv);
        yield return null;
        ResetAudioAfterGameOver(false);
        //print("Investigation scene loaded");
        //print("4:"+autoInteractOnReturntoInv);
        saveManager.OnInvestigationSceneStart();
        //print("5:"+autoInteractOnReturntoInv);

        inv_GameManager = GameObject.FindFirstObjectByType<Inv_GameManager>();
        inv_PlayerCTRL = GameObject.FindFirstObjectByType<Inv_PlayerCTRL>();

        if (inv_GameManager == null || inv_PlayerCTRL == null)
        {
            Debug.LogError(
                "[ChiefManager] Investigation scene managers were not ready after loading."
            );
            yield break;
        }

        inv_GameManager.FadeScreen(false);

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


        switch(id){
            case "Map1_Intro":
                PlayBGM("Intro");
                break;
            case "Map1":
                if(invSceneLastPos != null) PlayBGM("Map1_Main");
                break;
            case "Map_Dream":
            case "Dream":
                PlayBGM("Dream_Main");
                break;
        }
        invSceneLastPos = null;
        autoInteractOnReturntoInv = null;
    }
    public void StartPersuasion(string id, string autoInteractionOnReturn, string returnInvestigationScene = null)
    {
        autoInteractOnReturntoInv = autoInteractionOnReturn;
        LoadingMotion();
        StartCoroutine(StartPersuasionScene(id, returnInvestigationScene));
    }
    IEnumerator StartPersuasionScene(string id, string returnInvestigationScene)
    {
        string sourceInvestigationId = inv_Scene_ID;
        yield return StartCoroutine(ExitSceneCoroutine(true));
        return_Inv_Scene_ID = string.IsNullOrEmpty(returnInvestigationScene)
            ? sourceInvestigationId
            : returnInvestigationScene;

        // A persuasion scene may return to a different investigation map.
        // Only carry the last position back when returning to the same map;
        // otherwise the destination's saved position (or JSON default) must win.
        if (!string.Equals(
                sourceInvestigationId,
                return_Inv_Scene_ID,
                StringComparison.Ordinal))
        {
            invSceneLastPos = null;
        }

        inv_Scene_ID = "";
        per_Scene_ID = id;
        PlayBGM(IsDreamPersuasion(id) ? DreamBGMId : MainBGMId);
        audioSource.PlayOneShot(persuasionEnteringSound);

        //temp
        Debug.LogWarning("UniconTempCodeisRemaining");
        currPuzzle++;
        LoadScene(1);
        //yield return new WaitUntil(() => FindFirstObjectByType<something>() != null);
        

        yield return null;
    }
    public void Inv_GameOver(string reason)
    {
        print("Game Over");
        LoadingMotion();
        StartCoroutine(Inv_GameOverScene(reason));
    }
    IEnumerator Inv_GameOverScene(string reason)
    {
        inv_PlayerCTRL.gameObject.SetActive(false);
        GameObject _gameOverPanel = Instantiate(gameOverPanel, GameObject.Find("Canvas").transform);
        _gameOverPanel.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = reason;
        _gameOverPanel.transform.Find("ReplayButton").GetComponent<Button>().onClick.AddListener(()=>RestartGame());
        _gameOverPanel.SetActive(true);
        print(_gameOverPanel);
        yield return null;
    }
    public void RestartGame()
    {
        ResetAudioAfterGameOver(false);
        SaveManager.ResetAllSaveData();
        inv_Scene_ID="";
        per_Scene_ID="";
        return_Inv_Scene_ID="";
        invSceneLastPos=null;
        autoInteractOnReturntoInv=null;
        currScene = "Start";
        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }

    public void ReturnToStartScene()
    {
        if (isReturningToStartScene)
        {
            return;
        }

        isReturningToStartScene = true;
        StartCoroutine(ReturnToStartSceneCoroutine());
    }

    IEnumerator ReturnToStartSceneCoroutine()
    {
        yield return StartCoroutine(ExitSceneCoroutine(true));

        // This object also owns the persistent SaveManager and audio sources. Remove it
        // before showing the start screen so a later game starts with fresh managers.
        ResetAudioAfterGameOver(false);
        bgmAudioSource.Stop();
        bgmAudioSource.resource = null;
        lastBGMId = null;
        currScene = "Start";

        Destroy(gameObject);
        SceneManager.LoadScene("StartScene");
    }

    public void PlayBGM(string id, float maximumDuration = -1f)
    {
        if (audioLockedForJumpScare && id != JumpScareSoundId)
        {
            return;
        }

        switch (id)
        {
            case JumpScareSoundId:
                PlayJumpScare(maximumDuration);
                return;
            case GameOverSoundId:
                PlayLoopingEffect(gameOverSound);
                return;
            case LaughterSoundId:
                PlayLaughterLoop();
                return;
            case SoulPlaceSoundId:
                PlayOneShot(soulPlaceSound);
                return;
            case EyeSoundId:
                PlayOneShot(eyeSound);
                return;
            case BigEyeSoundId:
                PlayOneShot(bigEyeSound);
                return;
            case GlitchSoundId:
                PlayOneShot(glitchSound);
                return;
        }

        AudioClip clip=null;
        print(id);
        switch(id){
            case IntroBGMId:
                clip = introBGM;
                break;
            case MainBGMId:
                clip = map1_MainBGM;
                break;
            case DreamBGMId:
                clip = dream_MainBGM;
                break;
            case "Guard":
                clip = guardBGM;
                break;
        }
        if(clip==null){
            Debug.LogError("no clip for sound id: " + id);
            return;
        }

        StopOngoingEffects();
        lastBGMId = id;
        bgmAudioSource.loop = true;
        bgmAudioSource.volume = 1f;
        if(clip == bgmAudioSource.resource && bgmAudioSource.isPlaying) return;
        bgmAudioSource.resource = clip;
        bgmAudioSource.Play();
    }

    private void PlayInvestigationBGM(string investigationId)
    {
        switch (investigationId)
        {
            case "Map1_Intro":
                PlayBGM(IntroBGMId);
                break;
            case "Map_Dream":
            case "Dream":
                PlayBGM(DreamBGMId);
                break;
            default:
                // Map1, the part between the intro and village, the village maps,
                // Map_House, and the post-dream return all use the main track.
                PlayBGM(MainBGMId);
                break;
        }
    }

    private static bool IsDreamPersuasion(string persuasionId)
    {
        return !string.IsNullOrEmpty(persuasionId) &&
               (persuasionId.StartsWith("Map_Dream", StringComparison.Ordinal) ||
                persuasionId.StartsWith("Dream", StringComparison.Ordinal));
    }

    public void ResetAudioAfterGameOver(bool resumeBGM = true)
    {
        audioLockedForJumpScare = false;

        if (jumpScareStopCoroutine != null)
        {
            StopCoroutine(jumpScareStopCoroutine);
            jumpScareStopCoroutine = null;
        }

        StopOngoingEffects();
        audioSource.Stop();
        audioSource.resource = null;
        audioSource.volume = soundEffectVolume;

        if (resumeBGM && !string.IsNullOrEmpty(lastBGMId))
        {
            string bgmId = lastBGMId;
            lastBGMId = null;
            PlayBGM(bgmId);
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("ChiefManager sound effect clip is missing.", this);
            return;
        }

        audioSource.PlayOneShot(clip, soundEffectVolume);
    }

    private void PlayLoopingEffect(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("ChiefManager looping sound effect clip is missing.", this);
            return;
        }

        StopLaughterLoop();
        if (loopingEffectAudioSource.resource == clip && loopingEffectAudioSource.isPlaying)
        {
            return;
        }

        loopingEffectAudioSource.Stop();
        loopingEffectAudioSource.resource = clip;
        loopingEffectAudioSource.loop = true;
        loopingEffectAudioSource.volume = soundEffectVolume;
        loopingEffectAudioSource.Play();
    }

    private void PlayLaughterLoop()
    {
        if (laughterCoroutine != null)
        {
            return;
        }

        AudioClip[] availableLaughterSounds = laughterSounds == null
            ? Array.Empty<AudioClip>()
            : laughterSounds.Where(clip => clip != null).ToArray();
        if (availableLaughterSounds.Length == 0)
        {
            Debug.LogWarning("ChiefManager laughter clips are missing.", this);
            return;
        }

        loopingEffectAudioSource.Stop();
        loopingEffectAudioSource.loop = false;
        laughterCoroutine = StartCoroutine(PlayLaughterLoopCore(availableLaughterSounds));
    }

    private IEnumerator PlayLaughterLoopCore(AudioClip[] clips)
    {
        int clipIndex = 0;
        while (!audioLockedForJumpScare)
        {
            AudioClip clip = clips[clipIndex];
            loopingEffectAudioSource.resource = clip;
            loopingEffectAudioSource.volume = soundEffectVolume;
            loopingEffectAudioSource.Play();
            yield return new WaitForSeconds(clip.length);
            clipIndex = (clipIndex + 1) % clips.Length;
        }

        laughterCoroutine = null;
    }

    private void StopLaughterLoop()
    {
        if (laughterCoroutine != null)
        {
            StopCoroutine(laughterCoroutine);
            laughterCoroutine = null;
        }
    }

    private void StopOngoingEffects()
    {
        StopLaughterLoop();
        if (loopingEffectAudioSource != null)
        {
            loopingEffectAudioSource.Stop();
            loopingEffectAudioSource.resource = null;
        }
    }

    private void PlayJumpScare(float maximumDuration)
    {
        if (jumpScareSound == null)
        {
            Debug.LogWarning("ChiefManager jump-scare clip is missing.", this);
            return;
        }

        StopLaughterLoop();
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        foreach (AudioSource source in allAudioSources)
        {
            source.Stop();
        }

        audioLockedForJumpScare = true;
        audioSource.resource = jumpScareSound;
        audioSource.loop = false;
        audioSource.volume = jumpScareVolume;
        audioSource.Play();

        float requestedDuration = maximumDuration > 0f ? maximumDuration : jumpScareDuration;
        float playbackDuration = requestedDuration > 0f
            ? Mathf.Min(requestedDuration, jumpScareSound.length)
            : jumpScareSound.length;
        jumpScareStopCoroutine = StartCoroutine(StopJumpScareAfter(playbackDuration));
    }

    private IEnumerator StopJumpScareAfter(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        audioSource.Stop();
        audioSource.resource = null;
        audioSource.volume = soundEffectVolume;
        jumpScareStopCoroutine = null;
    }

    private static void ConfigureAudioSource(AudioSource source, bool loop)
    {
        if (source == null)
        {
            return;
        }

        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
    }

}
