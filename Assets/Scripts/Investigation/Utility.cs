using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Investigation;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
    public class Utility : MonoBehaviour
    {
        Dictionary<GameObject, Coroutine> runningFadingCoroutine = new Dictionary<GameObject, Coroutine>();
        /// <summary>
        /// Fade In/Out an Object
        /// </summary>
        /// <param name="fadeIn">true: fade in / false: fade out</param>
        /// <param name="doDestroy">true(default): destory object after fading <para>false: SetActive(false) after fading</param>
        protected Coroutine FadeObject(GameObject targetObj, bool fadeIn, float delay, float fadingTime, bool doDestroy=true)
        {
            if (runningFadingCoroutine.ContainsKey(targetObj))
            {
                StopCoroutine(runningFadingCoroutine[targetObj]);
                runningFadingCoroutine.Remove(targetObj);
            }
            Coroutine currCoroutine = StartCoroutine(FadeSlowly(targetObj, doDestroy, delay, fadingTime, fadeIn));
            runningFadingCoroutine[targetObj] = currCoroutine;
            return currCoroutine;
        }
        protected IEnumerator FadeSlowly(GameObject targetObj, bool fadeIn, float delay, float fadingTime, bool doDestroy=true)
        {
            yield return new WaitForSeconds(delay);
            //Fade Effect
            yield return new WaitForSeconds(fadingTime);
            if (doDestroy)
            {
                Destroy(targetObj);
            }
            else
            {
                targetObj.SetActive(false);
            }
        }
        protected void StopFading(GameObject targetObj, float originalOpacity)
        {
            //temp
            Color orgColor = targetObj.GetComponent<Image>().color;
            targetObj.GetComponent<Image>().color = new Color(orgColor.r, orgColor.g, orgColor.b, originalOpacity);
        }

        public void SetSpriteImage<T>(GameObject obj, string imagePath, List<AsyncOperationHandle<Sprite>> handles) where T : Component
        {
            Addressables.LoadAssetAsync<Sprite>(imagePath).Completed += handle =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                    return;

                if (obj == null) // Unity destroyed object
                {
                    Addressables.Release(handle);
                    return;
                }

                Sprite sprite = handle.Result;

                if (typeof(T) == typeof(Image))
                {
                    Image curr = (Image)(object)obj.GetComponent<T>();
                    if (curr == null) return;

                    curr.sprite = sprite;
                    Color original = curr.color;
                    curr.color = new Color(original.r, original.g, original.b, 1);
                }
                else if (typeof(T) == typeof(SpriteRenderer))
                {
                    SpriteRenderer curr = (SpriteRenderer)(object)obj.GetComponent<T>();
                    if (curr == null) return;

                    curr.sprite = sprite;
                    Color original = curr.color;
                    curr.color = new Color(original.r, original.g, original.b, 1);
                }

                handles.Add(handle);
            };
        }
    }
}