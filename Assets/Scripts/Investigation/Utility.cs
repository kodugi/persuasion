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

        public void SetSpriteImage<T>(GameObject obj, string imagePath, List<AsyncOperationHandle<Sprite>> handles) where T : Component
        {
            Addressables.LoadAssetAsync<Sprite>(imagePath).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Sprite sprite = handle.Result;
                    if (typeof(T) == typeof(Image))
                    {
                        Image curr = ((Image)(object)obj.GetComponent<T>());
                        curr.sprite = sprite;
                        Color original = curr.color;
                        curr.color = new Color(original.r,original.g,original.b,1);
                    }
                    else if (typeof(T) == typeof(SpriteRenderer))
                    {
                        SpriteRenderer curr = ((SpriteRenderer)(object)obj.GetComponent<T>());
                        curr.sprite = sprite;
                        Color original = curr.color;
                        curr.color = new Color(original.r,original.g,original.b,1);
                    }
                    handles.Add(handle);
                }
            };
        }
    }
}