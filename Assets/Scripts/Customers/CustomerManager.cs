using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public List<Customer> NPCs;
    public Customer FindNPC(int id)
    {
        for (int i = 0; i < NPCs.Count; i++)
        {
            if (NPCs[i].npcId == id)
                return NPCs[i];
        }
        return null;
    }
}
