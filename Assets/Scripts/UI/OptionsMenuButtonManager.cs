using UnityEngine;

public class OptionsMenuButtonManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private MMManager.OptionsBack _buttonType;

    public void ButtonClicked()
    {
        MMManager._.OptionsButtonClicked(_buttonType);
    }
}
