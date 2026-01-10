using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShadowCopyCollisionDetection : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   void CheckForCollision()
   {
       LayerMask layerMask = LayerMask.GetMask("ShadowRealm");

       int hitCount = Physics.OverlapBoxNonAlloc(transform.position, transform.localScale / 2, new Collider[10], transform.rotation, layerMask);
   }

  
}
