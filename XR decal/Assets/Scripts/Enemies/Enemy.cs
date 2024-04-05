using System;
using GameMaster;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemies
{
    public class Enemy : MonoBehaviour
    {
        [FormerlySerializedAs("MaxHealth")] public float maxHealth;
        [FormerlySerializedAs("Health")] public float health;
        [FormerlySerializedAs("Speed")] public float speed;
        [FormerlySerializedAs("ID")] public int id;
        public int nodeIndex;
        public void Init()
        {
            health = maxHealth;
            transform.position = GameManager.nodePositons[0];
            nodeIndex = 0;
        }

        private void Update()
        {
            // Debug.Log("hello");
        }
    }
}
