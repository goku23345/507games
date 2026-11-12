using UnityEngine;
// Move/scale this module root to edit the playable surface and its visual together.
public class WhiteboxPlatform : MonoBehaviour {
 public bool shortcutOnly;
 public KnightLevel.Platform Surface {get{return new KnightLevel.Platform(transform.position.x-Mathf.Abs(transform.lossyScale.x)*.5f,transform.position.x+Mathf.Abs(transform.lossyScale.x)*.5f,transform.position.y);}}
}
