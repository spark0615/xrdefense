using System.Collections.Generic;
using GameMaster;
using Unity.VisualScripting;
using UnityEngine;

namespace Enemies
{
    public class EntitySummoner : MonoBehaviour
    {
        public static List<Enemy> enemiesInGame;
        public static List<Transform> enemiesInGameTransforms;
        private static Dictionary<int, GameObject> _enemyPrefabs;

        private static Dictionary<int, Queue<Enemy>> _enemyObjectPools;

        private static bool _isInit;

    

        public static void Init()
        {
            if (!_isInit)
            {
                enemiesInGame = new List<Enemy>();
                enemiesInGameTransforms = new List<Transform>();
                _enemyPrefabs = new Dictionary<int, GameObject>();
                _enemyObjectPools = new Dictionary<int, Queue<Enemy>>();
                
                
                EnemySummon[] enemies = Resources.LoadAll<EnemySummon>("Enemies");
                
                foreach (EnemySummon enemy in enemies)
                {
                    Debug.Log(enemy.name + "  " + enemy.enemyID.ToString() + "    " + enemy.enemyPrefab);
                    _enemyPrefabs.Add(enemy.enemyID, enemy.enemyPrefab);
                    _enemyObjectPools.Add(enemy.enemyID, new Queue<Enemy>());
                }
            }
            else
            {
                Debug.Log("EntitySummoner: Already initialized");
            }
        

        }

        public static Enemy SummonEnemy(int enemyID)
        {
        
            if (_enemyPrefabs.ContainsKey(enemyID))
            {
                Queue<Enemy> referencedQueue = _enemyObjectPools[enemyID];
                Enemy summonedEnemy;
                if (referencedQueue.Count > 0)
                {

                    summonedEnemy = referencedQueue.Dequeue();
                    summonedEnemy.Init();
                    summonedEnemy.gameObject.SetActive(true);
                }
                else
                {
                    GameObject newEnemy = Instantiate(_enemyPrefabs[enemyID], GameManager.nodePositons[0], Quaternion.identity);
                    
                    summonedEnemy = newEnemy.GetComponent<Enemy>();
                    
                    summonedEnemy.Init();
                }
                
                enemiesInGameTransforms.Add(summonedEnemy.transform);
                enemiesInGame.Add(summonedEnemy);
                summonedEnemy.id = enemyID;
                return summonedEnemy;
            }
            else
            {
                Debug.Log($"EntitySummoner: Enemy with ID {enemyID} does not exist!");
                return null;
            }
        
        }

        public static void RemoveEnemy(Enemy enemy)
        {
            if (enemy == null)
            {
                // Debug.Log("EntitySummoner.cs : RemoveEnemy : Can't remove null");
                return;
            }
            _enemyObjectPools[enemy.id].Enqueue(enemy);
            enemy.gameObject.SetActive(false);
            enemiesInGameTransforms.Remove(enemy.transform);
            enemiesInGame.Remove(enemy);
            // Debug.Log("EntitySummoner|RemoveEnemy|Removing " + enemy.name + " " + enemy.id );
        }
    
    }
}
