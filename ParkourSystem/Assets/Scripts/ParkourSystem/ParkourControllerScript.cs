using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourControllerScript : MonoBehaviour
{
    [SerializeField] List<ParkourAction> parkourActions;
    [SerializeField] ParkourAction JumpDownAction;
    EnviromentScaner enviromentScaner;
    Animator animator;
    
    PlayerController playerController;
    [SerializeField] float autoJumpHeightLimit = 1;
    private void Awake()
    {
        enviromentScaner = GetComponent<EnviromentScaner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        var hitData = enviromentScaner.ObstacleCheck();
        if (Input.GetButton("Jump") && !playerController.inAction && !playerController.isHanging) {
            
            if (hitData.forwardHitFound)
            {
                foreach (var action in parkourActions) {
                    if (action.CheckIfPossible(hitData, transform)) {
                        StartCoroutine(DoParkourAction(action));
                        break;
                    }
                }
                
            }
        }

        if (playerController.isOnLedge && !playerController.inAction && !hitData.forwardHitFound) {

            bool shouldJump = true;
            if (playerController.LedgeData.height > autoJumpHeightLimit && !Input.GetButton("Jump"))
                shouldJump = false;

            if(shouldJump && playerController.LedgeData.angle <= 50) {
                playerController.isOnLedge = false;
                StartCoroutine(DoParkourAction(JumpDownAction));
            }
            
        }
        
       // enviromentScanner.ObstacleCheck();
    }

    IEnumerator DoParkourAction(ParkourAction action) {

        
        playerController.SetControl(false);

        MatchTargetParams matchTargetParams = null;
        if (action.EnableTargetMatching) {
            matchTargetParams = new MatchTargetParams()
            {
                pos = action.MatchPos,
                bodypart = action.MatchBodyPart,
                posWeight = action.MatchPositionWeight,
                startTime = action.MatchStartTime,
                targetTime = action.MatchTargetTime

            };
        }
        yield return playerController.DoAction(action.AnimName, matchTargetParams, action.TargetRotation, action.RotateToObstacle,
            action.PostActionDelay, action.Mirror);

        playerController.SetControl(true);
        
    }

    void MatchTarget(ParkourAction action) {
        if (animator.isMatchingTarget) return;
        animator.MatchTarget(action.MatchPos,transform.rotation,action.MatchBodyPart,
            new MatchTargetWeightMask(action.MatchPositionWeight,0), action.MatchStartTime,
            action.MatchTargetTime);
    }

}
