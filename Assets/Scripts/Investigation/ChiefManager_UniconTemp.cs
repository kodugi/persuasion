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
    List<string> map1PuzzleList = new List<string>(){"Map1_Guard1", "Map1_Writer", "Map1_Granny", "Map1_WitchMother"};
    int currPuzzle = 0;

    void TempSkipCheckerOnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            print("skipping");
            switch (currScene)
            {
                case "Persuasion":
                    print("Developer Option: Skipping the Puzzle");
                    StartInvestigation();
                    break;
                case "Investigation":
                    if(currPuzzle >= map1PuzzleList.Count)
                    {
                        Inv_GameOver("퍼즐 모두 끝냈습니다");
                    }
                    else StartPersuasion(map1PuzzleList[currPuzzle], "");
                    break;
            }
        }
    }

}