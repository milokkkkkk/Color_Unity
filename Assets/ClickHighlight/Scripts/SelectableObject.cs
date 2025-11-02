using UnityEngine;


public class SelectableObject : MonoBehaviour
{
    private SimpleOutline _outline;


    private void Awake()
    {
        _outline = GetComponent<SimpleOutline>();
        _outline.enabled = false;
    }


    public void ChangeSelectionState()
    {
        if ( _outline == null ) return;

        _outline.enabled = !_outline.enabled;
    }
}
