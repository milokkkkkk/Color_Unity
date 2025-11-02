using UnityEngine;

public class MouseClick : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if ( Physics.Raycast( ray, out RaycastHit hit ) )
            {
                SelectableObject hitObject = hit.collider.GetComponent<SelectableObject>();
                if ( hitObject != null )
                {
                    hitObject.ChangeSelectionState();
                }
            }
        }
    }
}
