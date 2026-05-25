using UnityEngine;

// Este script se pone en zonas invisibles del mapa (Triggers).
// Cuando el jugador las pisa, lanza un tutorial específico.
public class TutorialTrigger : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Pon un 1 para el tutorial de Interactuar, o un 2 para el de Atacar.")]
    public int tutorialIndexToShow;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si el que pisa la zona invisible es el jugador...
        if (collision.CompareTag("Player"))
        {
            // Avisamos al Mánager de que muestre el tutorial correspondiente
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.ShowTutorial(tutorialIndexToShow);
            }

            // Destruimos esta zona invisible para que no vuelva a saltar si el jugador pasa otra vez por aquí
            Destroy(gameObject);
        }
    }
}