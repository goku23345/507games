using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class OpenStudy {
 static OpenStudy(){
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-studyPlay")>=0&&!SessionState.GetBool("StudyStarted",false)){
   SessionState.SetBool("StudyStarted",true);
   EditorApplication.delayCall+=()=>{EditorSceneManager.OpenScene("Assets/Scenes/DodgeStudy.unity");EditorApplication.EnterPlaymode();};
  }
 }
}
