using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorCollisionScript : MonoBehaviour
{
    public Material errorMaterial;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "cube1")
        {
            this.gameObject.GetComponent<MeshRenderer>().material = errorMaterial;
            BrickPlacementManager.collide = true;
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "cube1")
        {
            this.gameObject.GetComponent<MeshRenderer>().material = errorMaterial;
            BrickPlacementManager.collide = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "cube1")
        {
            BrickPlacementManager.collide = false;
        }
    }

}
