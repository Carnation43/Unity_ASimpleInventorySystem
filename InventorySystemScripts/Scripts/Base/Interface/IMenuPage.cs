using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMenuPage
{
    GameObject PageGameObject { get; }

    void OpenPage();

    void ClosePage();

    void ResetPage();
}
