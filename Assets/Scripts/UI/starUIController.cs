using UnityEngine;
using UnityEngine.EventSystems;

public class starUIController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject star;

    public void Start()
    {
       if (star != null)
        {
            star.SetActive(false);
        }
    }
    //event data described the pointer (mouse) position
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (star != null)
        {
            star.SetActive(true);
            LeanTween.scale(star, new Vector3(2f, 2f, 2f), 0.35f);
            LeanTween.rotateAroundLocal(star, new Vector3(0, 0, 360), 360f, 0.35f);
        }
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (star != null)
        {
            star.transform.localScale = new Vector3(1f, 1f, 1f);
            star.SetActive(false);
        }
    }
}
