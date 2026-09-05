using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Trashcan: Inv_InteractionObj
    {
        private Inv_PlayerCTRL playerCTRL;

        override protected void Starter()
        {
            playerCTRL = GameObject.FindFirstObjectByType<Inv_PlayerCTRL>();
        }

        override public void variation(List<string> parameters = null)
        {
            if (parameters[0] == "ClothObtained")
            {
                state = 1;
                bool haveMetWitch =
                    saveManager.TryLoadProgress("MetWitchMother", out object result)
                    && result is bool metWitch
                    && metWitch;

                playerCTRL.Think("어린 여자아이의 것으로 보이는 옷을 얻었다. 상태는 깨끗해 보인다.");

                if (haveMetWitch)
                {
                    playerCTRL.Think("아까 그 여자의 딸의 것일까? 이걸 입고 다시 가보자. (나에게 아이템을 드래그해서 입을 수 있다.)");
                }
                else
                {
                    playerCTRL.Think("(나에게 아이템을 드래그해서 입을 수 있다.)");
                }
            }
            base.variation(parameters);
        }
    }
}
