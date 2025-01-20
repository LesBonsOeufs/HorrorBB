using UnityEngine;

namespace Root
{
    public class CollidersBasedOctree : MonoBehaviour
    {
        [SerializeField] private float size = 15f;
        [SerializeField] private bool useBoundsTree = true;
        [SerializeField] private bool usePointTree = false;

        public BoundsOctree<Collider> BoundsTree { get; private set; }
        public PointOctree<Collider> PointTree { get; private set; }

        private void OnValidate()
        {
            //Might need finding only static objects, or objects with a specific tag
            Collider[] lColliders = FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            if (useBoundsTree)
            {
                BoundsTree = new BoundsOctree<Collider>(size, transform.position, 1f, 1.25f);

                foreach (Collider lCollider in lColliders)
                    BoundsTree.Add(lCollider, lCollider.bounds);
            }
            else
                BoundsTree = null;

            if(usePointTree)
            {
                PointTree = new PointOctree<Collider>(size, transform.position, 1f);

                foreach (Collider lCollider in lColliders)
                    PointTree.Add(lCollider, lCollider.transform.position);
            }
            else
                PointTree = null;
        }

        private void OnDrawGizmosSelected()
        {
            if (useBoundsTree)
            {
                BoundsTree.DrawAllBounds(); // Draw node boundaries
                BoundsTree.DrawAllObjects(); // Draw object boundaries
                BoundsTree.DrawCollisionChecks(); // Draw the last *numCollisionsToSave* collision check boundaries
            }
            
            if (usePointTree)
            {
                PointTree.DrawAllBounds(); // Draw node boundaries
                PointTree.DrawAllObjects(); // Mark object positions
            }
        }
    }
}