using SailwindModdingHelper;
using UnityEngine;
using System.Text.Json;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;

namespace AnchorRework
{
    internal class PickupableBoatAnchor : PickupableItem
    {
        public bool isColliding;
        public ConfigurableJoint joint;
        public Anchor anchor;
        public float yankSpeed = 15;
        private Transform topAttach;
        private RopeControllerAnchor anchorController;
        public string dataName;
        private bool gameSettled;

        //public Dictionary<string, string> modData;
        /*transform.position.x.ToString(CultureInfo.InvariantCulture) + ","
                    + transform.position.y.ToString(CultureInfo.InvariantCulture) + ","
                    + transform.position.z.ToString(CultureInfo.InvariantCulture) + ","*/


        public void SaveAnchorData()
        {
            if (Main.saveAnchorPosition.Value && joint.linearLimit.limit > 1)
            {
                Vector3 pos2 = new Vector3(transform.position.x - GetTopAttach().position.x, transform.position.y - GetTopAttach().position.y, transform.position.z - GetTopAttach().position.z);
                // pos x, pos y, pos z, controller rope length, is set?, joint rope length.
                string anchorData = 
                    pos2.x.ToString(CultureInfo.InvariantCulture) + "," 
                    + pos2.y.ToString(CultureInfo.InvariantCulture) + "," 
                    + pos2.z.ToString(CultureInfo.InvariantCulture) + "," 
                    + GetAnchorController().currentLength.ToString(CultureInfo.InvariantCulture) + ","
                    + anchor.IsSet().ToString() + ","
                    + joint.linearLimit.limit.ToString(CultureInfo.InvariantCulture) + ","
                    + transform.eulerAngles.x.ToString(CultureInfo.InvariantCulture) + ","
                    + transform.eulerAngles.y.ToString(CultureInfo.InvariantCulture) + ","
                    + transform.eulerAngles.z.ToString(CultureInfo.InvariantCulture) ;

                if (GameState.modData.ContainsKey(dataName))
                {
                    GameState.modData[dataName] = anchorData;
                }
                else
                {
                    GameState.modData.Add(dataName, anchorData);
                }
                Main.logSource.LogDebug(anchorData);
            }
            else if (GameState.modData.ContainsKey (dataName)) 
            {
                GameState.modData.Remove(dataName); 
            }
        }

        IEnumerator LoadAnchorData()
        {

            if (Main.saveAnchorPosition.Value && GameState.modData.ContainsKey(dataName)) 
            {
                GameState.modData.TryGetValue(dataName, out string anchorData);
                string[] strings = anchorData.Split(',');

                //Vector3 pos = new Vector3(float.Parse(strings[0], CultureInfo.InvariantCulture), float.Parse(strings[1], CultureInfo.InvariantCulture), float.Parse(strings[2], CultureInfo.InvariantCulture));
                Vector3 pos = new Vector3(float.Parse(strings[0], CultureInfo.InvariantCulture), float.Parse(strings[1], CultureInfo.InvariantCulture), float.Parse(strings[2], CultureInfo.InvariantCulture));
                float savedLength = float.Parse(strings[5], CultureInfo.InvariantCulture);
                float dist = Vector3.Magnitude(pos);
                if (dist > savedLength && dist < GetAnchorController().maxLength) savedLength = dist;

                var lim = joint.linearLimit;
                lim.limit = savedLength; //Vector3.Distance(pos, GetTopAttach().position); //float.Parse(strings[5], CultureInfo.InvariantCulture);
                GetAnchorController().currentLength = savedLength / GetAnchorController().maxLength; //float.Parse(strings[3], CultureInfo.InvariantCulture);
                joint.linearLimit = lim;
                //Main.logSource.LogDebug("rope length= " + GetAnchorController().currentLength);
                //Main.logSource.LogDebug("joint limit= " + joint.linearLimit.limit);
                yield return new WaitForEndOfFrame();
                transform.position = pos + GetTopAttach().position;

                Vector3 rot = new Vector3(float.Parse(strings[6], CultureInfo.InvariantCulture), float.Parse(strings[7], CultureInfo.InvariantCulture), float.Parse(strings[8], CultureInfo.InvariantCulture));
                transform.eulerAngles = rot;
                if (strings[4] == "True") anchor.InvokePrivateMethod("SetAnchor");
            }
            //modData = GameState.modData;

        }

