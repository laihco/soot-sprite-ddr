using UnityEngine;

public class CreditsMenuButtonManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private MMManager.CreditsButtons _buttonType;

    public void ButtonClicked()
    {
        MMManager._.CreditsButtonClicked(_buttonType);
    }
}
