using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Investigation
{
public class InteractionObj_Map1_Note_On_Ground: InteractionObj
    {
        override public void CheckState()
        {
            base.CheckState();
            gameManager.TryGetProgress("notePossessed", out object notePossessed);
            if(notePossessed != null && ((bool)notePossessed)==true)
            {
                Destroy(gameObject);
            }
        }
        override public void variation(List<string> parameters = null)
        {
            if (parameters[0] == "PickedUp")
            {
                bool havePen = false;
                if(gameManager.TryGetProgress("penPossessed", out object result))
                {
                    if((bool)result) havePen = true;
                }
                if (havePen)
                {
                    interactionManager.Effects(
                        new JObject
                        {
                            ["type"]="thought",
                            ["thought"]="노트와 펜을 모두 얻었어! 이제 그림도 그릴 수 있고, 메모도 할 수 있겠다."
                        }
                    );
                }
                else
                {
                    interactionManager.Effects(
                        new JObject
                        {
                            ["type"]="thought",
                            ["thought"]="노트를 얻었어! 펜은 어디서 구해야 하지?"
                        }
                    );
                }
            }
            base.variation(parameters);
        }
    }
}