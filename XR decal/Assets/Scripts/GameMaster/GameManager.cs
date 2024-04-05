using System.Collections;
using System.Collections.Generic;
using Enemies;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace GameMaster
{
    public struct MoveEnemiesJob : IJobParallelForTransform
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector3> nodePositions;
        [NativeDisableParallelForRestriction]
        public NativeArray<float> enemySpeeds;
        [NativeDisableParallelForRestriction]
        public NativeArray<int> nodeIndex;
        public float deltaTime;

        public void Execute(int i, TransformAccess transform)
        {
            if (nodeIndex[i] < nodePositions.Length)
            {
                Vector3 positionToMoveTo = nodePositions[nodeIndex[i]];
                
                transform.position = Vector3.MoveTowards(transform.position, positionToMoveTo, enemySpeeds[i] * deltaTime);
                
                if (transform.position == positionToMoveTo)
                {
                    nodeIndex[i] += 1;
                }    
                Vector3 direction = (transform.position - positionToMoveTo).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                float maxDegreesPerSecond = 360f / 1.0f;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegreesPerSecond * deltaTime);
            }
            
            
        }
    }
    
    public class GameManager : MonoBehaviour
    {
        public bool endGame;
        private static Queue<int> _enemyIDtoSummon;
        private static Queue<Enemy> _enemyToRemove;
        
        
        public static Vector3[] nodePositons;
        public Transform nodeParent;
        
        
        private void Start()
        {
            _enemyIDtoSummon = new Queue<int>();
            _enemyToRemove = new Queue<Enemy>();
            EntitySummoner.Init();

            nodePositons = new Vector3[nodeParent.childCount];
            for (int i = 0; i < nodePositons.Length; i++)
            {
                nodePositons[i] = nodeParent.GetChild(i).position;
            }
            
            StartCoroutine(GameLoop());
            InvokeRepeating(nameof(SummonTest), 0f, 1f);
            // InvokeRepeating(nameof(RemoveTest), 0f, 2f);
        }

        void SummonTest()
        {
            // Debug.Log("SummonTest: summoning enemy");
            EnqueueEnemyIDToSummon(2);
        }

        // void RemoveTest()
        // {
        //     if (EntitySummoner.enemiesInGame.Count > 0)
        //     {
        //         EntitySummoner.RemoveEnemy(EntitySummoner.enemiesInGame[0]);
        //     }
        // }
        IEnumerator GameLoop()
        {
            Debug.Log("GameLoop!");
            while (!endGame)
            {
                
                //spawn enemies
                while (_enemyIDtoSummon.Count > 0)
                {
                    
                    EntitySummoner.SummonEnemy(_enemyIDtoSummon.Dequeue());
                }

                //move enemies
                NativeArray<Vector3> nodeToUse = new NativeArray<Vector3>(nodePositons, Allocator.TempJob);
                NativeArray<int> nodeIndices = new NativeArray<int>(EntitySummoner.enemiesInGame.Count, Allocator.TempJob);
                NativeArray<float> enemySpeeds = new NativeArray<float>(EntitySummoner.enemiesInGame.Count, Allocator.TempJob);
                TransformAccessArray enemyAccess = new TransformAccessArray(EntitySummoner.enemiesInGameTransforms.ToArray(), 2);

                for (int i = 0; i < EntitySummoner.enemiesInGame.Count; i++)
                {
                    enemySpeeds[i] = EntitySummoner.enemiesInGame[i].speed;
                    nodeIndices[i] = EntitySummoner.enemiesInGame[i].nodeIndex;
                }

                MoveEnemiesJob moveJob = new MoveEnemiesJob
                {
                    nodePositions = nodeToUse,
                    enemySpeeds = enemySpeeds,
                    nodeIndex = nodeIndices,
                    deltaTime = Time.deltaTime
                };

                JobHandle moveJobHandle = moveJob.Schedule(enemyAccess);
                moveJobHandle.Complete();

                
                for (int i = 0; i < EntitySummoner.enemiesInGame.Count; i++)
                {
                    
                    EntitySummoner.enemiesInGame[i].nodeIndex = nodeIndices[i];
                    
                    if (EntitySummoner.enemiesInGame[i].nodeIndex == nodePositons.Length)
                    {
                        EnqueueEnemyRemove(EntitySummoner.enemiesInGame[i]);
                    }
                }
                
                
                nodeToUse.Dispose();
                nodeIndices.Dispose();
                enemySpeeds.Dispose();
                enemyAccess.Dispose();
                
                
                //Tick towers
                //Damage Enemy
                
                //remove enemy
                while (_enemyToRemove.Count > 0)
                {
                    EntitySummoner.RemoveEnemy(_enemyToRemove.Dequeue());
                }
                
            
            
                yield return null;
            }
        
        }
        private static void EnqueueEnemyIDToSummon(int id)
        {
            _enemyIDtoSummon.Enqueue(id);
        }

        public static void EnqueueEnemyRemove(Enemy enemy)
        {
            
            _enemyToRemove.Enqueue(enemy);
        }
    }


}

