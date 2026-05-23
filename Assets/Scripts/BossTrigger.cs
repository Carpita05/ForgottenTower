using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public BossHealth bossScript; // Arrastra aquí al BossSlime
    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si el jugador entra y aún no se ha activado
        if (!activated && collision.CompareTag("Player"))
        {
            activated = true; // Para que no se active mil veces

            if (bossScript != null)
            {
                bossScript.ActivationBoss();
            }

            // Opcional: Puedes destruir este objeto después de activarlo 
            // o simplemente dejarlo así.
        }
    }
}