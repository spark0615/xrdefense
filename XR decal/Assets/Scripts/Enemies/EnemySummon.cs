using UnityEngine;
using UnityEngine.Serialization;

namespace Enemies
{
    [CreateAssetMenu(fileName = "New EnemySummon", menuName = "Create EnemySummon")]
    public class EnemySummon : ScriptableObject
    {
        [FormerlySerializedAs("EnemyPrefab")] public GameObject enemyPrefab;
        [FormerlySerializedAs("EnemyID")] public int enemyID;
    }
}