using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Trashcan: Inv_InteractionObj
    {
        override public void variation(List<string> parameters = null)
        {
            if (parameters[0] == "ClothObtained")
            {
                bool haveMetWitch = false;
                if(saveManager.TryLoadProgress("MetWitchMother", out object result))
                {
                    if((bool)result) haveMetWitch = true;
                }
                if (haveMetWitch)
                {
                    interactionManager.Effects(
                        new JObject
                        {
                            ["type"]="thought",
                            ["thought"]="군데군데 불에 그을린 듯한 옷... 마녀로 몰려 죽었다던 아까 그 여자의 딸의 것일까?"
                        }
                    );
                    interactionManager.Effects(
                        new JObject
                        {
                            ["type"]="thought",
                            ["thought"]="이걸 입고 다시 그 여자에게 가보자. (나에게 아이템을 드래그해서 입을 수 있다.)"
                        }
                    );
                }
                else
                {
                    interactionManager.Effects(
                        new JObject
                        {
                            ["type"]="thought",
                            ["thought"]="군데군데 불에 그을린 듯한 옷을 발견했다."
                        }
                    );
                    interactionManager.Effects(
                        new JObject
                        {
                            ["type"]="thought",
                            ["thought"]="(나에게 아이템을 드래그해서 입을 수 있다.)"
                        }
                    );
                }
            }
            base.variation(parameters);
        }
    }
}