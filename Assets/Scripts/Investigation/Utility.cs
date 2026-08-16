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
        Dictionary<GameObject, Coroutine> runningFadingCoroutine = new Dictionary<GameObject, Coroutine>();
        /// <summary>
        /// Fade In/Out an Object
        /// </summary>
        /// <param name="fadeIn">true: fade in / false: fade out</param>
        /// <param name="doDestroy">true(default): destory object after fading <para>false: SetActive(false) after fading</param>
        protected Coroutine FadeObject(GameObject targetObj, bool fadeIn, float delay, float fadingTime, bool doDestroy=true, float lowOpacity=0f, float highOpacity=1f)
        {
            if (runningFadingCoroutine.ContainsKey(targetObj))
            {
                StopCoroutine(runningFadingCoroutine[targetObj]);
                runningFadingCoroutine.Remove(targetObj);
            }

            Coroutine currCoroutine = StartCoroutine(FadeSlowly(targetObj, fadeIn, delay, fadingTime, doDestroy, lowOpacity, highOpacity));
            
            runningFadingCoroutine[targetObj] = currCoroutine;
            return currCoroutine;
        }
        protected IEnumerator FadeSlowly(GameObject targetObj, bool fadeIn, float delay, float fadingTime, bool doDestroy=true, float lowOpacity=0f, float highOpacity=1f)
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
            if (!fadeIn)
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
        protected void StopFading(GameObject targetObj, float originalOpacity)
        {
            //temp
            Color orgColor = targetObj.GetComponent<Image>().color;
            targetObj.GetComponent<Image>().color = new Color(orgColor.r, orgColor.g, orgColor.b, originalOpacity);
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