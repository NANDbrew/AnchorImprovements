using SailwindModdingHelper;
using UnityEngine;

namespace AnchorRework
{
    internal class PickupableBoatAnchor : PickupableItem
    {
        public bool isColliding;
        Vector3 initialPos;
        Transform initialParent;
        float outCurrentSqrDist;
        ConfigurableJoint joint;
        Anchor anchor;
        public float yankForce = 200;
        private float GetCurrentDistanceSquared()
        {
            Vector3 b = this.initialParent.TransformPoint(this.initialPos);
            return Vector3.SqrMagnitude(base.transform.position - b);
        }

        public override void ExtraLateUpdate()
        {
            if (!this.held)
            {
                this.enableRedOutline = false;
                return;
            }
            if (this.outCurrentSqrDist > joint.linearLimit.limit * 0.8f)
            {
                ModLogger.Log(Main.mod, joint.name);

                this.enableRedOutline = true;
                return;
            }
            this.enableRedOutline = false;
        }
        private void Awake()
        {
            joint = GetComponentInParent<ConfigurableJoint>();
            anchor = joint.GetComponentInParent<Anchor>();
            initialPos = base.transform.localPosition;
            initialParent = base.transform.parent;
        }
        private void Update()
        {
            if (this.held)
            {
                float currentDistanceSquared2 = Mathf.Sqrt(GetCurrentDistanceSquared());
                this.outCurrentSqrDist = currentDistanceSquared2;
                if (currentDistanceSquared2 > joint.linearLimit.limit)
                {
                    this.OnDrop();
                    this.held.DropItem();
                    Vector3 yankPos = initialPos - this.transform.position;
                    GetComponentInParent<Rigidbody>().AddForceAtPosition(yankPos.normalized * yankForce, joint.transform.position, ForceMode.Impulse);
                }
            }
        }
        public override void OnPickup()
        {
            heldRotationOffset = 270f;
            anchor.InvokePrivateMethod("ReleaseAnchor");
        }
        public override void OnDrop()
        {
            if (isColliding)
            {
                anchor.InvokePrivateMethod("SetAnchor");
            }
            base.OnDrop();
        }
        private void OnCollisionExit()
        {
            isColliding = false;
            anchor.InvokePrivateMethod("ReleaseAnchor");
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Terrain"))
            {
                isColliding = true;
            }
        }
    }
}
