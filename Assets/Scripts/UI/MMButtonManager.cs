using UnityEngine;

public class MMButtonManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private MMManager.MMButtons _buttonType;

    public void ButtonClicked()
    {
        MMManager._.MMButtonClicked(_buttonType);
    }
}
