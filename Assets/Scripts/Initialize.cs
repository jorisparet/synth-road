using UnityEngine;

public class Initialize : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] ObstacleManager obstacleManager;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] GroundScroller groundScroller;
    [SerializeField] PowerManager powerManager;
    [SerializeField] TimeControl timeControl;
    [SerializeField] Shrink shrink;

    // Initialize all the main game objects to their starting values and parameters
    public void Run()
    {
        mainMenu.SetActive(false);
        playerMovement.Initialize();
        obstacleManager.Initialize();
        scoreManager.Initialize();
        groundScroller.Initialize();
        powerManager.Initialize();
        timeControl.Initialize();
        shrink.Initialize();
    }
}
