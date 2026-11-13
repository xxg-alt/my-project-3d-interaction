using UnityEngine;

public class SceneSwitch : MonoBehaviour
{
    
    public GameObject[] artCanvas;

    void Start()
    {
        Show();
    }

    
    public void OnCanvas(GameObject Panel)
    {
        Show();
        if(Panel != null)
        {
            Panel.SetActive(true);
            Cameramove.SetUIMode(true);
        }
    }
    public void OffCanvas() { Show(); }

    void Show()
    {
        if (artCanvas != null)
        {
            foreach (GameObject canvas in artCanvas)
            {
                canvas.SetActive(false);
            }
        }   
        Cameramove.SetUIMode(false);
    }
}
