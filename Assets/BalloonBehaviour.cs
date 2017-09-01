using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BalloonsGame
{
    public class BalloonBehaviour : MonoBehaviour 
    {
        public float speed;

    	void FixedUpdate() 
        {
            transform.position += new Vector3(0, speed);
    	}
    }
}