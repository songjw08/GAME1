using Akila.FPSFramework.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Managers/Spwan Manager")]
    public class SpawnManager : MonoBehaviour
    {
        public List<SpwanableObject> spwanableObjects = new List<SpwanableObject>();
        public float spawnRadius = 5;
        public float respawnDelay = 5;

        [Separator]
        public List<SpwanSide> sides;

        public static SpawnManager Instance;

        public bool isActive { get; set; } = true;
        public UnityEvent<GameObject> onPlayerSpawn { get; set; } = new UnityEvent<GameObject>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public async void SpawnActor(string actorObjName, float delay)
        {
            float time = 0;

            while (time < delay)
            {
                time += Time.deltaTime;

                await Task.Yield();
            }

            if (Application.isPlaying == false) return;

            GameObject obj = spwanableObjects.Find(x => x.name == actorObjName).obj;

            SpawnActor(obj);
        }

        public void SpawnActor(string actorObjName)
        {
            GameObject obj = spwanableObjects.Find(x => x.name == actorObjName).obj;

            SpawnActor(obj);
        }

        public GameObject SpawnActor(GameObject actorObj)
        {
            onPlayerSpawn?.Invoke(actorObj);

            if(!isActive) return null;

            Actor selfActor = actorObj.GetComponent<Actor>();
            Vector3 actorPosition = GetPlayerPosition(selfActor.teamId);
            Quaternion actorRotation = GetPlayerRotation(selfActor.teamId);

            GameObject newActorObject = Instantiate(actorObj, actorPosition, actorRotation);
            Actor newSelfActor = newActorObject.GetComponent<Actor>();

            newSelfActor.kills = selfActor.kills;
            newSelfActor.deaths = selfActor.deaths;

            Vector3 position = GetPlayerPosition(selfActor.teamId);
            Quaternion rotation = GetPlayerRotation(selfActor.teamId);

            newActorObject.transform.SetPositionAndRotation(position, rotation);

            return newActorObject;
        }

        public Transform GetPlayerSpawnPoint(int sideId)
        {
            int pointIndex = Random.Range(0, sides[sideId].points.Length);

            return sides[sideId].points[pointIndex];
        }

        public Vector3 GetPlayerPosition(int sideId)
        {
            Vector3 addedPosition = Random.insideUnitCircle * spawnRadius;

            addedPosition.z = addedPosition.y;

            addedPosition.y = 0;

            return GetPlayerSpawnPoint(sideId).position + addedPosition;
        }

        public Quaternion GetPlayerRotation(int sideId)
        {
            return GetPlayerSpawnPoint(sideId).rotation;
        }

        private void OnDrawGizmos()
        {
            foreach (SpwanSide point in sides)
            {
                foreach (Transform transform in point.points)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(transform.position, spawnRadius * transform.lossyScale.magnitude);
                }
            }
        }

        [ContextMenu("Setup/Network Components")]
        private void SetupNetworkComponents()
        {
#if UNITY_EDITOR
            FPSFrameworkEditor.InvokeConvertMethod("ConvertSpawnManager", this, new object[] { this });
#endif
        }

        [System.Serializable]
        public class SpwanSide
        {
            public Transform[] points;
        }

        [System.Serializable]
        public class SpwanableObject
        {
            public string name;
            public GameObject obj;
        }
    }
}