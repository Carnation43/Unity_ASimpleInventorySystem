using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RadialMenuIconDB", menuName = "ScriptableObjects/RadialMenuIconDB")]
public class RadialMenuIconDB : ScriptableObject
{
    [Header("Radial Menu Icons")]
    // Common use icon
    public Sprite detailsIcon;
    public Sprite dropIcon;
    public Sprite sortIcon;

    // Specific icon
    public Sprite useIcon;
    public Sprite combineIcon;
    public Sprite fixIcon;
    public Sprite equipIcon;
    public Sprite unequipIcon;
    public Sprite enhanceIcon;
    public Sprite presentIcon;
    public Sprite craftIcon;
}
