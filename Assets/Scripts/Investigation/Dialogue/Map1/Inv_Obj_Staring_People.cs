using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
public class Inv_Obj_Staring_People: Utility
    {
        public GameObject player;
        public Inv_GameManager manager;
        public GameObject house_gather;
        Dictionary<GameObject, (GameObject eye, Vector3 originalPos)> people = new Dictionary<GameObject, (GameObject eye, Vector3 originalPos)>();
        [SerializeField] float maxDistance = 0.01f;
        bool doRun = false;
        float moveSpeed = 3f;

        private static Inv_Obj_Staring_People instance;

        GameObject closestPerson;

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            CheckPeople(gameObject);
        }        
        void Start()
        {
        }
        void CheckPeople(GameObject parent)
        {
            foreach(Transform child in parent.transform)
            {
                // PeopleStaring is an Addressable prefab. Do not rely on custom
                // project tags here: missing tags throw in a player build and
                // abort this object's initialization.
                if (child.TryGetComponent<Animator>(out _))
                {
                    people[child.gameObject] = default;
                    CheckEye(child.gameObject, child.gameObject);
                }
            }
        }
        void CheckEye(GameObject parent, GameObject idx)
        {
            foreach(Transform child in parent.transform)
            {
                if (child.name == "Eye")
                {
                    people[idx] = (child.gameObject, child.localPosition);
                }
                else CheckEye(child.gameObject, idx);
            }
        }
        void Update()
        {
            int cnt = 0;
            foreach(var pair in people)
            {
                GameObject person = pair.Key;
                if(person == null) continue;
                cnt++;
                GameObject eye = pair.Value.eye;
                Vector3 originalPos = pair.Value.originalPos;
                eye.transform.localPosition = (player.transform.position-(originalPos+person.transform.position)).normalized*maxDistance+originalPos;//  +person.transform.position;
                if (doRun)
                {
                    person.transform.position = Vector3.MoveTowards(person.transform.position,house_gather.transform.position,moveSpeed * Time.deltaTime);
                    if(Vector3.Distance(person.transform.position, house_gather.transform.position) < 0.1f)
                    {
                        if(person == closestPerson)
                        {
                            manager.ChangeCamera(house_gather.transform, 0.1f);
                        }
                        Destroy(person);
                    }
                    player.GetComponent<Inv_PlayerCTRL>().CanPlayerMove(false);
                }
            }
            if(cnt <= 0)
            {
                manager.ChangeCamera(player.transform, 0.1f);
                player.GetComponent<Inv_PlayerCTRL>().CanPlayerMove(true);
                Destroy(gameObject);
            }
        }
        public void StartRunning()
        {
            float minDistance = float.MaxValue;
            foreach(var pair in people)
            {
                GameObject person = pair.Key;
                person.GetComponent<Animator>().SetBool("isWalking", true);
                foreach(Transform child in person.transform)
                {
                    child.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                }
                float distance= Vector3.Distance(person.transform.position, player.transform.position);
                if(distance < minDistance)
                {
                    minDistance = distance;
                    closestPerson = person;
                }
            }
            manager.ChangeCamera(closestPerson.transform, 0.1f);
            doRun = true;
        }
    }
}
