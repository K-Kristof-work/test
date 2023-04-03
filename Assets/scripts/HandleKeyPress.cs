using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HandleKeyPress : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(IsPauseMenuActive())
            {
                DeactivatePauseMenu();
            }
            else
            {
                ActivatePauseMenu();
            }
        }        
    }

    public void ActivatePauseMenu()
    {
        Transform parent = gameObject.transform;
        if (parent != null)
        {
            Transform pauseMenuTransform = parent.Find("PauseMenu");
            if (pauseMenuTransform != null)
            {
                if (!pauseMenuTransform.gameObject.activeSelf)
                {
                    pauseMenuTransform.gameObject.SetActive(true);
                }
            }
        }
    }

    public void DeactivatePauseMenu()
    {
        Transform parent = gameObject.transform;
        if (parent != null)
        {
            Transform pauseMenuTransform = parent.Find("PauseMenu");
            if (pauseMenuTransform != null)
            {
                if (pauseMenuTransform.gameObject.activeSelf)
                {
                    pauseMenuTransform.gameObject.SetActive(false);
                }
            }
        }
    }

    public bool IsPauseMenuActive()
    {
        Transform parent = gameObject.transform;
        if (parent != null)
        {
            Transform pauseMenuTransform = parent.Find("PauseMenu");
            if (pauseMenuTransform != null)
            {
                return pauseMenuTransform.gameObject.activeSelf;
            }
        }
        return false;
    }
    
}
