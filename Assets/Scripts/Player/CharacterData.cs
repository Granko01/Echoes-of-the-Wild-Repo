using UnityEngine;

[CreateAssetMenu(menuName = "EotW/Character Data", fileName = "CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName  = "Character";
    [TextArea(2, 3)]
    public string description    = "";
    public Sprite portrait;
    public Color  characterColor = Color.white;

    [Header("Stats")]
    public float  moveSpeed      = 6f;
    public float  jumpForce      = 12f;
}
