using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    public struct MathUtilities
    {
        public static Vector3 RandomVector3(Vector3 refrecne)
        {
            return new Vector3(Random.Range(-refrecne.x, refrecne.x), Random.Range(-refrecne.y, refrecne.y), Random.Range(-refrecne.z, refrecne.z));
        }
    }
}