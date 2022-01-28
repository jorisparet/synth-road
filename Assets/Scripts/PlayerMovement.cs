using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float movementSmoothing = 0.05f;
    [SerializeField] ParticleSystem explosion;
    [SerializeField] AudioClip explosionSound;
    [SerializeField] AudioClip explosionSoundSlow;
    [SerializeField] AudioClip bonusSound;
    [SerializeField] int obstacleLayer;
    [SerializeField] CameraShake cameraShake;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] GameObject mainMenu;

    bool active = false;
    float movement = 0f;
    float velocity = 0f;
    MeshRenderer meshRenderer;
    BoxCollider boxCollider;
    Rigidbody body;
    public bool hasExploded = false;
    public int bonusCount = 0;
    TimeControl timeControl;
    Shrink shrink;
    AudioSource audioSource;
    AudioSource bonusAS;
    Vector3 initialPosition;
    Quaternion initialRotation;

    public void Initialize()
    {
        active = true;
        transform.SetPositionAndRotation(initialPosition, initialRotation);
        hasExploded = false;
        meshRenderer.enabled = true;
        boxCollider.enabled = true;
        timeControl.enabled = true;
        shrink.enabled = true;
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        bonusCount = 0;
    }

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        boxCollider = GetComponent<BoxCollider>();
        body = GetComponent<Rigidbody>();

        timeControl = GetComponent<TimeControl>();
        shrink = GetComponent<Shrink>();

        audioSource = gameObject.AddComponent<AudioSource>();
        bonusAS = gameObject.AddComponent<AudioSource>();

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        //Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
            MoveLeftOrRight();
    }

    void MoveLeftOrRight()
    {
        movement = Mathf.SmoothDamp(movement, Input.GetAxisRaw("Horizontal"), ref velocity, movementSmoothing);
        transform.position += Vector3.right * movement * Time.deltaTime * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == obstacleLayer && !hasExploded)
        {
            explosion.Play();
            if (timeControl.activated)
            {
                audioSource.PlayOneShot(explosionSoundSlow);
            }
            else
            {
                audioSource.PlayOneShot(explosionSound);
            }
            StartCoroutine(cameraShake.Shake(.2f, 0.5f));
            hasExploded = true;
            meshRenderer.enabled = false;
            boxCollider.enabled = false;
            scoreManager.Stop();
            mainMenu.SetActive(true);
            DisablePowers();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        bonusCount++;
        other.gameObject.GetComponentInChildren<ParticleSystem>().Play();
        bonusAS.PlayOneShot(bonusSound);
        StartCoroutine(scoreManager.MultiplierZoom(0.35f, 1.5f));
        other.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    private void DisablePowers()
    {
        DisableTimeControl();
        DisableShrink();
    }

    private void DisableTimeControl()
    {
        timeControl.enabled = false;
    }

    private void DisableShrink()
    {
        shrink.enabled = false;
    }

}
