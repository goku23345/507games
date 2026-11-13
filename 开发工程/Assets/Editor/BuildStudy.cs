using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using UnityEditor.Build.Reporting;
public static class BuildStudy {
 public static void Run(){
  System.IO.Directory.CreateDirectory("Assets/Resources");
  var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Block.mat");
  if(mat==null){mat=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(mat,"Assets/Resources/Block.mat");}
  var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Zombie/FBXs/Zombie1.FBX");
  if(model==null)throw new System.Exception("Zombie model missing");
  var root=new GameObject("Enemy");
  var visual=(GameObject)PrefabUtility.InstantiatePrefab(model);visual.transform.SetParent(root.transform,false);
  Bounds bounds=new Bounds();bool first=true;
  foreach(var r in visual.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}
  float scale=2.25f/bounds.size.y;visual.transform.localScale=Vector3.one*scale;visual.transform.localPosition=new Vector3(-bounds.center.x*scale,-bounds.min.y*scale,-bounds.center.z*scale);
  var em=new Material(mat);em.color=new Color(.52f,.46f,.37f);
  var tex=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Zombie/Textures/Zombie_ao.png");em.mainTexture=tex;
  if(AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Enemy.mat")!=null)AssetDatabase.DeleteAsset("Assets/Resources/Enemy.mat");
  AssetDatabase.CreateAsset(em,"Assets/Resources/Enemy.mat");
  foreach(var r in visual.GetComponentsInChildren<Renderer>()){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)mats[i]=em;r.sharedMaterials=mats;}
  var old=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Resources/Enemy.controller");if(old!=null)AssetDatabase.DeleteAsset("Assets/Resources/Enemy.controller");
  var ctl=AnimatorController.CreateAnimatorControllerAtPath("Assets/Resources/Enemy.controller");
  string[] names={"Idle","Walk","Attack","Fall"};string[] paths={"Z_Idle","Z_Walk_InPlace","Z_Attack","Z_FallingBack"};
  for(int i=0;i<4;i++){var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Zombie/Animations/"+paths[i]+".anim");var state=ctl.layers[0].stateMachine.AddState(names[i]);state.motion=clip;Debug.Log("ASSET_CLIP "+names[i]+" "+(clip!=null?clip.length:0));}
  var animator=visual.GetComponent<Animator>();if(animator==null)animator=visual.AddComponent<Animator>();animator.runtimeAnimatorController=ctl;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
  foreach(var c in root.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);
  PrefabUtility.SaveAsPrefabAsset(root,"Assets/Resources/Enemy.prefab");Object.DestroyImmediate(root);AssetDatabase.SaveAssets();
  // Keep source GUIDs and sprite local IDs, so the original sprite clips remain valid.
  string art="Assets/Hero Knight - Pixel Art/";
  foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{art})){
   var ti=AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;
   if(ti!=null){ti.filterMode=FilterMode.Point;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.SaveAndReimport();}
  }
  var previous=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Resources/Knight.controller");if(previous!=null)AssetDatabase.DeleteAsset("Assets/Resources/Knight.controller");
  var kc=AnimatorController.CreateAnimatorControllerAtPath("Assets/Resources/Knight.controller");
  string[] states={"Idle","Run","Jump","Fall","Roll","Attack1","Attack2","Attack3","Hurt","Death"};
  foreach(string name in states){var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(art+"Animations/HeroKnight_"+name+".anim");if(clip==null)throw new System.Exception("Missing Knight "+name);var st=kc.layers[0].stateMachine.AddState(name);st.motion=clip;Debug.Log("KNIGHT_CLIP "+name+" length="+clip.length);}
  var knight=new GameObject("Hero Knight");var sr=knight.AddComponent<SpriteRenderer>();
  var sprites=AssetDatabase.LoadAllAssetsAtPath(art+"Sprites/HeroKnight.png");foreach(var a in sprites)if(a is Sprite){sr.sprite=(Sprite)a;break;}
  var sm=new Material(Shader.Find("Sprites/Default"));if(AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/KnightSprite.mat")!=null)AssetDatabase.DeleteAsset("Assets/Resources/KnightSprite.mat");AssetDatabase.CreateAsset(sm,"Assets/Resources/KnightSprite.mat");sr.sharedMaterial=sm;
  knight.AddComponent<Animator>().runtimeAnimatorController=kc;PrefabUtility.SaveAsPrefabAsset(knight,"Assets/Resources/Knight.prefab");Object.DestroyImmediate(knight);AssetDatabase.SaveAssets();
  PlayerSettings.productName="骑士试炼 - Platform Combat";PlayerSettings.companyName="Six Person Study";
  PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;
  var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);var input=settings.FindProperty("activeInputHandler");if(input!=null){input.intValue=0;settings.ApplyModifiedProperties();}
  var lowerScene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var levelRoot=new GameObject("LOWER_BLOCKOUT / TEAM FILL-IN").transform;
  var entry=Zone(levelRoot,"01_EntryHub / safe landing + return gate");
  var low=Zone(levelRoot,"02_LowRoute / combat corridor");
  var high=Zone(levelRoot,"03_HighRoute / jumping + rewards");
  var end=Zone(levelRoot,"04_EndArena / story archive + surface return");

