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
        [SerializeField] float maxDistance = 0.1f;
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
        }        
        void Start()
        {
            CheckPeople(gameObject);
        }
        void CheckPeople(GameObject parent)
        {
            foreach(Transform child in parent.transform)
            {
                if (child.gameObject.CompareTag("Inv_Staring_People"))
                {
                    people[child.gameObject] = default;
                }

                CheckEye(child.gameObject, child.gameObject);
            }
        }
        void CheckEye(GameObject parent, GameObject idx)
        {
            foreach(Transform child in parent.transform)
            {
                if (child.gameObject.CompareTag("Inv_Staring_People_Eye"))
                {
                    people[idx] = (child.gameObject, child.gameObject.transform.position);
                }
                else CheckEye(child.gameObject, idx);
            }
        }
        void Update()
        {
            foreach(var pair in people)
            {
                GameObject person = pair.Key;
                if(person == null) continue;
                GameObject eye = pair.Value.eye;
                Vector3 originalPos = pair.Value.originalPos;
                eye.transform.position = (player.transform.position-originalPos).normalized*maxDistance+originalPos+person.transform.position;
                if (doRun)
                {
                    person.transform.position = Vector3.MoveTowards(person.transform.position,house_gather.transform.position,moveSpeed * Time.deltaTime);
                    if(Vector3.Distance(person.transform.position, house_gather.transform.position) < 0.1f)
                    {
                        if(person == closestPerson)
                        {
                            manager.ChangeCamera(player.transform, 1);
                        }
                        Destroy(person);
                    }
                }
            }
        }
        public void StartRunning()
        {
            float minDistance = float.MaxValue;
            foreach(var pair in people)
            {
                GameObject person = pair.Key;
                float distance= Vector3.Distance(person.transform.position, player.transform.position);
                if(distance < minDistance)
                {
                    minDistance = distance;
                    closestPerson = person;
                }
            }
            manager.ChangeCamera(closestPerson.transform, 0.5f);
            doRun = true;
        }
    }
}