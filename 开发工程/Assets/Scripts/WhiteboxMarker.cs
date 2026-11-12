using UnityEngine;
public class WhiteboxMarker : MonoBehaviour {
 public enum Role { Entry,Checkpoint,StoryItem,ReturnExit }
 public Role role;
 void OnDrawGizmos(){Gizmos.color=Color.cyan;Gizmos.DrawWireSphere(transform.position,.3f);
#if UNITY_EDITOR
 UnityEditor.Handles.Label(transform.position+Vector3.up*.5f,role.ToString());
#endif
 }
}
