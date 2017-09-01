using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BalloonsGame
{
    public class CollisionDestroy : MonoBehaviour 
    {
        public event Action OnColliderDestroyed;

        void OnTriggerEnter2D(Collider2D col)
        {
            Destroy(col.gameObject);
            OnColliderDestroyed();
        }

    }
}