using UnityEngine;

public class GroundScroller : MonoBehaviour
{
    [SerializeField] float roadSpeedMultiplier = 0.075f;
    [SerializeField] float edgeSpeedMultiplier = 0.155f;
    [SerializeField] Renderer road;
    [SerializeField] Renderer roadEdge;
    [SerializeField] MovableSpawner obstacleManager;

    bool active = false;
    string _RoadOffset = "_Offset";
    string _EdgeOffset = "_Offset";
    Vector2 currentRoadOffset;
    Vector2 currentEdgeOffset;
    Vector2 offset;

    public void Initialize()
    {
        active = true;
        road.sharedMaterial.SetVector(_RoadOffset, Vector2.zero);
        roadEdge.sharedMaterial.SetVector(_EdgeOffset, Vector2.zero);
    }

    void Update()
    {
        if (active)
            Scroll();
    }

    private void Scroll()
    {
        offset = Vector2.down * obstacleManager.currentScrollSpeed * Time.deltaTime;

        currentRoadOffset = road.sharedMaterial.GetVector(_RoadOffset);
        road.sharedMaterial.SetVector(_RoadOffset, currentRoadOffset + roadSpeedMultiplier * offset);

        currentEdgeOffset = roadEdge.sharedMaterial.GetVector(_EdgeOffset);
        roadEdge.sharedMaterial.SetVector(_EdgeOffset, currentEdgeOffset + edgeSpeedMultiplier * offset);
    }
}
