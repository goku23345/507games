using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class BossLiveJob {
 static BossLiveJob(){EditorApplication.update+=Tick;}
 static void Tick(){
  if(!System.IO.File.Exists("boss-upgrade.request")||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
  if(EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.isPlaying=false;return;}
  System.IO.File.Delete("boss-upgrade.request");EditorSceneManager.SaveOpenScenes();if(System.IO.File.Exists("lift-upgrade.request")){System.IO.File.Delete("lift-upgrade.request");BuildStudy.UpgradeLift();}else BuildStudy.UpgradeBoss();System.IO.File.WriteAllText("boss-live.done","ok");
 }
}
