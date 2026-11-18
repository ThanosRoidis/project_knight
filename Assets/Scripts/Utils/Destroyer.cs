using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shared
{
    public class Destroyer : MonoBehaviour
    {
        public float delay;

        private float instatiationTime;

        private void Awake()
        {
            instatiationTime = Time.time;
        }

        private void Update()
        {
            if (delay > 0) {
                if (GenericUtils.TimeElapsed(instatiationTime, delay)) {
                    Destroy();
                }
            }
        }


        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
