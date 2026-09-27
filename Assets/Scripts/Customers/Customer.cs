using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Customer", menuName = "ScriptableStorage/Customer")]
public class Customer : ScriptableObject
{
    public Image CharacterPortrait;
    public Image OpenMouthPortrait;
    public int npcId;
    public string Name;
    public AudioClip TextSFX;
}
