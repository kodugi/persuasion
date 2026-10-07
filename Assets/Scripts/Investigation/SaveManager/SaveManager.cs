using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Investigation;

public partial class SaveManager : MonoBehaviour
{
    [SerializeField]
    public bool resetOnQuit = true;
    [SerializeField]
    public bool saveWhilePlaying = true;

    private GameManager gameManager;
    private ChiefManager chiefManager;

    public Dictionary<string, object> generalSave =
        new Dictionary<string, object>();

    public static SaveManager Instance { get; private set; }

    /// <summary>
    /// becomes true when (resetOnQuit == true) and (application is quitting)
    /// </summary>
    public static bool IsQuittingAndResetting { get; private set; }

    private string savePath = null;
}
