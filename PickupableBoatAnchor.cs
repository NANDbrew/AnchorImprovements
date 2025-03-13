//using SailwindModdingHelper;
using UnityEngine;
using System.Text.Json;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using HarmonyLib;

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
        float currentThrowPower = 0f;
        private BoatHorizon boatHorizon;
        private bool closeToPlayer;
        private float initialHoldDist = 1.5f;

        Traverse HPthrowPower;

        private void Awake()
        {
            holdDistance = initialHoldDist;
            heldRotationOffset = 200f;
            big = true;

            if (!Main.boatAnchors.Contains(this))
            {
                Main.boatAnchors.Add(this);
            }

        }
        public override void Start()
        {
            base.Start();
            joint = GetComponentInParent<ConfigurableJoint>();
            dataName = Main.NAME + "." + joint.connectedBody.name;
            anchor = joint.GetComponentInParent<Anchor>();
            GetAnchorController();
            boatHorizon = joint.connectedBody.gameObject.GetComponentInChildren<BoatHorizon>();
            StartCoroutine(LoadAnchorData());
        }
        private void Update()
        {
            if ((bool)held)
            {
                if (GetCurrentDistance() >= GetAnchorController().maxLength)
                {
                    OnDrop();
                    held.DropItem();
                    Vector3 yankPos = topAttach.position - transform.position;
                    GetComponentInParent<Rigidbody>().AddForceAtPosition(yankPos.normalized * yankSpeed, joint.transform.position, ForceMode.VelocityChange);
                }
            }
            if (closeToPlayer && !boatHorizon.closeToPlayer)
            {
                SaveAnchorData();
            }
            closeToPlayer = boatHorizon.closeToPlayer;
        }

        public void SaveAnchorData()
        {
            if (!closeToPlayer)
            {
                return;
            }
            if (GetAnchorController() && Main.saveAnchorPosition.Value && joint.linearLimit.limit > 1)
            {
                Vector3 topPos = joint.connectedBody.transform.TransformPoint(joint.connectedAnchor);
                Vector3 pos2 = new Vector3(transform.position.x - topPos.x, transform.position.y - topPos.y, transform.position.z - topPos.z);
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
                    + transform.eulerAngles.z.ToString(CultureInfo.InvariantCulture);

                if (GameState.modData.ContainsKey(dataName))
                {
                    GameState.modData[dataName] = anchorData;
                }
                else
                {
                    GameState.modData.Add(dataName, anchorData);
                }
#if DEBUG
                Main.logSource.LogDebug(joint.connectedBody.name + " (" + anchorData + ")");
#endif
            }
            else if (GameState.modData.Remove(dataName))
            {
                Main.logSource.LogDebug("Removed " + dataName + " from modData");
            }
        }

        IEnumerator LoadAnchorData()
        {
#if DEBUG
            Main.logSource.LogDebug("waiting for load conditions: " + dataName);
#endif
            yield return new WaitUntil(() => GameState.playing && !GameState.justStarted && closeToPlayer);
#if DEBUG
            Main.logSource.LogDebug("looking for data: " + dataName);
#endif
            if (Main.saveAnchorPosition.Value && GameState.modData.TryGetValue(dataName, out string anchorData))
            {
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
                //transform.position = pos + GetTopAttach().position;
                transform.position = pos + joint.connectedBody.transform.TransformPoint(joint.connectedAnchor);
                Vector3 rot = new Vector3(float.Parse(strings[6], CultureInfo.InvariantCulture), float.Parse(strings[7], CultureInfo.InvariantCulture), float.Parse(strings[8], CultureInfo.InvariantCulture));
                transform.eulerAngles = rot;
                if (strings[4] == "True") InvokePrivate(anchor, "SetAnchor");
#if DEBUG
                Main.logSource.LogDebug("loaded data for " + dataName + ": " + anchorData);
#endif
                GameState.modData.Remove(dataName);
            }
        }

        public RopeControllerAnchor GetAnchorController()
        {
            if (anchorController == null) anchorController = joint.connectedBody.gameObject.GetComponent<BoatMooringRopes>().GetAnchorController();
            return anchorController;
        }

        private float GetCurrentDistance()
        {
            return Vector3.Distance(transform.position, joint.connectedBody.transform.TransformPoint(joint.connectedAnchor));
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

            if (held)
            {
                //Debug.Log("holding pointer = " + held);
                if (GameInput.GetKey(InputName.Throw))
                {
                    currentThrowPower += Time.deltaTime;
                    HPthrowPower.SetValue(currentThrowPower);
                    //float num2 = Mathf.Lerp(0, 0.5f, currentThrowPower);
                    //this.transform.Translate(holdingPointer.transform.forward * (0f - num2), Space.World);
                    //holdDistance = initialHoldDist - num2;
                    //Debug.Log(num2);
                }
                else if (GameInput.GetKeyUp(InputName.Throw))
                {
                    Rigidbody component = GetComponent<Rigidbody>();
                    OnDrop();
                    if (currentThrowPower > held.throwDelay)
                    {
                        held.StartCoroutine(ThrowItemAfterDelay(component, currentThrowPower - held.throwDelay, held));
                        currentThrowPower = 0f;
                    }
                    held.DropItem();

                }
            }

        }
        private IEnumerator ThrowItemAfterDelay(Rigidbody heldRigidbody, float force, GoPointer holdingPointer)
        {
            yield return new WaitForFixedUpdate();
            //Debug.Log("currentThrowForce = " + force);
            //Debug.Log("pointer throw force = " + holdingPointer.throwForce);
            //heldRigidbody.AddForce(holdingPointer.transform.forward * holdingPointer.throwForce * Mathf.Min(force, 1) * heldRigidbody.mass);
            heldRigidbody.AddForce(holdingPointer.transform.forward * 20 * Mathf.Min(force, 1), ForceMode.VelocityChange);

#if DEBUG
            Debug.Log("Threw anchor: " + heldRigidbody.name + " @ " + (Mathf.Min(force, 1) * 20) + " m/s");
#endif
        }
        public override void OnPickup()
        {
            HPthrowPower = Traverse.Create(held).Field("currentThrowPower");
            InvokePrivate(anchor, "ReleaseAnchor");
            Main.logSource.LogDebug("Picked up anchor");
            GetAnchorController().currentLength = anchorController.maxLength;
            GetComponent<Collider>().isTrigger = true;
           // Main.logSource.LogDebug("anchor controller limit" + linearLimit.limit);
            //Main.logSource.LogDebug("joint limit" + joint.linearLimit.limit);
        }

        public object InvokePrivate(object obj, string name)
        {
            return AccessTools.Method(obj.GetType(), name).Invoke(obj, null);
        }

        public override void OnDrop()
        {
            GetComponent<Collider>().isTrigger = false;
            holdDistance = initialHoldDist;

            if (isColliding)
            {
                InvokePrivate(anchor, "SetAnchor");
                //this.StartCoroutine(SetRopeLength(0));
                GetAnchorController().currentLength = GetCurrentDistance() / anchorController.maxLength;
            }
            else
            {
                GetAnchorController().currentLength = (GetCurrentDistance() + currentThrowPower * 10) / anchorController.maxLength;
                //this.StartCoroutine(SetRopeLength(Mathf.Min(currentThrowPower, 1) * 5));
            }
            base.OnDrop();
            //Debug.Log(linearLimit.limit);

        }


        IEnumerator WaitForDoneProcess(float timeout)
        {
            while (GetCurrentDistance() < GetAnchorController().maxLength * 0.8 && !isColliding)
            {
                yield return null;
                timeout -= Time.deltaTime;
                if (timeout <= 0f) break;
            }
        }
        IEnumerator SetRopeLength(float timeout)
        {
            //Debug.Log("timer = " + timeout);
            yield return WaitForDone(timeout);// wait for done or [timeout] seconds, whichever comes first.
            GetAnchorController().currentLength = GetCurrentDistance() / anchorController.maxLength;
        }
        YieldInstruction WaitForDone(float timeout) { return StartCoroutine(WaitForDoneProcess(timeout)); }


        private void OnCollisionExit(Collision collision)
        {
            if (collision.collider.CompareTag("Terrain"))
            {
                isColliding = false;
                if (held)
                {
                    InvokePrivate(anchor, "ReleaseAnchor");
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

        /*public override void OnScroll(float input)
        {
            heldRotationOffset = 200f;
        }*/
    }
}
