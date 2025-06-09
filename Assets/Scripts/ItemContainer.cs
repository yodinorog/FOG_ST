using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemContainer : MonoBehaviour
{
    public ItemInstance item;
    public int amount = 1;
    public bool isBeingPickedUp = false;

    public GameObject droppedBy;
    public bool hasExitedDropper = false;

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == droppedBy)
        {
            hasExitedDropper = true;
        }
    }
}