  entry.position=new Vector3(75,-18,0);low.position=new Vector3(102,-18,0);high.position=new Vector3(91,-14,0);end.position=new Vector3(127,-18,0);
  System.IO.Directory.CreateDirectory("Assets/Whitebox/Materials");System.IO.Directory.CreateDirectory("Assets/Whitebox/Prefabs");
  var matLow=ModuleMaterial("LowerRoute",mat,new Color(.28f,.25f,.42f));
  var matHigh=ModuleMaterial("HighRoute",mat,new Color(.36f,.48f,.58f));

  var edgeMat=ModuleMaterial("Edge",mat,new Color(.7f,.7f,.95f));
  Surface(entry,"Hub floor",69,81,-18,matLow,edgeMat);
  Surface(low,"Encounter 1 floor",84,98,-18,matLow,edgeMat);Surface(low,"Encounter 2 floor",101,115,-18,matLow,edgeMat);
  Surface(end,"End arena floor",118,136,-18,matLow,edgeMat);
  Surface(high,"Ascent 1",75,78,-16.6f,matHigh,edgeMat);Surface(high,"Ascent 2",79.5f,82.5f,-15.2f,matHigh,edgeMat);
  Surface(high,"High ledge 1",84,87,-14,matHigh,edgeMat);Surface(high,"High ledge 2",89,94,-14,matHigh,edgeMat);
  Surface(high,"Descent 1",96,100,-15.2f,matHigh,edgeMat);Surface(high,"Descent 2",102,105,-16.6f,matHigh,edgeMat);
  




  var entryPoint=Point(entry,"EntrySpawn",WhiteboxMarker.Role.Entry,72,-18);
  var exit=Point(end,"ReturnExit / E after story pickup",WhiteboxMarker.Role.ReturnExit,134,-18);
  var grate=LowerBox("ReturnExit visual",new Vector3(134,-16.6f,.4f),new Vector3(.15f,2.8f,.15f),edgeMat);grate.transform.SetParent(exit,true);
  var check=Point(low,"Checkpoint",WhiteboxMarker.Role.Checkpoint,103,-18);
  var beacon=LowerBox("Checkpoint visual",new Vector3(103,-16.5f,.7f),new Vector3(.12f,3,.12f),edgeMat);beacon.transform.SetParent(check,true);
  Point(end,"StoryItem / old archive",WhiteboxMarker.Role.StoryItem,131,-18);

