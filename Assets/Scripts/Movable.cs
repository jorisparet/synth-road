using UnityEngine;

public class Movable : MonoBehaviour
{
    [SerializeField] float disableCollisionPosition = 2f;
    [SerializeField] float destroyPosition = -15;

    MovableSpawner movableSpawner;
    BoxCollider[] colliders;
    bool collisionsEnabled = true;

    private void Awake()
    {
        movableSpawner = FindObjectOfType<MovableSpawner>();
        colliders = GetComponentsInChildren<BoxCollider>();
    }

    void FixedUpdate()
    {

        transform.position += Vector3.back * movableSpawner.currentScrollSpeed * Time.fixedDeltaTime;

        // Disable collisions after passing the player
        if (collisionsEnabled && transform.position.z < disableCollisionPosition)
        {
            EnableCollisions(false);
        }

        // Destroy object when it goes too far beyond the player
        if (transform.position.z < destroyPosition)
        {
            DestroyMovable();
        }
    }

    public void DestroyMovable()
    {
        Destroy(gameObject);
    }

    void EnableCollisions(bool enabled)
    {
        foreach (BoxCollider collider in colliders)
        {
            collider.enabled = enabled;
        }
        collisionsEnabled = enabled;
    }
}
