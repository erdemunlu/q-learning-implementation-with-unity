using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickManager : MonoBehaviour
{

    public static Vector3 leftClick= new Vector3(-29, 29, 0);
    public static Vector3 rightClick;
    public static GameObject rightClickObject;
    public static int flag = 0;

    void Update()
    {
        if(flag == 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
                if (hit.collider != null)
                {
                    UnityEngine.Debug.Log(hit.collider.gameObject.name);
                    UnityEngine.Debug.Log(hit.collider.gameObject.transform.position);
                    leftClick = hit.collider.gameObject.transform.position;
                }

            }
            else if (Input.GetMouseButtonDown(1))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
                if (hit.collider != null)
                {
                    UnityEngine.Debug.Log(hit.collider.gameObject.name);
                    UnityEngine.Debug.Log(hit.collider.gameObject.transform.position);
                    rightClick = hit.collider.gameObject.transform.position;
                    rightClickObject = hit.collider.gameObject;
                    
                }
            }
        }
        else if(flag == 1)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
                if (hit.collider != null)
                {
                    leftClick = hit.collider.gameObject.transform.position;
                }
            }
            
        }
        

    }
    
}
