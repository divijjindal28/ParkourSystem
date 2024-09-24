using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Parkour System/New Parkour Action")]
public class ParkourAction : ScriptableObject
{
    [SerializeField] string animaName;
    [SerializeField] string obstacleTag;
    [SerializeField] float minHeight;
    [SerializeField] float maxHeight;

    [SerializeField] bool rotateToObstacle;
    [SerializeField] float postActionDelay;
    [Header("TargetMatching")]
    [SerializeField] bool enableTargetMatching;
    [SerializeField] protected AvatarTarget matchBodyPart;
    [SerializeField] float matchStartTime;
    [SerializeField] float matchTargetTime;
    [SerializeField] Vector3 matchPositionWeight = new Vector3(0,1,0);
    
    public Quaternion TargetRotation { get; set; }
    public Vector3 MatchPos { get; set; }
    public bool Mirror { get; set; }
    public virtual bool CheckIfPossible(ObstacleHitData hitData, Transform player) {
        //CheckTag
        if (!string.IsNullOrEmpty(obstacleTag) && hitData.forwardHit.transform.tag != obstacleTag)
            return false;

        //Check Height
        float height = hitData.heightHit.point.y - player.position.y;

        if (height < minHeight || height > maxHeight) {
            return false;
        }

        if (rotateToObstacle)
            TargetRotation = Quaternion.LookRotation( -hitData.forwardHit.normal);

        if (enableTargetMatching)
            MatchPos = hitData.heightHit.point;
        return true;
    }

    public string AnimName => animaName;
    public bool RotateToObstacle => rotateToObstacle;

    public bool EnableTargetMatching => enableTargetMatching;
    public AvatarTarget MatchBodyPart => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;
    public Vector3 MatchPositionWeight => matchPositionWeight;
    public float PostActionDelay => postActionDelay;
}