  Encounter(low,"Encounter_01 / bypassable",90,-18,2.5f,false);
  Encounter(low,"Encounter_02 / flanking landing",108,-18,3,false);
  Encounter(end,"Encounter_03 / protects lever",125,-18,3,true);
  foreach(var zone in new[]{entry,low,high,end}){
   new GameObject("ART_SLOT / replace visual children").transform.SetParent(zone,false);
   new GameObject("AUDIO_SLOT / ambient and cues").transform.SetParent(zone,false);
   new GameObject("PROP_SLOT / non-blocking details").transform.SetParent(zone,false);
  }
  PrefabUtility.SaveAsPrefabAsset(levelRoot.gameObject,"Assets/Whitebox/Prefabs/LowerBlockout.prefab");
  var platformTemplate=Surface(null,"WB_Platform",-1.5f,1.5f,0,matLow,edgeMat);
  PrefabUtility.SaveAsPrefabAsset(platformTemplate.gameObject,"Assets/Whitebox/Prefabs/WB_Platform.prefab");Object.DestroyImmediate(platformTemplate.gameObject);
  var encounterTemplate=Encounter(null,"WB_Encounter",0,0,2,false);
  PrefabUtility.SaveAsPrefabAsset(encounterTemplate.gameObject,"Assets/Whitebox/Prefabs/WB_Encounter.prefab");Object.DestroyImmediate(encounterTemplate.gameObject);
  AssetDatabase.SaveAssets();
  System.IO.Directory.CreateDirectory("Assets/Scenes");EditorSceneManager.SaveScene(lowerScene,"Assets/Scenes/LowerPassage.unity");
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var camera=new GameObject("Side Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;camera.orthographicSize=3.8f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.075f,.095f,.12f);camera.transform.position=new Vector3(0,2,-10);
  new GameObject("Knight Level").AddComponent<KnightLevel>();
  System.IO.Directory.CreateDirectory("Assets/Scenes");EditorSceneManager.SaveScene(scene,"Assets/Scenes/DodgeStudy.unity");
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/DodgeStudy.unity",true),new EditorBuildSettingsScene("Assets/Scenes/LowerPassage.unity",true)};
  BuildCurrentPlayer();
 }
 // This is the normal build entry after teammates edit the scene. It never regenerates the blockout.
 public static void UpgradeStoryAndCombat(){
  if(AssetDatabase.LoadAssetAtPath<CombatTuning>("Assets/Resources/CombatTuning.asset")==null)AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CombatTuning>(),"Assets/Resources/CombatTuning.asset");
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/LowerPassage.unity",OpenSceneMode.Single);
  Transform level=null;foreach(var root in scene.GetRootGameObjects())if(root.name.StartsWith("LOWER_BLOCKOUT"))level=root.transform;
  if(level==null)throw new System.Exception("Lower blockout missing");
  var remove=new System.Collections.Generic.List<GameObject>();
  foreach(Transform zone in level){
   if(zone.name.StartsWith("05_ReturnLoop"))remove.Add(zone.gameObject);
   foreach(Transform child in zone)if(child.name.StartsWith("Return climb"))remove.Add(child.gameObject);
  }
  foreach(var obj in remove)Object.DestroyImmediate(obj);
  foreach(var marker in level.GetComponentsInChildren<WhiteboxMarker>(true)){
   if(marker.role==WhiteboxMarker.Role.ReturnExit)Object.DestroyImmediate(marker.gameObject);
   else if(marker.role==WhiteboxMarker.Role.StoryItem){marker.name="StoryItem / old archive";marker.transform.position=new Vector3(131,-18,0);var old=marker.transform.Find("LeverHandle");if(old!=null)Object.DestroyImmediate(old.gameObject);}
  }
  var end=level.Find("04_EndArena / guard + shortcut lever");
  if(end==null)foreach(Transform zone in level)if(zone.name.StartsWith("04_EndArena"))end=zone;
  end.name="04_EndArena / story archive + surface return";
  var exit=Point(end,"ReturnExit / E after story pickup",WhiteboxMarker.Role.ReturnExit,134,-18);
  var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Whitebox/Materials/Edge.mat");
  var pillar=LowerBox("ReturnExit visual",new Vector3(134,-16.6f,.4f),new Vector3(.15f,2.8f,.15f),mat);pillar.transform.SetParent(exit,true);
  PrefabUtility.SaveAsPrefabAsset(level.gameObject,"Assets/Whitebox/Prefabs/LowerBlockout.prefab");
  AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene);
  EditorSceneManager.OpenScene("Assets/Scenes/DodgeStudy.unity",OpenSceneMode.Single);BuildCurrentPlayer();
 }
 public static void UpgradeBoss(){
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/LowerPassage.unity",OpenSceneMode.Single);
  Transform level=null;foreach(var root in scene.GetRootGameObjects())if(root.name.StartsWith("LOWER_BLOCKOUT"))level=root.transform;
  Transform arena=null;foreach(Transform zone in level)if(zone.name.StartsWith("04_EndArena"))arena=zone;
  var body=AssetDatabase.LoadAssetAtPath<Material>("Assets/Whitebox/Materials/HighRoute.mat");
  var edge=AssetDatabase.LoadAssetAtPath<Material>("Assets/Whitebox/Materials/Edge.mat");
  foreach(var q in arena.GetComponentsInChildren<WhiteboxPlatform>())if(q.name.StartsWith("Boss tier"))Object.DestroyImmediate(q.gameObject);
  foreach(var q in arena.GetComponentsInChildren<WhiteboxPlatform>())if(q.name=="End arena floor"){q.transform.position=new Vector3(138,-18,0);q.transform.localScale=new Vector3(40,1,1);}
  Surface(arena,"Boss tier lower left",120,125,-15.8f,body,edge);Surface(arena,"Boss tier lower middle left",130,135,-15.8f,body,edge);Surface(arena,"Boss tier lower middle right",140,145,-15.8f,body,edge);Surface(arena,"Boss tier lower right",150,155,-15.8f,body,edge);
  Surface(arena,"Boss tier upper left",124,129,-13.6f,body,edge);Surface(arena,"Boss tier upper middle left",134,139,-13.6f,body,edge);Surface(arena,"Boss tier upper middle right",144,149,-13.6f,body,edge);Surface(arena,"Boss tier upper right",153,157,-13.6f,body,edge);
  foreach(var q in arena.GetComponentsInChildren<WhiteboxEncounter>())if(q.exitGuard){q.transform.position=new Vector3(136,-18,0);q.patrolHalfWidth=20;}
  foreach(var marker in arena.GetComponentsInChildren<WhiteboxMarker>()){
   if(marker.role==WhiteboxMarker.Role.StoryItem)marker.transform.position=new Vector3(153,-18,0);
   if(marker.role==WhiteboxMarker.Role.ReturnExit)marker.transform.position=new Vector3(156,-18,0);
  }
  var tuning=AssetDatabase.LoadAssetAtPath<CombatTuning>("Assets/Resources/CombatTuning.asset");tuning.bossHP=240;tuning.bossChargeDuration=1.1f;EditorUtility.SetDirty(tuning);
  EditorSceneManager.SaveScene(scene);PrefabUtility.SaveAsPrefabAsset(level.gameObject,"Assets/Whitebox/Prefabs/LowerBlockout.prefab");AssetDatabase.SaveAssets();
  EditorSceneManager.OpenScene("Assets/Scenes/DodgeStudy.unity",OpenSceneMode.Single);BuildCurrentPlayer();
 }
 public static void UpgradeLift(){
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/LowerPassage.unity",OpenSceneMode.Single);
  Transform level=null;foreach(var root in scene.GetRootGameObjects())if(root.name.StartsWith("LOWER_BLOCKOUT"))level=root.transform;
  Transform zone=null;foreach(Transform z in level)if(z.name.StartsWith("02_LowRoute"))zone=z;
  var old=zone.Find("Mandatory lift / scene module");if(old!=null)Object.DestroyImmediate(old.gameObject);
  var exit=zone.Find("Lift upper exit");if(exit!=null)Object.DestroyImmediate(exit.gameObject);
  var wall=zone.Find("Lift wall / must go over top");if(wall!=null)Object.DestroyImmediate(wall.gameObject);
  var body=AssetDatabase.LoadAssetAtPath<Material>("Assets/Whitebox/Materials/HighRoute.mat");var edge=AssetDatabase.LoadAssetAtPath<Material>("Assets/Whitebox/Materials/Edge.mat");
  Surface(zone,"Mandatory lift / scene module",110,113,-18,body,edge).gameObject.AddComponent<WhiteboxLift>();
  Surface(zone,"Lift upper exit",114,118.5f,-13.6f,body,edge);
  var block=LowerBox("Lift wall / must go over top",new Vector3(114.3f,-15.8f,0),new Vector3(.6f,4.4f,2.4f),body);block.transform.SetParent(zone,true);
  EditorSceneManager.SaveScene(scene);PrefabUtility.SaveAsPrefabAsset(level.gameObject,"Assets/Whitebox/Prefabs/LowerBlockout.prefab");AssetDatabase.SaveAssets();
  EditorSceneManager.OpenScene("Assets/Scenes/DodgeStudy.unity",OpenSceneMode.Single);BuildCurrentPlayer();
 }
 public static void UpgradeSpikes(){
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/DodgeStudy.unity",OpenSceneMode.Single);
  WhiteboxSpikes trap=null;foreach(var root in scene.GetRootGameObjects()){trap=root.GetComponentInChildren<WhiteboxSpikes>(true);if(trap!=null)break;}
  if(trap==null){var root=new GameObject("Upper middle / timed spikes / ART SLOT");trap=root.AddComponent<WhiteboxSpikes>();}
  trap.transform.position=new Vector3(51.3f,0,0);EditorUtility.SetDirty(trap);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();BuildCurrentPlayer();
 }
 public static void BuildExisting(){BuildCurrentPlayer();}
 [MenuItem("白模工作台/构建试玩（保留现有场景）")]
 public static void BuildFromMenu(){
  if(EditorApplication.isPlaying){Debug.LogError("请先停止 Play，再构建现有白模。");return;}
  EditorSceneManager.SaveOpenScenes();BuildCurrentPlayer();
 }
 static void BuildCurrentPlayer(){
  bool mac=Application.platform==RuntimePlatform.OSXEditor;
  var target=mac?BuildTarget.StandaloneOSX:BuildTarget.StandaloneWindows64;
  var destination=mac?"Build/DodgeStudy.app":"Build/DodgeStudy.exe";
  if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,target)){
   Debug.LogError("请在Unity Hub中为当前编辑器安装对应桌面平台的构建支持。");
   if(Application.isBatchMode)EditorApplication.Exit(1);return;
  }
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/DodgeStudy.unity","Assets/Scenes/LowerPassage.unity"},locationPathName=destination,target=target,options=BuildOptions.Development});
  Debug.Log("STUDY_BUILD_RESULT "+report.summary.result+" errors="+report.summary.totalErrors);
  if(report.summary.result!=BuildResult.Succeeded&&Application.isBatchMode)EditorApplication.Exit(1);
 }
 static Material ModuleMaterial(string name,Material original,Color color){
  string path="Assets/Whitebox/Materials/"+name+".mat";var result=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(result==null){result=new Material(original);AssetDatabase.CreateAsset(result,path);}result.color=color;return result;
 }
 static GameObject LowerBox(string name,Vector3 pos,Vector3 size,Material material){
  var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.position=pos;obj.transform.localScale=size;obj.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(obj.GetComponent<Collider>());return obj;
 }

 static Transform Zone(Transform parent,string name){var g=new GameObject(name).transform;g.SetParent(parent,false);return g;}
 static Transform Point(Transform parent,string name,WhiteboxMarker.Role role,float x,float y){var g=Zone(parent,name);g.position=new Vector3(x,y,0);g.gameObject.AddComponent<WhiteboxMarker>().role=role;return g;}
 static Transform Encounter(Transform parent,string name,float x,float y,float radius,bool endGuard){var g=Zone(parent,name);g.position=new Vector3(x,y,0);var c=g.gameObject.AddComponent<WhiteboxEncounter>();c.patrolHalfWidth=radius;c.exitGuard=endGuard;c.enemyPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Enemy.prefab");return g;}
 static Transform Surface(Transform parent,string name,float left,float right,float top,Material body,Material edge,bool shortcut=false){
  var g=Zone(parent,name);g.position=new Vector3((left+right)/2,top,0);g.gameObject.AddComponent<WhiteboxPlatform>().shortcutOnly=shortcut;
  var b=LowerBox("Visual_Block",Vector3.zero,new Vector3(1,.44f,2.4f),body);b.transform.SetParent(g,false);b.transform.localPosition=new Vector3(0,-.22f,0);
  var e=LowerBox("Visual_Edge",Vector3.zero,new Vector3(1,.04f,2.45f),edge);e.transform.SetParent(g,false);e.transform.localPosition=new Vector3(0,-.015f,-.03f);
  g.localScale=new Vector3(right-left,1,1);return g;
 }

}
