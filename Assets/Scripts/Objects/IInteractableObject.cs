using UnityEngine
    ;
public interface IInteractable
{
    void Interact();           // Основное действие
    void OnPointerEnter();     // Когда луч навёлся на объект
    void OnPointerStay();      // Каждый кадр пока наведены
    void OnPointerExit();      // Когда луч ушёл с объекта

}
