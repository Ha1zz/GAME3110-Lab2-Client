using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CubeScript : MonoBehaviour
{

    public string cubeid = ""; 

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Cube ID: " + cubeid);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Cube ID 2: " + cubeid);
    }

    public void SelfDestruct()
    {
        Destroy(this.gameObject);
    }
}
