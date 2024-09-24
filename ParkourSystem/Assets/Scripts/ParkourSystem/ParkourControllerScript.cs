using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourControllerScript : MonoBehaviour
{
    [SerializeField] List<ParkourAction> parkourActions;
    [SerializeField] ParkourAction JumpDownAction;
    EnviromentScaner enviromentScaner;
    Animator animator;
    bool inAction;
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
        if (Input.GetButton("Jump") && !inAction) {
            
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

        if (playerController.isOnLedge && !inAction && !hitData.forwardHitFound) {

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

        inAction = true;
        playerController.SetControl(false);
        animator.SetBool("mirrorAction", action.Mirror);
        animator.CrossFade(action.AnimName, 0.2f);
        yield return null;
        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(action.AnimName))
            Debug.LogError("Parkour Animation is wrong");
       

        float timer = 0f;
        while (timer <= animState.length) {
            timer += Time.deltaTime;

            // Rotate player towards obstacle
            if (action.RotateToObstacle) {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, action.TargetRotation, playerController.RotationSpeed * Time.deltaTime);
            }

            if (action.EnableTargetMatching)
            {
                MatchTarget(action);
            }

            if(animator.IsInTransition(0) && timer > 0.5f)
                    break;

            yield return null;
        }
        yield return new WaitForSeconds(action.PostActionDelay);
        playerController.SetControl(true);
        inAction = false;
    }

    void MatchTarget(ParkourAction action) {
        if (animator.isMatchingTarget) return;
        animator.MatchTarget(action.MatchPos,transform.rotation,action.MatchBodyPart,
            new MatchTargetWeightMask(action.MatchPositionWeight,0), action.MatchStartTime,
            action.MatchTargetTime);
    }

}
