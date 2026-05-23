using UnityEngine;

// Este script se coloca en una zona invisible (un "Trigger") justo en la entrada a la sala del jefe. 
// Al pisarlo, despierta al jefe y arranca el combate.
public class BossTrigger : MonoBehaviour
{
    // Arrastra aquí el script del jefe desde el Inspector de Unity 
    // para que este activador sepa a quién tiene que despertar.
    public BossHealth bossScript;

    // Un seguro para que el evento solo ocurra una vez. 
    // Evita que el jefe se active repetidamente si entramos y salimos de la zona muy rápido.
    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Comprobamos si ha sido exactamente el Jugador quien ha pisado la zona 
        // (y no una bala o un enemigo cualquiera), y que no se haya activado ya antes.
        if (!activated && collision.CompareTag("Player"))
        {
            // Marcamos el seguro como activado para no volver a repetirlo.
            activated = true;

            // Por pura seguridad, comprobamos que hemos asignado un jefe en el Inspector.
            if (bossScript != null)
            {
                // Le damos la orden al jefe de despertar y mostrar su barra de vida.
                bossScript.ActivationBoss();
            }
        }
    }
}