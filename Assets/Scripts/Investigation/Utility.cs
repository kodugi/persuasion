using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Investigation;

namespace Investigation
{
    public class Utility : MonoBehaviour
    {
        /// <summary>
        /// Fade In/Out an Object
        /// </summary>
        /// <param name="fadeIn">true: fade in / false: fade out</param>
        /// <param name="doDestroy">true(default): destory object after fading <para>false: SetActive(false) after fading</param>
        protected void FadeObject(GameObject targetObj, bool fadeIn, float delay, float fadingTime, bool doDestroy=true)
        {
            StartCoroutine(FadeSlowly(targetObj, doDestroy, delay, fadingTime, fadeIn));
        }
        protected IEnumerator FadeSlowly(GameObject targetObj, bool fadeIn, float delay, float fadingTime, bool doDestroy=true)
        {
            yield return new WaitForSeconds(delay);
            //Fade Effect
            if (doDestroy)
            {
                Destroy(targetObj);
            }
            else
            {
                targetObj.SetActive(false);
            }
        }
    }
}