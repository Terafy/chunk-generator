using UnityEngine;

public class ChunkInteraction : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private ChunkRenderer chunkRenderer;
    [SerializeField] private int damagePerClick = 1;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit) && hit.collider.gameObject == chunkRenderer.gameObject)
        {
            chunkRenderer.HitBlockAtWorldPos(hit.point, damagePerClick);
        }
    }
}
