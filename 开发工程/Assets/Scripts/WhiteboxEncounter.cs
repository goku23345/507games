using UnityEngine;
public class WhiteboxEncounter : MonoBehaviour {
 public GameObject enemyPrefab;
 public float patrolHalfWidth=2;
 public bool exitGuard;
 void OnDrawGizmos(){Gizmos.color=exitGuard?Color.magenta:Color.red;Gizmos.DrawWireCube(transform.position+Vector3.up, new Vector3(.8f,2,.5f));Gizmos.DrawLine(transform.position+Vector3.left*patrolHalfWidth,transform.position+Vector3.right*patrolHalfWidth);
#if UNITY_EDITOR
 UnityEditor.Handles.Label(transform.position+Vector3.up*2.3f,name);
#endif
 }
}
