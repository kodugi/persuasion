using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Guard1: Inv_InteractionObj
    {
        [SerializeField] private float moveSpeed = 3f;
        private Inv_Interact interactManager;
        private bool chasing = false;
        private Transform player;
        private AsyncOperationHandle<RuntimeAnimatorController> animatorHandle;
        private Animator animator;
        protected override void Starter()
        {
            interactManager = GameObject.FindFirstObjectByType<Inv_Interact>();
            player = GameObject.FindFirstObjectByType<Inv_PlayerCTRL>().transform;

            StartCoroutine(LoadAnim());
        }

        IEnumerator LoadAnim()
        {
            var handle =
                Addressables.LoadAssetAsync<RuntimeAnimatorController>("Map1_Guard_Animator");

            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                animator = gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = handle.Result;
                animatorHandle = handle;
            }
            else
            {
                Debug.LogError("Failed to load Animator Controller");
            }
        }
        void Update()
        {
            if(chasing) {
                transform.position = Vector3.MoveTowards(transform.position,player.position,moveSpeed * Time.deltaTime);
                player.gameObject.GetComponent<Inv_PlayerCTRL>().CanPlayerMove(false);

            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player") && chasing) {
                chasing = false;
                animator.SetBool("Running", false);
                interactManager.ForceInteraction(obj_name);

            }
        }
        override public void variation(List<string> parameters)
        {
            foreach(string parameter in parameters)
            {
                switch (parameter)
                {
                    case "Chase":
                        if(state ==0) {
                            chasing = true;
                            animator.SetBool("Running", true);
                        }
                        break;
                    case "Caught":
                        state = 1;
                        break;
                    case "Met":
                        state = 2;
                        break;
                    case "Pull":
                        animator.enabled = false;
                        FadeSwitch(0,4, 0, 0f);
                        FadeObject(player.gameObject, false, 0, 0f,false);
                        if (animatorHandle.IsValid())
                        {
                            Addressables.Release(animatorHandle);
                        }
                        break;
                    case "Throw":
                        FadeSwitch(4,3, 0, 0f);
                        break;
                }
            }
            base.variation();
        }
    }
}