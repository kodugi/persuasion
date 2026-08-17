using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
public class Inv_Obj_Staring_People: Utility
    {
        public GameObject player;
        public GameObject house_gather;
        Dictionary<GameObject, Vector3> eyes = new Dictionary<GameObject, Vector3>();
        List<GameObject> people = new List<GameObject>();
        [SerializeField] float maxDistance = 0.1f;
        bool doRun = false;
        float moveSpeed = 3f;

        private static Inv_Obj_Staring_People instance;

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
            CheckEye(gameObject);
        }
        void CheckEye(GameObject parent)
        {
            foreach(Transform child in parent.transform)
            {
                if (child.gameObject.CompareTag("Inv_Staring_People_Eye"))
                {
                    eyes[child.gameObject] = child.position;
                }
                else if (child.gameObject.CompareTag("Inv_Staring_People"))
                {
                    people.Add(child.gameObject);
                }

                CheckEye(child.gameObject);
            }
        }
        void Update()
        {
            foreach(var pair in eyes)
            {
                GameObject eye = pair.Key;
                if(eye == null) continue;
                Vector3 originalPos = pair.Value;
                eye.transform.position = (player.transform.position-originalPos).normalized*maxDistance+originalPos;
            }
            if (doRun)
            {
                foreach(GameObject person in people)
                {
                    person.transform.position = Vector3.MoveTowards(person.transform.position,house_gather.transform.position,moveSpeed * Time.deltaTime);
                    if(Vector3.Distance(person.transform.position, house_gather.transform.position) < 0.1f)
                    {
                        Destroy(person);
                        people.Remove(person);
                    }
                }
            }
        }
        public void StartRunning()
        {
            doRun = true;
        }
    }
}