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
using System.Threading.Tasks;

namespace Investigation
{
    public class Utility : MonoBehaviour
    {
        private Color NewColorwithSetOpacity(Color color, float value)
        {
            return new Color(color.r, color.g, color.b, value);
        }
        private void SetOpacity(object target, float value)
        {
            if(target is GameObject target_g)
            {
                if(target_g.TryGetComponent<SpriteRenderer>(out SpriteRenderer temp_s)) target = temp_s;
                else if(target_g.TryGetComponent<Image>(out Image temp_i)) target = temp_i;
                else return;
            }
            if(target is SpriteRenderer target_s)
            {
                target_s.color = NewColorwithSetOpacity(target_s.color, value);
            }
            else if(target is Image target_i)
            {
                target_i.color = NewColorwithSetOpacity(target_i.color, value);
            }
        }
        private Color GetColor(object target)
        {
            if(target is GameObject target_g)
            {
                if(target_g.TryGetComponent<SpriteRenderer>(out SpriteRenderer temp_s)) target = temp_s;
                else if(target_g.TryGetComponent<Image>(out Image temp_i)) target = temp_i;
                else {
                    Debug.LogError("Couldn't Get Color from: "+target_g.name);
                    return new Color();
                }
            }
            if(target is SpriteRenderer target_s)
            {
                return target_s.color;
            }
            else if(target is Image target_i)
            {
                return target_i.color;
            }
            else{
                    Debug.LogError("Couldn't Get Color");
                    return new Color();
            }
        }
        /// <summary>
        /// Fade In/Out an Object
        /// </summary>
        /// <param name="fadeIn">true: fade in / false: fade out</param>
        /// <param name="doDestroy">true(default): destory object after fading <para>false: SetActive(false) after fading</param>
        private Dictionary<GameObject, List<Coroutine>> runningFadingCoroutines = new Dictionary<GameObject, List<Coroutine>>();

        protected Coroutine FadeObject(GameObject targetObj, bool fadeIn, float delay, float fadingTime, bool doDestroy = true, float lowOpacity = 0f, float highOpacity = 1f)
        {
            StopFading(targetObj, GetColor(targetObj).a);

            runningFadingCoroutines[targetObj] = new List<Coroutine>();

            Coroutine coroutine = StartFadeCoroutine(targetObj, targetObj, fadeIn, delay, fadingTime, doDestroy, lowOpacity, highOpacity);
            CheckAllChildrenToFade(targetObj, targetObj, fadeIn, delay, fadingTime, doDestroy, lowOpacity, highOpacity);
            return coroutine;
        }

        private void CheckAllChildrenToFade(GameObject parent, GameObject headObj, bool fadeIn, float delay, float fadingTime, bool doDestroy, float lowOpacity, float highOpacity)
        {
            foreach (Transform child in parent.transform)
            {
                GameObject childObj = child.gameObject;

                StartFadeCoroutine(childObj, headObj, fadeIn, delay, fadingTime, doDestroy, lowOpacity, highOpacity);
                CheckAllChildrenToFade(childObj, headObj, fadeIn, delay, fadingTime, doDestroy, lowOpacity, highOpacity);
            }
        }

        private Coroutine StartFadeCoroutine(GameObject targetObj, GameObject headObj, bool fadeIn, float delay, float fadingTime, bool doDestroy, float lowOpacity, float highOpacity)
        {
            Coroutine coroutine = StartCoroutine(FadeSlowly(targetObj, headObj, fadeIn, delay, fadingTime, doDestroy, lowOpacity, highOpacity));

            runningFadingCoroutines[headObj].Add(coroutine);
            return coroutine;
        }

        private IEnumerator FadeSlowly(GameObject targetObj, GameObject headObj, bool fadeIn, float delay, float fadingTime, bool doDestroy = true, float lowOpacity = 0f, float highOpacity = 1f)
        {
            yield return new WaitForSeconds(delay);

            float elapsed = 0f;
            while (elapsed < fadingTime)
            {
                float ratio = elapsed/fadingTime;
                if(!fadeIn) ratio = 1-ratio;

                float value = lowOpacity+ratio*(highOpacity-lowOpacity);

                SetOpacity(targetObj, value);

                elapsed += Time.deltaTime;
                yield return null;
            }
            if (!fadeIn && targetObj == headObj)
            {
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
        protected void StopFading(GameObject headObj, float originalOpacity)
        {
            if (!runningFadingCoroutines.TryGetValue(headObj, out List<Coroutine> coroutines))
                return;

            foreach (Coroutine coroutine in coroutines)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }

            runningFadingCoroutines.Remove(headObj);


        }
        private void ResetOpacity(GameObject obj, float originalOpacity)
        {
            SetOpacity(obj, originalOpacity);
            foreach(Transform child in obj.transform)
            {
                ResetOpacity(child.gameObject, originalOpacity);
            }
        }

        public void SetSpriteImage<T>(GameObject obj, string imagePath, List<AsyncOperationHandle<Sprite>> handles=null) where T : Component
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
                    Image curr = obj.GetComponent<Image>();
                    if (curr == null) return;

                    curr.sprite = sprite;
                    Color original = curr.color;
                    curr.color = new Color(original.r, original.g, original.b, 1);
                }
                else if (typeof(T) == typeof(SpriteRenderer))
                {
                    SpriteRenderer curr = obj.GetComponent<SpriteRenderer>();
                    if (curr == null) return;

                    curr.sprite = sprite;
                    Color original = curr.color;
                    curr.color = new Color(original.r, original.g, original.b, 1f);
                }

                if(handles != null) handles.Add(handle);
                else print("handle not assigned");
            };
        }
        public void ClearHandles(List<AsyncOperationHandle<Sprite>> handles)
        {
            foreach (var handle in handles)
            {
                if(handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            handles.Clear();
        }
    }
}