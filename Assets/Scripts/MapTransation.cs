using Unity.Cinemachine;
using UnityEngine;

public class MapTransation : MonoBehaviour
{

    [SerializeField] PolygonCollider2D mapBoundry;
    CinemachineConfiner2D confiner;
    [SerializeField] Direction direction;
    [SerializeField] float additivPos = 2f;
    enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    [System.Obsolete]
    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            confiner.BoundingShape2D = mapBoundry;
            UpdatePlayerPosition(collision.gameObject);

            MapController_Dynamic.Instance?.UpdateCurrentArea(mapBoundry.name);
        }
    }

    private void UpdatePlayerPosition(GameObject player) {
    
        Vector3 newPos = player.transform.position;

        switch (direction) {
            case Direction.Up:
                newPos.y += additivPos   ;
                break;
            case Direction.Down:
                newPos.y -= additivPos;
                break;
            case Direction.Left:
                newPos.x += additivPos;
                break;
            case Direction.Right:
                newPos.x -= additivPos;
                break;
        }
        player.transform.position = newPos;
    }


}
