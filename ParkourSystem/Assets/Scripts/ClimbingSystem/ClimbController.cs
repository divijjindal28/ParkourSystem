using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    EnviromentScaner enviromentScaner;
    PlayerController playerController;
    ClimbPoint currentPoint;
    // Start is called before the first frame update
    void Awake()
    {
        enviromentScaner = GetComponent<EnviromentScaner>();
        playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerController.isHanging)
        {

            if (Input.GetButton("Jump") && !playerController.inAction)
            {
                if (enviromentScaner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit))
                {
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);
                    Debug.Log("abc");
                    playerController.SetControl(false);

                    StartCoroutine(JumpToLedge("IdleToHang", currentPoint.transform, 0.41f, 0.54f));
                }
            }
            if (Input.GetButton("Drop") && !playerController.inAction) {
                if (enviromentScaner.DropLedgeCheck(out RaycastHit ledgeHit)) {
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);
                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("DropToHang", currentPoint.transform, 0.30f, 0.45f,handOffset: new Vector3(0.25f, 0.2f,-0.2f)));
                }

            }
        }
        else
        {
            if (Input.GetButton("Drop") && !playerController.inAction) {
                StartCoroutine(JumpFromHang());
                return;
            }
            float h = Mathf.Round(Input.GetAxis("Horizontal"));
            float v = Mathf.Round(Input.GetAxis("Vertical"));
            var inputDir = new Vector2(h, v);

            if (playerController.inAction || inputDir == Vector2.zero) return;

            if (currentPoint.MountPoint && inputDir.y == 1) {
                StartCoroutine(MountFromHang());
                return;
            }

            var neighbour = currentPoint.GetNeighbour(inputDir);
            if (neighbour == null) return;
            if (neighbour.connectionType == ConnectionType.Jump && Input.GetButton("Jump"))
            {
                currentPoint = neighbour.point;
                if (neighbour.direction.y == 1)
                    StartCoroutine(JumpToLedge("HangHopUp", currentPoint.transform, 0.35f, 0.65f, handOffset: new Vector3(.25f, .08f, .15f)));

                if (neighbour.direction.y == -1)
                    StartCoroutine(JumpToLedge("HangHopDown", currentPoint.transform, 0.31f, 0.65f, handOffset: new Vector3(.25f, .1f, .13f)));

                if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("HangHopRight", currentPoint.transform, 0.20f, 0.50f));

                if (neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("HangHopLeft", currentPoint.transform, 0.20f, 0.50f));
            }
            else if (neighbour.connectionType == ConnectionType.Move)
            {
                currentPoint = neighbour.point;
                if (neighbour.direction.x == 1)
                {
                    Debug.Log("ShimmyRight" + currentPoint.transform.localPosition);
                    StartCoroutine(JumpToLedge("ShimmyRight", currentPoint.transform, 0f, 0.38f, handOffset: new Vector3(.25f, .05f, .1f)));
                }


                else if (neighbour.direction.x == -1) {
                    Debug.Log("ShimmyLeft" + currentPoint.transform.localPosition);
                    StartCoroutine(JumpToLedge("ShimmyLeft", currentPoint.transform, 0f, 0.38f, AvatarTarget.LeftHand, handOffset: new Vector3(0.9f, .05f, .1f)));
                }


                //if (neighbour.direction.x == 1)
                //{
                //    Debug.Log("ShimmyRight" + currentPoint.transform.localPosition);
                //    StartCoroutine(JumpToLedge("ShimmyRight", currentPoint.transform, 0f, 0.38f, handOffset: new Vector3(.25f, .05f, .1f)));
                //}


                //else if (neighbour.direction.x == -1)
                //{
                //    Debug.Log("ShimmyLeft" + currentPoint.transform.localPosition);
                //    StartCoroutine(JumpToLedge("ShimmyLeft", currentPoint.transform, 0f, 0.38f, AvatarTarget.LeftHand, handOffset: new Vector3(.25f, .05f, .1f)));
                //}
            }

        }



        IEnumerator JumpToLedge(string anim, Transform ledge, float matchStartTime, float matchTargetTime,
            AvatarTarget hand = AvatarTarget.RightHand, Vector3? handOffset = null)
        {
            var matchParams = new MatchTargetParams()
            {
                pos = GetHandPosition(ledge, hand, handOffset),
                bodypart = AvatarTarget.RightHand,
                startTime = matchStartTime,
                targetTime = matchTargetTime,
                posWeight = Vector3.one
            };

            var targetRotation = Quaternion.LookRotation(-ledge.forward);

            yield return playerController.DoAction(anim, matchParams, targetRotation, true);
            playerController.isHanging = true;

        }

        Vector3 GetHandPosition(Transform ledge, AvatarTarget hand, Vector3? handOffset)
        {
            var OffVal = (handOffset != null) ? handOffset.Value : new Vector3(.25f, .1f, 0.1f);
            var hDir = hand == AvatarTarget.RightHand ? ledge.right : ledge.right;
            Vector3 finalPos = ledge.position + ledge.forward * OffVal.z + Vector3.up * OffVal.y - hDir * OffVal.x;
            Debug.Log("GetHandPosition : "+ finalPos);
            return finalPos;
        }

        IEnumerator JumpFromHang() {
            playerController.isHanging = false;
            yield return playerController.DoAction("JumpFromHang");
            playerController.ResetTargetRotation();
            playerController.SetControl(true);
        }

        IEnumerator MountFromHang()
        {
            playerController.isHanging = false;
            yield return playerController.DoAction("MountFromHang");
            playerController.EnableCharacterController(true);
            yield return new WaitForSeconds(0.5f);
            playerController.ResetTargetRotation();
            playerController.SetControl(true);
        }

        ClimbPoint GetNearestClimbPoint(Transform ledge, Vector3 hitPoint)
        {
            var points = ledge.GetComponentsInChildren<ClimbPoint>();
            ClimbPoint nearestPoint = null;
            float nearestPointDist = Mathf.Infinity;
            foreach (ClimbPoint point in points) {
                float distance = Vector3.Distance(point.transform.position, hitPoint);
                if (distance < nearestPointDist) {
                    nearestPoint = point;

                    nearestPointDist = distance;
                }
            }
            return nearestPoint;
        }
    }
}