        public Transform GetTopAttach()
        {
            if (topAttach == null)
            {
                topAttach = joint.connectedBody.gameObject.GetComponent<BoatMooringRopes>().GetAnchorController().GetComponent<RopeEffect>().GetPrivateField<Transform>("attachmentOne");
            }
            return topAttach; 
        }

        public RopeControllerAnchor GetAnchorController()
        {
            if (anchorController == null) anchorController = joint.connectedBody.gameObject.GetComponent<BoatMooringRopes>().GetAnchorController();
            return anchorController;
        }

        private float GetCurrentDistance()
        {
            return Vector3.Distance(transform.position, GetTopAttach().position);
        }

        public override void ExtraLateUpdate()
        {

            if (held && GetCurrentDistance() > anchorController.maxLength * 0.9f)
            {
                //ModLogger.Log(Main.mod, joint.name);

                this.enableRedOutline = true;
            }
            else
            {
                this.enableRedOutline = false;
            }
        }
        private void Awake()
        {
            gameSettled = false;
            holdDistance = 1.5f;
            heldRotationOffset = 200f;
            big = false;
            joint = GetComponentInParent<ConfigurableJoint>();
            anchor = joint.GetComponentInParent<Anchor>();
            anchorController = joint.connectedBody.gameObject.GetComponent<BoatMooringRopes>().GetAnchorController();
            dataName = Main.NAME + "." + joint.connectedBody.name;

            if (!Main.boatAnchors.Contains(this))
            {
                Main.boatAnchors.Add(this);
            }

        }
        private void Update()
        {
            if (this.held)
            {
                //anchor.GetComponent<CapsuleCollider>().enabled = false;

                if (GetCurrentDistance() >= GetAnchorController().maxLength)
                {
                    this.OnDrop();
                    this.held.DropItem();
                    Vector3 yankPos = topAttach.position - this.transform.position;
                    GetComponentInParent<Rigidbody>().AddForceAtPosition(yankPos.normalized * yankSpeed, joint.transform.position, ForceMode.VelocityChange);
                }
            }

            if (!gameSettled && GameState.playing && !GameState.justStarted && !Refs.shiftingWorld.GetComponent<FloatingOriginManager>().GetPrivateField<bool>("instantShifting"))
            {
                StartCoroutine(LoadAnchorData());
                gameSettled = true;
            }

/*            else
            {
                anchor.GetComponent<CapsuleCollider>().enabled = true;
            }*/
        }
        public override void OnPickup()
        {
            anchor.InvokePrivateMethod("ReleaseAnchor");
            Main.logSource.LogDebug("Picked up anchor");
            GetAnchorController().currentLength = anchorController.maxLength;

           // Main.logSource.LogDebug("anchor controller limit" + linearLimit.limit);
            //Main.logSource.LogDebug("joint limit" + joint.linearLimit.limit);
        }
        public override void OnDrop()
        {
            GetAnchorController().currentLength = GetCurrentDistance() / anchorController.maxLength;

            if (isColliding)
            {
                anchor.InvokePrivateMethod("SetAnchor");
            }
            base.OnDrop();
            //Debug.Log(linearLimit.limit);

        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.collider.CompareTag("Terrain"))
            {
                isColliding = false;
                if (held)
                {
                    anchor.InvokePrivateMethod("ReleaseAnchor");
                }

            }      
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Terrain"))
            {
                isColliding = true;
            }
        }

        public override void OnScroll(float input)
        {
            heldRotationOffset = 200f;
        }
    }
}
