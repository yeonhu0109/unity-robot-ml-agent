

using System.Collections.Generic;
using UnityEngine;










// 로봇 팔의 "몸"을 담당한다.
public class RobotArmController : MonoBehaviour
{
   [System.Serializable]
   // 관절의 하나의 설정과 상태 등을 담고 있는 클래스 객체
   public class Joint
   {
       // 객체의 이름
       public string label = "관절"; // 화면에 표시할 이름. 예: 어깨, 팔꿈치, 좌우 회전
       public Transform pivot; // 회전만 담당하는 오브젝트
       public Vector3 axis = Vector3.left; // 회전축. 회전 방향
       public float minAngle = -90f; // 이 관절이 돌 수 있는 최소 각도(도).
       public float maxAngle = 90f; // "이 관절이 돌 수 있는 최대 각도(도).
       public float maxSpeed = 120f; // 초당 최대 몇 도까지 돌 수 있는가
       public float startAngle = 0f; // 에피소드가 시작될 때 돌아갈 초기 각도
      
       public float Angle { get; private set; } // 지금 각도(도). 이 값이 팔의 진짜 상태다
       public float Velocity { get; private set; } // 지금 각속도(도/초). 관측에 쓴다.


       public void Drive(float normalizedSpeed, float deltaTime)
       {
           // 입력을 "각도"가 아니라 "속도"로 받아서 조금씩 더해 나간다.
           // 그래야 한 프레임에 팔이 순간이동하듯 튀지 않는다.
           float beforeAngle = Angle;
           float deltaAngle = Mathf.Clamp(normalizedSpeed, -1f, 1f) * maxSpeed *  deltaTime;
          
           Angle = Mathf.Clamp(Angle + deltaAngle, minAngle, maxAngle);
           Velocity = deltaTime > 0f ? (Angle - beforeAngle) / deltaTime : 0f;
          
           Apply();
       }


       public void ResetTo(float angle)
       {
           Angle = Mathf.Clamp(angle, minAngle, maxAngle);
           Velocity = 0f;
           Apply();
       }
      
      
       // 해당 모터 실제로 회전을 적용 함수
       void Apply()
       {
           if(pivot != null)
               pivot.localRotation = Quaternion.AngleAxis(Angle, axis);
       }
   }
  
   // 관절 객체 담은 정보
   // 어깨부터 순서대로 넣는다. 관절 개수 = DOF = 액션 개수.
   public Joint[] joints = new Joint[0];
  
   // 오브젝트가 배치되면 호출되는 최초 함수
   void Awake()
   {
       ResetPose();
   }


   /// 모든 관절을 시작 각도로 되돌린다.
   public void ResetPose()
   {
       foreach (var j in joints)
       {
           j.ResetTo(j.startAngle);
       }
   }


   public void Drive(int index, float normalizedSpeed, float deltaTime)
   {
       joints[index].Drive(normalizedSpeed, deltaTime);
   }
  


   void FixedUpdate()
   {
       if (RobotArmInput.ResetPressed) ResetPose();


       for (int i = 0; i < joints.Length; i++)
       {
           Drive(i, RobotArmInput.Joint(i), Time.fixedDeltaTime);
       }
   }

   public float floorHeight = 0f;

   
   Transform[] _chain;
   Transform[] Chain
   {
       get
       {
           if (_chain != null && _chain.Length > 0) return _chain;
           var list = new List<Transform>();
           for (var t = joints[0]; t != null && t != transform; t = t.parent)
               list.Add(t);
           list.Reverse();
           return _chain = list.ToArray();
       }
   }
   
   
   public bool IsBlocked()
   {
   }
}
