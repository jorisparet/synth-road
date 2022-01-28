using UnityEngine;

public class GroundScroller : MonoBehaviour
{
    [SerializeField] float roadSpeedMultiplier = 1f;
    [SerializeField] float edgeSpeedMultiplier = 1f;
    [SerializeField] Renderer road;
    [SerializeField] Renderer roadEdge;
    [SerializeField] ObstacleManager obstacleManager;

    bool active = false;
    string _RoadOffset = "_Offset";
    string _EdgeOffset = "_Offset";
    Vector2 currentRoadOffset;
    Vector2 currentEdgeOffset;
    Vector2 offset;

    public void Initialize()
    {
        active = true;
        road.material.SetVector(_RoadOffset, Vector2.zero);
        roadEdge.material.SetVector(_EdgeOffset, Vector2.zero);
    }

    void Update()
    {
        if (active)
            Scroll();
    }

    private void Scroll()
    {
        offset = Vector2.down * obstacleManager.scrollSpeed * Time.deltaTime;

        currentRoadOffset = road.material.GetVector(_RoadOffset);
        road.material.SetVector(_RoadOffset, currentRoadOffset + roadSpeedMultiplier * offset);

        currentEdgeOffset = roadEdge.material.GetVector(_EdgeOffset);
        roadEdge.material.SetVector(_EdgeOffset, currentEdgeOffset + edgeSpeedMultiplier * offset);
    }
}
