using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableOnMap : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    private RectTransform rectTransform;
    private RectTransform mapaRect; // El panel padre
    private Canvas canvasPrincipal;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        mapaRect = transform.parent.GetComponent<RectTransform>();
        canvasPrincipal = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Pone el dron que estamos tocando por encima de los demás visualmente
        transform.SetAsLastSibling(); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 1. Movemos el dron según el movimiento del ratón
        rectTransform.anchoredPosition += eventData.delta / canvasPrincipal.scaleFactor;

        // 2. Limitamos la posición para que no se salga del mapa (600x600)
        // Esto asume que tu mapa y tu dron tienen el Pivot en X:0.5, Y:0.5
        Vector2 pos = rectTransform.anchoredPosition;
        
        // Si el mapa mide 600, va de -300 a +300. Usamos rect.xMin y xMax para que sea automático.
        pos.x = Mathf.Clamp(pos.x, mapaRect.rect.xMin, mapaRect.rect.xMax);
        pos.y = Mathf.Clamp(pos.y, mapaRect.rect.yMin, mapaRect.rect.yMax);
        
        rectTransform.anchoredPosition = pos;
    }
}