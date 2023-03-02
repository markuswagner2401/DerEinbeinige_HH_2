using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class ConnectButton : MonoBehaviour
{
    [SerializeField] UIConnectorActivator uIConnectorActivator;

    [SerializeField] ButtonFunction buttonFunction;
    public enum ButtonFunction
    {
        connectAsRacer,
        connectAsZuschauer,
        connectAsInspizient,
        disconnect,
        requestTimeline,
        cancel
    }

    public float resetDuration = 1f;

    public UnityEvent OnKlick;
    public UnityEvent OnReset;

    [SerializeField] Color activeColor;
    [SerializeField] Color inactiveColor;

    [SerializeField] bool toggleOn = false;




    [SerializeField] private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        if (buttonFunction == ButtonFunction.requestTimeline)
        {

            ColorBlock colors = button.colors;
            colors.normalColor = toggleOn ? activeColor : inactiveColor;
            button.colors = colors;
        }
    }

    public void OnClick()
    {
        StartCoroutine(ButtonRoutine());
    }

    IEnumerator ButtonRoutine()
    {
        OnKlick.Invoke();

        switch (buttonFunction)
        {
            case ButtonFunction.connectAsRacer:
                MultiplayerConnector.instance.Connect(true);
                MultiplayerConnector.instance.SetRole(Role.Rennfahrer);
                break;

            case ButtonFunction.connectAsZuschauer:
                MultiplayerConnector.instance.Connect(true);
                MultiplayerConnector.instance.SetRole(Role.Zuschauer);
                break;

            case ButtonFunction.connectAsInspizient:
                MultiplayerConnector.instance.Connect(true);
                MultiplayerConnector.instance.SetRole(Role.Inspizient);
                break;
            

            case ButtonFunction.disconnect:
                MultiplayerConnector.instance.Connect(false);
                MultiplayerConnector.instance.SetRole(Role.None);
                break;

            case ButtonFunction.requestTimeline:
                toggleOn = !toggleOn;
                ColorBlock colors = button.colors;
                colors.normalColor = toggleOn ? activeColor : inactiveColor;
                button.colors = colors;
                TimeLineHandler.instance.GetComponent<SyncPlayableDirector>().SetCanGetOwnership(toggleOn);
                break;

            case ButtonFunction.cancel:
                break;

            default:
                break;
        }



        button.interactable = false;
        yield return new WaitForSeconds(resetDuration);
        button.OnPointerExit(null);
        button.interactable = true;
        //button.Select();
        if(uIConnectorActivator != null)
        {
            uIConnectorActivator.ShowConnector(false);
        }
        
        OnReset.Invoke();
    }
}
