using Unity.VisualScripting;
using UnityEngine;

public class CheckPointScript : MonoBehaviour
{
    private string CheckPointName;
    private int CheckPointNumber;
    GamePlay Object;

    private void Start()
    {
        CheckPointName = gameObject.name;

        if (CheckPointName == "CheckPoint1")
            CheckPointNumber = 1;
        else if (CheckPointName == "CheckPoint2")
            CheckPointNumber = 2;
        else if(CheckPointName == "CheckPoint3")
            CheckPointNumber = 3;

        Object = gameObject.GetComponentInParent<GamePlay>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            
            Object.CheckPointHit(CheckPointNumber);
            //Debug.Log("Check Point Number Sent: " + CheckPointNumber);
        }
        else if(other.tag == "AI")
        {
            if (!other.TryGetComponent(out AiScript ai))
                return;

            //int id = ai.AINum;

            //Object.AICheckPointHit(CheckPointNumber, id);
        }
    }
}
