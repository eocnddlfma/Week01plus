using UnityEngine;

public class SSH_SwitchManager : MonoBehaviour
{
    [SerializeField] private SSH_Switch[] _switches;
    [SerializeField] private GameObject  _wall;

    public bool AllOn
    {
        get
        {
            foreach (SSH_Switch sw in _switches)
                if (!sw.IsOn) return false;
            return true;
        }
    }

    public void OnSwitchActivated()
    {
        foreach (SSH_Switch sw in _switches)
        {
            if (!sw.IsOn) return;
        }

        OpenWall();
    }

    private void OpenWall()
    {
        if (_wall != null) _wall.SetActive(false);
    }
}
