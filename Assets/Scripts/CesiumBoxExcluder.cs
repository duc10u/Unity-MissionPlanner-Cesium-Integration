using CesiumForUnity;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CesiumBoxExcluder : CesiumTileExcluder
{
    private BoxCollider _boxCollider;
    private Bounds _bounds;

    public bool invert = false;
    private Camera mainCamera; // Reference to the main camera

    protected override void OnEnable()
    {
        // Get the BoxCollider component attached to this GameObject
        this._boxCollider = this.gameObject.GetComponent<BoxCollider>();

        // Initialize bounds based on the collider
        this._bounds = new Bounds(this._boxCollider.center, this._boxCollider.size);

        // Get the main camera in the scene
        mainCamera = Camera.main;

        base.OnEnable();
    }

    protected void Update()
    {
        if (mainCamera != null)
        {
            // Update the center of the BoxCollider to match the camera's position
            this._boxCollider.center = this.transform.InverseTransformPoint(mainCamera.transform.position);
        }

        // Update the bounds to match the collider's center and size
        this._bounds.center = this._boxCollider.center;
        this._bounds.size = this._boxCollider.size;
    }

    public bool CompletelyContains(Bounds bounds)
    {
        return Vector3.Min(this._bounds.max, bounds.max) == bounds.max &&
               Vector3.Max(this._bounds.min, bounds.min) == bounds.min;
    }

    public override bool ShouldExclude(Cesium3DTile tile)
    {
        if (!this.enabled)
        {
            return false;
        }

        if (this.invert)
        {
            return this.CompletelyContains(tile.bounds);
        }

        return !this._bounds.Intersects(tile.bounds);
    }
}
