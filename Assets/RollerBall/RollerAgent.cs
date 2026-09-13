using UnityEngine;
using Unity.MLAgents; // 추가: Agent 클래스
using Unity.MLAgents.Sensors; // 추가: VectorSensor
using Unity.MLAgents.Actuators; // 추가: ActionBuffers
using UnityEngine.InputSystem; // 추가: 키보드 입력
public class RollerAgent : Agent // MonoBehaviour → Agent
{
    private Rigidbody _rigidbody;
    
    public Transform target; // Inspector에서 Target을 연결
    public float forceMultiplier = 10f;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    
    public override void OnEpisodeBegin()
    {
// 1) 에이전트가 바닥 아래로 떨어졌다면 속도를 0으로 만들고 제자리로
        if (transform.localPosition.y < 0)
        {
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.linearVelocity = Vector3.zero;
            transform.localPosition = new Vector3(0, 0.5f, 0);
        }
// 2) 목표를 바닥 위 임의의 위치로 옮긴다
        target.localPosition = new Vector3(
            Random.value * 8 - 4,
            0.5f,
            Random.value * 8 - 4);
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.localPosition); // 3개
        sensor.AddObservation(transform.localPosition); // 3개
        sensor.AddObservation(_rigidbody.linearVelocity.x); // 1개
        sensor.AddObservation(_rigidbody.linearVelocity.z); // 1개
    } // 합계 8개
    
    
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
// 행동: x축 힘, z축 힘
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        _rigidbody.AddForce(controlSignal * forceMultiplier);
// 보상
        float distanceToTarget =
            Vector3.Distance(transform.localPosition, target.localPosition);
        if (distanceToTarget < 1.42f) // 목표 도달
        {
            SetReward(1.0f);
            EndEpisode();
        }
        else if (transform.localPosition.y < 0) // 바닥 밖으로 떨어짐
        {
            EndEpisode();
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        var kb = Keyboard.current;
        if (kb == null) return;
        continuousActionsOut[0] = (kb.dKey.isPressed ? 1f : 0f)
                                  - (kb.aKey.isPressed ? 1f : 0f);
        continuousActionsOut[1] = (kb.wKey.isPressed ? 1f : 0f)
                                  - (kb.sKey.isPressed ? 1f : 0f);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
