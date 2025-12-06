using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Parallax Scrolling for 2D backgrounds
/// </summary>
public class BackgroundController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [Range(0, 1)]
    [SerializeField] private float parallaxEffectX;

    private float startPos;
    private float spriteWidth;

    private void Start()
    {
        startPos = transform.position.x;
        if (GetComponent<SpriteRenderer>() != null)
            spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void LateUpdate()
    {
        float relativeDistance = cameraTransform.position.x * (1 - parallaxEffectX);

        float distance = cameraTransform.position.x * parallaxEffectX; // 0 = move with cam || 1 = won't move

        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);

        if (relativeDistance > startPos + spriteWidth)
        {
            startPos += spriteWidth;
        }
        else if (relativeDistance < startPos - spriteWidth)
        {
            startPos -= spriteWidth;
        }
    }
}
