using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeteorHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if(this.transform.position.y < 0)
        {
            this.GetComponent<Rigidbody>().isKinematic = true;
            this.GetComponent<ParticleSystem>().Stop();
            Invoke("remove", 3);
        }
    }

    private void remove()
    {
        Destroy(this.gameObject);
    }
}
