using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    public sealed class ObjectsPool : MonoBehaviour
    {
        [SerializeField]
        private GameObject prefab;

        [SerializeField]
        private int poolSize = 200;

        [SerializeField]
        private string poolName = "Objects Pool";

        private readonly Queue<GameObject> pool = new Queue<GameObject>();

        private void Start()
        {
            var parentObject = new GameObject(poolName);

            for (var i = 0; i < poolSize; ++i)
            {
                var obj = Instantiate(prefab);
                obj.transform.parent = parentObject.transform;
                obj.SetActive(false);

                pool.Enqueue(obj);
            }
        }

        public GameObject Instantiate(Vector3 position)
        {
            var newObject = pool.Dequeue();

            newObject.SetActive(true);
            newObject.transform.position = position;

            pool.Enqueue(newObject);

            return newObject;
        }
    }
}