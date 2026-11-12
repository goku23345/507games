using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public partial class KnightLevel : MonoBehaviour {
 [System.Serializable] public struct Platform {public float left,right,top;public Platform(float l,float r,float y){left=l;right=r;top=y;}}
 public Platform[] platforms={
 new Platform(-3,12,0),new Platform(14,25,0),new Platform(29,43,0),new Platform(47,78,0),new Platform(81,94,0),new Platform(98,110,0),new Platform(114,130,0),
 new Platform(5,8,1.45f),new Platform(9,12,2.9f),new Platform(14,17,2.9f),new Platform(18,21,1.45f),
 new Platform(30.5f,33.5f,1.4f),new Platform(35,38,2.8f),new Platform(39.5f,42,4.15f),new Platform(44,46,2.8f),
 new Platform(49,51.5f,1.35f),new Platform(53,55.5f,2.7f),
 new Platform(63,66,1.4f),new Platform(67.5f,70.5f,2.8f),new Platform(72,75,4.2f),new Platform(76.5f,79,2.8f),new Platform(83,85,1.4f),
 new Platform(99,101.5f,1.4f),new Platform(103,106,2.8f),new Platform(108,111,4.2f),new Platform(112.5f,115,2.8f),new Platform(117,119.5f,1.4f)};
 [System.Serializable] public struct CoinSpot {public float x,y;public bool bonus;public CoinSpot(float xx,float yy,bool b=false){x=xx;y=yy;bonus=b;}}
 // A small collection study in the opening section: 6 guide coins and 8 upper-route bonuses.
 public CoinSpot[] coinLayout={
 new CoinSpot(2,.85f),new CoinSpot(4,.85f),new CoinSpot(10.2f,.85f),new CoinSpot(15.8f,.85f),new CoinSpot(17.8f,.85f),new CoinSpot(22.5f,.85f),
 new CoinSpot(6.6f,2.3f,true),new CoinSpot(7.5f,2.3f,true),new CoinSpot(10.3f,3.75f,true),new CoinSpot(11.4f,3.75f,true),
 new CoinSpot(15.1f,3.75f,true),new CoinSpot(16.3f,3.75f,true),new CoinSpot(19.2f,2.3f,true),new CoinSpot(20.3f,2.3f,true),new CoinSpot(80.5f,-14.35f,true),new CoinSpot(90.5f,-13.15f,true),new CoinSpot(104,-15.75f,true)};
 class WorldCoin {public int id;public CoinSpot spot;public bool taken;public Transform shape;}
 readonly List<WorldCoin> coins=new List<WorldCoin>();int coinsTaken;float coinFeedback;Vector3 coinFeedbackPosition;
 class Foe {public float x,home,y,left,right,t;public int hp=60,maxHP=60,face=-1,state;public Transform root,warning;public Animator anim;public bool ramHit,exitGuard;public int bossMove,bossCycle;public float targetX,targetY;}
 List<Foe> foes=new List<Foe>(); List<Transform> gates=new List<Transform>(),beacons=new List<Transform>();
 public float speed=5.2f,jumpSpeed=11.5f,gravity=24;
 float x=0,y=0,vy=0,roll=-1,atk=-1,inv=0,coyote=0,buffer=0,clock=0;
 int face=1,hp=100,checkpoint=0,furthestCheckpoint=0,combo=0,falls=0,kills=0; bool cameraProbe; bool grounded=true,started,won,verify,paused,slow;
 SpriteRenderer hero;Animator heroAnim;string animState="";Font font;float accumulator;float inputMove;bool jumpPressed,rollPressed,attackPressed,jumpHeld,interactPressed;string message="高低两路均可前进；红色预兆后闪避，敌人恢复时反击。";
 float[] checkpointX={0,23,40,62,91,108,125};
 float previousX,previousY; Camera cam;
 bool doorOpen; float doorEffect; int ramMisses,ramImpacts;
 readonly List<Transform> doorPlanks=new List<Transform>(); Transform baitMark,doorSignal;
 const float RamDoorX=24, RamSpeed=9.5f, RamWindup=1.05f;
 // One monotonic quest state per run: 0=key at end, 1=carried, 2=delivered.
 int relayStage,corePickups,relayDeliveries,passedChecks,failedChecks;
 bool hiddenRouteVisited;float relayFeedback;
 const float RelayX=68.8f,CoreX=125.7f,FinalGateX=127.3f;
 Transform relayScreen,finalGate,keyWorld,keyCarried;
 readonly List<Transform> hiddenVisuals=new List<Transform>();
 Platform[] lowerPlatforms=System.Array.Empty<Platform>(); // Collision cache comes from authored WhiteboxPlatform modules.
 bool bossEngaged;Transform bossGate;
 bool inLower,shaftCommitted,lowerCheckpointLit,lowerCleared,storyItemOwned,lowerPrepared;
 float lowerEntryX=72,lowerCheckpointX=103,lowerStoryX=153,lowerReturnX=156;int storyPickups;
 Transform storyVisual,storyCarried;CombatTuning tuning;bool showHitboxes;readonly HashSet<Foe> swingHits=new HashSet<Foe>();
 readonly List<WhiteboxPlatform> lowerModules=new List<WhiteboxPlatform>();
 float lowerRespawnX=72,transitionFade;int lowerEntries,lowerDeaths;
 readonly List<Foe> lowerFoes=new List<Foe>();readonly List<Transform> hatchVisuals=new List<Transform>();
 string RootPath {get{return Application.dataPath+"/../../";}}
 Material Mat(Color c){var m=new Material(Resources.Load<Material>("Block"));m.color=c;return m;}
 Transform Box(string name,Vector3 pos,Vector3 size,Color c){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;Destroy(o.GetComponent<Collider>());o.transform.position=pos;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=Mat(c);return o.transform;}
 void Start(){
  tuning=Resources.Load<CombatTuning>("CombatTuning");if(tuning==null)tuning=ScriptableObject.CreateInstance<CombatTuning>();speed=tuning.walkSpeed;jumpSpeed=tuning.jumpVelocity;gravity=tuning.gravity;
  cameraProbe=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-camera-probe")>=0; Application.targetFrameRate=60;Time.timeScale=1;verify=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-verify-level")>=0;
  font=Font.CreateDynamicFontFromOSFont("Microsoft YaHei",18);cam=Camera.main;cam.orthographicSize=5.625f;cam.backgroundColor=new Color(.07f,.1f,.14f);
  RenderSettings.ambientLight=new Color(.7f,.76f,.82f);
  var light=new GameObject("Key light").AddComponent<Light>();light.type=LightType.Directional;light.transform.rotation=Quaternion.Euler(35,-25,0);
  // Chunk data above is the editable map. Thin platforms are one-way from below.
  foreach(var p in platforms){
   if(p.left==47&&p.right==78){BuildSurface(new Platform(47,71,0));BuildSurface(new Platform(74,78,0));
    hatchVisuals.Add(Box("Closed hidden hatch",new Vector3(72.5f,-.22f,0),new Vector3(3,.44f,2.4f),new Color(.24f,.31f,.38f)));
    hatchVisuals.Add(Box("Hatch closed edge",new Vector3(72.5f,-.015f,-.02f),new Vector3(3,.04f,2.45f),new Color(.48f,.58f,.64f)));
   }else BuildSurface(p);
  }
  for(int i=0;i<checkpointX.Length;i++){float xx=checkpointX[i];beacons.Add(Box("Checkpoint "+i,new Vector3(xx,2,1),new Vector3(.15f,4,.15f),new Color(.22f,.55f,.58f)));}
  float[] homes={19.5f,37.5f,56.5f,87,104,121};float[] ls={15,30,48,82,99,115};float[] rs={23.25f,40.5f,59.5f,90,107,124};float[] gs={24,41.5f,60.5f,92,109,126};
  for(int i=0;i<homes.Length;i++){var e=new Foe{x=homes[i],home=homes[i],left=ls[i],right=rs[i]};e.root=Instantiate(Resources.Load<GameObject>("Enemy")).transform;e.anim=e.root.GetComponentInChildren<Animator>();e.warning=Box("Enemy telegraph",Vector3.zero,new Vector3(2.7f,.05f,1.3f),new Color(.9f,.2f,.12f));e.hp=e.maxHP=tuning.enemyHP;foes.Add(e);gates.Add(Box("Encounter gate "+i,new Vector3(gs[i],2.5f,0),new Vector3(.16f,5,2),new Color(.8f,.43f,.2f)));}
  // The first exit is a physical ram door; remaining encounter gates keep their original rules.
  gates[0].GetComponent<Renderer>().enabled=false;
  for(int i=0;i<4;i++)doorPlanks.Add(Box("Ram door / breakable plank "+i,new Vector3(RamDoorX,.55f+i*1.1f,-1.55f),new Vector3(.62f,1.03f,.28f),new Color(.64f,.34f,.15f)));
  baitMark=Box("Bait position / stand before door",new Vector3(22.9f,.035f,-1.6f),new Vector3(1.1f,.05f,.65f),new Color(.95f,.69f,.2f));
  doorSignal=Box("Ram door status",new Vector3(RamDoorX,.04f,-1.6f),new Vector3(1.25f,.06f,.8f),new Color(.9f,.45f,.12f));
  for(int i=0;i<coinLayout.Length;i++){
   var o=GameObject.CreatePrimitive(PrimitiveType.Cylinder);o.name="Coin "+i+(coinLayout[i].bonus?" / upper reward":" / main route");
   Destroy(o.GetComponent<Collider>());o.transform.localScale=new Vector3(.36f,.055f,.36f);o.transform.rotation=Quaternion.Euler(90,0,0);
   var gold=Mat(new Color(1,.72f,.08f));gold.EnableKeyword("_EMISSION");gold.SetColor("_EmissionColor",new Color(.5f,.27f,.015f));o.GetComponent<Renderer>().sharedMaterial=gold;
   coins.Add(new WorldCoin{id=i,spot=coinLayout[i],shape=o.transform});
  }
  Box("Exit frame",new Vector3(128,1.6f,.6f),new Vector3(.3f,3.2f,.5f),new Color(.35f,.9f,.62f));
  Box("Exit top",new Vector3(127.3f,3.2f,.6f),new Vector3(1.7f,.2f,.5f),new Color(.35f,.9f,.62f));
  finalGate=Box("Final locked door / return-to-relay",new Vector3(FinalGateX,1.6f,-1.85f),new Vector3(1.0f,3.2f,.4f),new Color(.8f,.24f,.17f));
  keyWorld=Box("Signal key / found at end",new Vector3(CoreX,.95f,-2.1f),new Vector3(.4f,.58f,.2f),new Color(.2f,.9f,1));
  keyCarried=Box("Carried signal key",Vector3.zero,new Vector3(.24f,.36f,.15f),new Color(.2f,.9f,1));
  Box("Middle relay console",new Vector3(RelayX,.65f,-1.9f),new Vector3(.75f,1.3f,.5f),new Color(.22f,.28f,.34f));
  relayScreen=Box("Relay screen",new Vector3(RelayX,.9f,-2.2f),new Vector3(.5f,.42f,.06f),new Color(.8f,.45f,.1f));
  hiddenVisuals.Add(Box("Safe shaft left guide",new Vector3(71,-1.5f,-1.4f),new Vector3(.08f,3,.08f),new Color(.5f,.9f,1)));
  hiddenVisuals.Add(Box("Safe shaft right guide",new Vector3(74,-1.5f,-1.4f),new Vector3(.08f,3,.08f),new Color(.5f,.9f,1)));
  if(!SceneManager.GetSceneByName("LowerPassage").isLoaded)SceneManager.LoadScene("LowerPassage",LoadSceneMode.Additive);
  // Sparse background columns make depth visible without obstructing platform edges.
  for(int i=0;i<35;i++)Box("Background pylon",new Vector3(i*4-3,-1,4),new Vector3(.6f,5+(i%3)*.4f,1),new Color(.06f,.09f,.12f));
  var player=Instantiate(Resources.Load<GameObject>("Knight"));hero=player.GetComponent<SpriteRenderer>();heroAnim=player.GetComponent<Animator>();hero.sortingOrder=10;
  PrepareSpikes();ResetAll();if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-verify-spikes")>=0){verify=true;StartCoroutine(VerifySpikes());return;}if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-verify-lift")>=0){verify=true;StartCoroutine(VerifyLift());return;}bool bossVerify=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-verify-boss")>=0;started=verify||cameraProbe||bossVerify;if(bossVerify){verify=true;StartCoroutine(VerifyBoss());return;}Visual(0);if(cameraProbe){verify=true;StartCoroutine(VerifyCamera());}else if(verify)StartCoroutine(VerifyLevel());if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-boss-preview")>=0){verify=false;started=true;relayStage=2;EnterLower();x=119.5f;y=-18;vy=0;grounded=true;SyncPresentation();}
 }
 void BuildSurface(Platform p){
  bool upper=p.top>0;Box(upper?"Upper route / one-way":"Ground island",new Vector3((p.left+p.right)/2,p.top-.22f,0),new Vector3(p.right-p.left,.44f,2.4f),upper?new Color(.29f,.43f,.49f):new Color(.24f,.31f,.38f));
  Box("Landing edge",new Vector3((p.left+p.right)/2,p.top-.015f,-.02f),new Vector3(p.right-p.left,.04f,2.45f),upper?new Color(.58f,.8f,.74f):new Color(.48f,.58f,.64f));
 }
 void PrepareLower(){
  if(lowerPrepared)return;
  var scene=SceneManager.GetSceneByName("LowerPassage");if(!scene.isLoaded)return;
  foreach(var root in scene.GetRootGameObjects()){
   lowerModules.AddRange(root.GetComponentsInChildren<WhiteboxPlatform>(true));
   foreach(var marker in root.GetComponentsInChildren<WhiteboxMarker>(true)){
    if(marker.role==WhiteboxMarker.Role.Entry)lowerEntryX=marker.transform.position.x;
    if(marker.role==WhiteboxMarker.Role.Checkpoint)lowerCheckpointX=marker.transform.position.x;
    if(marker.role==WhiteboxMarker.Role.StoryItem){lowerStoryX=marker.transform.position.x;}
    if(marker.role==WhiteboxMarker.Role.ReturnExit){lowerReturnX=marker.transform.position.x;}
   }
   foreach(var spawn in root.GetComponentsInChildren<WhiteboxEncounter>(true)){
    var pos=spawn.transform.position;var foe=new Foe{x=pos.x,home=pos.x,y=pos.y,left=pos.x-spawn.patrolHalfWidth,right=pos.x+spawn.patrolHalfWidth,exitGuard=spawn.exitGuard};foe.hp=foe.maxHP=spawn.exitGuard?tuning.bossHP:tuning.enemyHP;
    var prefab=spawn.enemyPrefab!=null?spawn.enemyPrefab:Resources.Load<GameObject>("Enemy");
    foe.root=Instantiate(prefab,spawn.transform).transform;foe.root.name="Runtime enemy / replace prefab on spawn";foe.anim=foe.root.GetComponentInChildren<Animator>();
    foe.warning=Box("Lower enemy telegraph",Vector3.zero,new Vector3(2.7f,.05f,1.3f),new Color(.9f,.2f,.12f));
    SceneManager.MoveGameObjectToScene(foe.warning.gameObject,scene);lowerFoes.Add(foe);
   }
  }
  storyVisual=Box("剧情道具 / 旧日记录匣",new Vector3(lowerStoryX,-17.1f,-2.2f),new Vector3(.55f,.45f,.25f),new Color(1,.8f,.25f));
  storyCarried=Box("Carried story archive",Vector3.zero,new Vector3(.26f,.23f,.15f),new Color(1,.8f,.25f));
  bossGate=Box("Boss arena seal",new Vector3(118,-14,0),new Vector3(.15f,8,.2f),new Color(1,.3f,.15f));
  lowerPrepared=true;PrepareLift();RefreshLowerSurfaces();
 }
 void RefreshLowerSurfaces(){
  var list=new List<Platform>();foreach(var module in lowerModules){bool on=!module.shortcutOnly;module.gameObject.SetActive(on);if(on)list.Add(module.Surface);}lowerPlatforms=list.ToArray();
 }
 void CollectStoryItem(){
  if(!inLower||storyItemOwned||!lowerCleared||Mathf.Abs(x-lowerStoryX)>.75f||Mathf.Abs(y+18)>.8f)return;
  storyItemOwned=true;storyPickups++;lowerRespawnX=lowerStoryX;
  message="获得剧情道具：旧日记录匣 ×1。到末端蓝色出口按 E 返回上层。";
  Debug.Log("STORY_ITEM_PICKED count="+storyPickups);
 }
 void UseLowerInteraction(){LeaveLower();}
 void EnterLower(){
  PrepareLower();inLower=true;shaftCommitted=false;lowerEntries++;x=lowerEntryX;y=-14;vy=-4;grounded=false;roll=atk=-1;transitionFade=.35f;SyncPresentation();
  var scene=SceneManager.GetSceneByName("LowerPassage");
  if(scene.isLoaded)Debug.Log("LOWER_SCENE_READY modules="+lowerModules.Count);
  message="安全落地：低路战斗，高路探索；末端多层Boss场取记录匣，蓝色出口按E返回。";
  Debug.Log("LOWER_ENTER safe=true hp="+hp+" falls="+falls);
 }
 void LeaveLower(){
  if(Mathf.Abs(x-lowerReturnX)>1.3f||Mathf.Abs(y+18)>.8f){message="到地下末端156米蓝色出口旁按 E 返回上层。";return;}
  if(!storyItemOwned){message="先击败末端守卫，并靠近金色记录匣取得剧情道具，再返回。";return;}
  inLower=false;hiddenRouteVisited=true;x=75.2f;y=0;vy=0;grounded=true;roll=atk=-1;transitionFade=.35f;SyncPresentation();
  message="携带旧日记录匣返回上层，终点门仍开启，继续向右。";Debug.Log("STORY_RETURN_SURFACE");
 }
 void Play(string state){if(animState==state)return;animState=state;heroAnim.Play(state,0,0);}
 void ResetAll(){spikeTime=0;spikeHitCycle=-1;liftTime=0;liftCrossed=false;if(lift!=null){var lp=lift.transform.position;lp.y=lift.bottom;lift.transform.position=lp;}bossEngaged=false;x=0;y=0;vy=0;hp=tuning.playerHP;roll=atk=-1;inv=0;checkpoint=0;furthestCheckpoint=0;kills=0;falls=0;won=false;paused=false;face=1;grounded=true;coyote=buffer=0;started=true;Time.timeScale=1;slow=false;doorOpen=false;doorEffect=0;ramMisses=ramImpacts=0;relayStage=corePickups=relayDeliveries=0;inLower=shaftCommitted=lowerCheckpointLit=lowerCleared=false;storyItemOwned=false;storyPickups=0;lowerRespawnX=lowerEntryX;if(lowerPrepared)RefreshLowerSurfaces();lowerEntries=lowerDeaths=0;transitionFade=0;foreach(var e in lowerFoes){e.hp=e.maxHP;e.x=e.home;if(e.exitGuard){e.y=-18;e.bossCycle=0;}e.state=0;e.t=0;e.anim.Play("Idle",0,0);}hiddenRouteVisited=false;relayFeedback=0;coinsTaken=0;coinFeedback=0;foreach(var coin in coins){coin.taken=false;coin.shape.gameObject.SetActive(true);}SyncPresentation();
  foreach(var e in foes){e.x=e.home;e.y=0;e.hp=e.maxHP;e.state=0;e.t=0;e.anim.Play("Idle",0,0);}message="第一道木门：到金色站位，引守卫朝门蓄力，再跳开或 J 翻滚。"; }
 void Respawn(){
  if(inLower){bossEngaged=false;falls++;lowerDeaths++;shaftCommitted=false;x=lowerRespawnX;y=-18;vy=0;hp=tuning.playerHP;roll=atk=-1;inv=1.2f;grounded=true;coyote=buffer=0;SyncPresentation();
   foreach(var e in lowerFoes)if(e.hp>0){e.x=e.home;if(e.exitGuard){e.y=-18;e.bossCycle=0;}e.hp=e.maxHP;e.state=0;e.t=0;e.anim.Play("Idle",0,0);}
   message="回到下层检查点，钥匙交付、终门与上层进度保持不变。";Debug.Log("LOWER_RESPAWN x="+x);return;
  }
  shaftCommitted=false;falls++;x=checkpointX[checkpoint];y=0;vy=0;hp=tuning.playerHP;roll=atk=-1;inv=1.2f;grounded=true;coyote=buffer=0;SyncPresentation();
  foreach(var e in foes)if(e.hp>0||(!doorOpen&&e==foes[0])){e.x=e.home;if(e.exitGuard){e.y=-18;e.bossCycle=0;}e.hp=e.maxHP;e.state=0;e.t=0;e.anim.Play("Idle",0,0);}
  message="回到检查点 "+checkpoint+"，生命已恢复。";Debug.Log("RESPAWN checkpoint="+checkpoint+" x="+x); }
 Rect PlayerBody(){return new Rect(x-tuning.playerHalfWidth,y+tuning.playerBodyBottom,tuning.playerHalfWidth*2,tuning.playerBodyHeight);}
 Rect EnemyBody(Foe e){return new Rect(e.x-tuning.enemyHalfWidth,e.y+tuning.enemyBodyBottom,tuning.enemyHalfWidth*2,tuning.enemyBodyHeight);}
 Rect PlayerAttack(){return new Rect(face>0?x+tuning.attackNear:x-tuning.attackReach,y+tuning.attackBottom,tuning.attackReach-tuning.attackNear,tuning.attackHeight);}
 Rect EnemyAttack(Foe e){return new Rect(e.face>0?e.x+tuning.enemyAttackNear:e.x-tuning.enemyAttackReach,e.y+tuning.enemyAttackBottom,tuning.enemyAttackReach-tuning.enemyAttackNear,tuning.enemyAttackHeight);}
 bool PlayerOverlaps(Foe e){return (e.x-x)*face>=0&&PlayerAttack().Overlaps(EnemyBody(e));}
 bool EnemyOverlaps(Foe e){return (x-e.x)*e.face>=0&&EnemyAttack(e).Overlaps(PlayerBody());}
 bool DodgeInvulnerable(){return roll>=tuning.dodgeInvStart&&roll<=tuning.dodgeInvEnd;}
 void Damage(Foe e,bool ram=false){
  if(inv>0||DodgeInvulnerable())return;
  if(ram){if(!new Rect(x-tuning.playerHalfWidth,y+tuning.playerBodyBottom,tuning.playerHalfWidth*2,tuning.playerBodyHeight).Overlaps(new Rect(x-1,e.y+.1f,2,1.8f)))return;}
  else if(!EnemyOverlaps(e))return;
  int amount=ram?tuning.ramDamage:e.exitGuard?tuning.bossDamage:tuning.enemyDamage;
  hp=Mathf.Max(0,hp-amount);inv=tuning.hitInvulnerability;message="受伤 -"+amount+"，生命 "+hp+"/"+tuning.playerHP;Debug.Log("HERO_HIT damage="+amount+" hp="+hp);
  if(hp<=0){Play("Death");message="倒下了。Enter 检查点复活，R 全关重开。";}
 }
 void AttackHit(){
  if(atk<tuning.attackWindup||atk>tuning.attackActiveEnd)return;
  foreach(var e in inLower?lowerFoes:foes){
   if(e.hp<=0||swingHits.Contains(e)||!PlayerOverlaps(e))continue;
   swingHits.Add(e);bool counter=e.state==3;int amount=combo==3?tuning.thirdAttackDamage:tuning.attackDamage;if(counter)amount=Mathf.RoundToInt(amount*tuning.counterMultiplier);
   e.hp=Mathf.Max(0,e.hp-amount);message=(counter?"反击":"剑击")+" -"+amount+"，敌人 "+e.hp+"/"+e.maxHP;Debug.Log("PLAYER_HIT_DAMAGE "+amount+" enemyHp="+e.hp);
   if(e.hp<=0){
    e.t=0;e.anim.CrossFadeInFixedTime("Fall",.1f);
    if(e==foes[0]&&!doorOpen){e.state=5;message="守门者会重新起身，木门仍需扑击撞开。";}
    else{e.state=4;if(inLower){if(e.exitGuard)lowerCleared=true;message=e.exitGuard?"守卫倒下，末端记录匣已解封。":"敌人倒下，继续探索。";}else kills++;}
   }
  }
 }
 void Tick(float dt,float move,bool jump,bool held,bool dodge,bool attack,bool interact=false){
  clock+=dt;transitionFade=Mathf.Max(0,transitionFade-dt);if(!paused)relayFeedback=Mathf.Max(0,relayFeedback-dt);inv=Mathf.Max(0,inv-dt);if(!paused)coinFeedback=Mathf.Max(0,coinFeedback-dt);if(doorOpen&&!paused)doorEffect=Mathf.Min(1,doorEffect+dt);
  if(!started||won||hp<=0||paused)return;previousX=x;previousY=y;TickLift(dt);
  if(jump)buffer=.14f;else buffer=Mathf.Max(0,buffer-dt);
  coyote=grounded?.12f:Mathf.Max(0,coyote-dt);
  if(Mathf.Abs(move)>.05f&&roll<0){face=move>0?1:-1;}
  if(dodge&&roll<0&&atk<0){roll=0;message="翻滚避开，留意前面的落点。";Debug.Log("ROLL_STARTED");}
  if(attack&&atk<0&&roll<0){atk=0;swingHits.Clear();combo=combo%3+1;}
  if(buffer>0&&coyote>0&&roll<0){vy=jumpSpeed;grounded=false;coyote=buffer=0;Debug.Log("JUMP_STARTED");}
  float dx=(roll>=0?face*tuning.dodgeSpeed:move*speed)*dt;x=Mathf.Clamp(x+dx,inLower?69:-1.5f,inLower?158:129);
  if(roll>=0){roll+=dt;if(roll>=tuning.dodgeDuration)roll=-1;}
  if(atk>=0){atk+=dt;if(atk>=tuning.attackWindup&&atk<=tuning.attackActiveEnd)AttackHit();if(atk>(combo==3?tuning.thirdAttackDuration:tuning.attackDuration))atk=-1;}
  // Each uncleared arena exit is a full-height gate, so upper routes cannot bypass encounters.
  for(int i=0;!inLower&&i<foes.Count;i++)if((i==0?!doorOpen:foes[i].hp>0)&&x>gates[i].position.x-.5f&&x<gates[i].position.x+1){x=gates[i].position.x-.5f;}
  if(!inLower&&relayStage<2&&x>FinalGateX-.85f)x=FinalGateX-.85f;
  float oldY=y;vy-=gravity*dt*(vy>0&&!held?1.8f:1);vy=Mathf.Max(vy,-20);y+=vy*dt;grounded=false;
  if(vy<=0)foreach(var p in inLower?lowerPlatforms:platforms){
   if(!inLower&&relayStage==2&&p.left==47&&p.right==78&&x>71&&x<74)continue;
   if(x>p.left-.22f&&x<p.right+.22f&&oldY>=p.top-.03f&&y<=p.top){y=p.top;vy=0;grounded=true;break;}
  }
  if(!inLower&&relayStage==2&&x>71&&x<74&&y<-.35f&&vy<0)shaftCommitted=true;
  if(!inLower&&shaftCommitted&&y<-2){EnterLower();return;}
  if(y<(inLower?-24:-6)){Debug.Log("FALL_POS x="+x+" y="+y+" lift="+(lift==null?999:lift.transform.position.y));Respawn();return;}
  TickSpikes(dt);if(hp<=0)return;
  LiftWall();
  if(inLower&&bossEngaged&&!lowerCleared)x=Mathf.Clamp(x,118.5f,157.5f);
  CollectCoins();
  if(inLower){
   if(x>=lowerCheckpointX&&!lowerCheckpointLit){lowerCheckpointLit=true;lowerRespawnX=lowerCheckpointX;message="下层中途检查点已启用。";Debug.Log("LOWER_CHECKPOINT");}
   CollectStoryItem();
   if(interact)UseLowerInteraction();
  }else{
   CollectRelayKey();if(interact)UseRelay();
  }
  for(int i=1;!inLower&&i<checkpointX.Length;i++)if(checkpoint!=i&&Mathf.Abs(x-checkpointX[i])<.7f&&kills>=i){
   bool firstVisit=i>furthestCheckpoint;checkpoint=i;furthestCheckpoint=Mathf.Max(furthestCheckpoint,i);
   if(firstVisit)hp=tuning.playerHP;
   message=firstVisit?"检查点 "+i+" 已点亮，生命恢复。":"复活点已切换到 "+Mathf.RoundToInt(checkpointX[i])+" 米。";
   Debug.Log("CHECKPOINT "+checkpoint+" firstVisit="+firstVisit);
  }
  foreach(var e in inLower?lowerFoes:foes){
   if(!inLower&&e==foes[0]&&!doorOpen){TickRamGuard(e,dt);continue;}
   if(e.hp<=0)continue;
   if(inLower&&e.exitGuard){TickBoss(e,dt);continue;}
   float dist=Mathf.Abs(e.x-x);if(dist>9)continue;e.t+=dt;
   if(e.state==0){
    e.face=x>e.x?1:-1;
    if(dist>1.65f){e.x=Mathf.Clamp(e.x+e.face*tuning.enemyChase*dt,e.left,e.right);if(e.anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))e.anim.CrossFadeInFixedTime("Walk",.1f);}
    else if(Mathf.Abs(y-e.y)<1.1f){e.state=1;e.t=0;e.anim.CrossFadeInFixedTime("Idle",.1f);message="红色预兆！J 翻滚或跳离攻击范围。";}
   }else if(e.state==1&&e.t>=tuning.enemyWindup){e.state=2;e.t=0;e.ramHit=false;e.anim.CrossFadeInFixedTime("Attack",.08f);}
   else if(e.state==2){
    if(!e.ramHit&&e.t<=tuning.enemyActive&&EnemyOverlaps(e)){e.ramHit=true;Damage(e);}
    if(e.t>=tuning.enemyActive){e.state=3;e.t=0;}
   }else if(e.state==3&&e.t>=tuning.enemyRecovery){e.state=0;e.t=0;e.anim.CrossFadeInFixedTime("Walk",.1f);}

  }
  if(!inLower&&x>=128&&kills==6&&relayStage==2){won=true;message="抵达出口！全关卡完成。R 从头再挑战。";Debug.Log("LEVEL_COMPLETE checkpoints="+checkpoint+" kills="+kills);}
 }
 void CollectCoins(){
  if(hp<=0)return;
  foreach(var coin in coins){
   if(coin.taken||Mathf.Abs(x-coin.spot.x)>.55f||coin.spot.y<y+.15f||coin.spot.y>y+1.65f)continue;
   coin.taken=true;coin.shape.gameObject.SetActive(false);coinsTaken++;coinFeedback=.7f;
   coinFeedbackPosition=new Vector3(coin.spot.x,coin.spot.y,-2.1f);
   Debug.Log("COIN_COLLECTED id="+coin.id+" bonus="+coin.spot.bonus+" total="+coinsTaken);
  }
 }
 int BonusCoinsTotal(){int n=0;foreach(var coin in coins)if(coin.spot.bonus)n++;return n;}
 int LowerCoinsCollected(){int n=0;foreach(var coin in coins)if(coin.taken&&coin.spot.y<0)n++;return n;}
 int BonusCoinsCollected(){int n=0;foreach(var coin in coins)if(coin.taken&&coin.spot.bonus)n++;return n;}
 bool CoinStateConsistent(){
  int n=0;foreach(var coin in coins){if(coin.taken)n++;if(coin.shape.gameObject.activeSelf==coin.taken)return false;}return n==coinsTaken;
 }
 void CollectRelayKey(){
  if(relayStage!=0||Mathf.Abs(x-CoreX)>.55f||y>.8f)return;
  relayStage=1;corePickups++;relayFeedback=2;
  message="拿到信号钥匙！带它返回 69 米的中继台，按 E 打开终点门并显现隐径。";
  Debug.Log("RELAY_KEY_PICKED x="+x+" count="+corePickups);
 }
 void UseRelay(){
  if(Mathf.Abs(x-RelayX)>1.25f||Mathf.Abs(y)>.75f){message="请回到 69 米中继台旁的地面，再按 E 交付。";return;}
  if(relayStage==0){message="中继台缺少信号钥匙。钥匙在终点锁门旁，拿到后返回这里按 E。";return;}
  if(relayStage==2){message="钥匙已经交付：终点门保持打开，下层入口已显露，不会重复领取或计数。";return;}
  relayStage=2;relayDeliveries++;relayFeedback=3;checkpoint=3;furthestCheckpoint=Mathf.Max(furthestCheckpoint,3);
  message="交付完成！终点门已打开，72米安全缺口已显露。跳下可进入下层关卡，再从下层出口返回。";
  Debug.Log("RELAY_DELIVERED count="+relayDeliveries+" lowerLevel=LowerPassage");
 }
 string RelayStatus(){return relayStage==0?"钥匙在终点门旁":relayStage==1?"携带钥匙 ← 返回69米按 E":"终门已开 → 下层"+(hiddenRouteVisited?"已完成":"入口已显露");}
 bool RelayConsistent(){
  if(relayStage<0||relayStage>2||corePickups>1||relayDeliveries>1)return false;
  if(keyWorld.gameObject.activeSelf!=(relayStage==0)||keyCarried.gameObject.activeSelf!=(relayStage==1)||finalGate.gameObject.activeSelf!=(relayStage<2))return false;
  foreach(var v in hiddenVisuals)if(v.gameObject.activeSelf!=(relayStage==2))return false;
  return relayStage<2||relayDeliveries==1;
 }
 void OpenRamDoor(Foe e){
  if(doorOpen)return;
  doorOpen=true;doorEffect=0;ramImpacts++;e.hp=0;e.state=4;e.t=0;kills++;
  e.anim.CrossFadeInFixedTime("Fall",.08f);
  message="轰！木门撞开了，守卫倒地。向右通过，检查点会保留开门状态。";
  Debug.Log("RAM_DOOR_OPEN enemyX="+e.x+" impacts="+ramImpacts+" kills="+kills);
 }
 void RamMiss(Foe e){
  e.state=3;e.t=0;ramMisses++;e.anim.CrossFadeInFixedTime("Idle",.1f);
  message="它扑空了，还能再引一次：站到门左的金色位置，等它朝右蓄力后闪开。";
  Debug.Log("RAM_MISSED retry="+ramMisses);
 }
 void TickRamGuard(Foe e,float dt){
  e.t+=dt;
  if(e.state==5){
   if(e.t>=2){e.hp=e.maxHP;e.state=0;e.t=0;e.anim.CrossFadeInFixedTime("Idle",.12f);message="守门者爬起来了，再诱它朝木门扑击。";Debug.Log("RAM_GUARD_RECOVERED");}
   return;
  }
  if(e.hp<=0)return;
  if(e.state==0){
   if(Mathf.Abs(x-e.x)>10)return;
   e.face=x>=e.x?1:-1;
   if(Mathf.Abs(x-e.x)>4.2f||Mathf.Abs(y-e.y)>1.1f){
    e.x=Mathf.Clamp(e.x+e.face*1.65f*dt,e.left,e.right);
    if(!e.anim.GetCurrentAnimatorStateInfo(0).IsName("Walk"))e.anim.CrossFadeInFixedTime("Walk",.1f);
   }else{
    // Lock direction now: dodging behind the guard will not retarget the charge.
    e.state=1;e.t=0;e.ramHit=false;e.anim.CrossFadeInFixedTime("Idle",.1f);
    message=e.face>0?"守卫朝门蓄力：先等地面闪红，再 A+J 向左翻滚，或 Space 跳开。":"它朝左蓄力了。绕到它右侧的金色站位，再引一次。";
    Debug.Log("RAM_WINDUP lockedDirection="+e.face+" heroX="+x+" enemyX="+e.x);
   }
  }else if(e.state==1){
   if(e.face>0&&e.t>=.7f)message="现在闪开！按住 A 再按 J 向左翻滚，或 Space 原地跳过扑击。";
   if(e.t>=RamWindup){e.state=2;e.t=0;e.anim.CrossFadeInFixedTime("Attack",.06f);Debug.Log("RAM_LAUNCHED direction="+e.face);}
  }else if(e.state==2){
   float oldX=e.x;e.x+=e.face*RamSpeed*dt;
   // Swept leading-edge crossing, rather than a proximity or player-position trigger.
   float doorLeft=RamDoorX-.31f;
   bool hitDoor=e.face>0&&oldX+.4f<doorLeft&&e.x+.4f>=doorLeft&&Mathf.Abs(e.y)<.5f;
   if(hitDoor){e.x=doorLeft-.4f;OpenRamDoor(e);return;}
   float sweptLeft=Mathf.Min(oldX,e.x)-.65f,sweptRight=Mathf.Max(oldX,e.x)+.65f;
   if(!e.ramHit&&x>sweptLeft&&x<sweptRight){e.ramHit=true;Damage(e,true);}
   if(e.t>=.76f||e.x<e.left){e.x=Mathf.Max(e.left,e.x);RamMiss(e);}
  }else if(e.state==3&&e.t>=1.1f){e.state=0;e.t=0;e.anim.CrossFadeInFixedTime("Walk",.1f);}
 }
 void Visual(float dt){
  VisualSpikes();
  heroAnim.speed=paused?0:1;foreach(var e in AllFoes()){e.anim.speed=paused?0:1;e.root.position=new Vector3(e.x,e.y,-1.65f);e.root.rotation=Quaternion.Euler(0,e.face>0?90:-90,0);e.warning.gameObject.SetActive(e.hp>0&&(e.state==1||e.state==2));e.warning.position=new Vector3(e.x+e.face*1.15f,e.y+.035f,-.2f);}
  for(int i=0;i<gates.Count;i++)gates[i].gameObject.SetActive(i==0?!doorOpen:foes[i].hp>0);
  for(int i=0;i<doorPlanks.Count;i++){
   var plank=doorPlanks[i];plank.gameObject.SetActive(!doorOpen||doorEffect<.85f);
   float f=doorOpen?doorEffect:0;float side=i%2==0?1:-1;
   plank.position=new Vector3(RamDoorX+f*(1.4f+i*.35f),.55f+i*1.1f+f*1.5f-f*f*7,-1.55f+side*f*.4f);
   plank.rotation=Quaternion.Euler(0,0,side*f*160);
   plank.GetComponent<Renderer>().sharedMaterial.color=doorOpen?Color.Lerp(new Color(1,.78f,.3f),new Color(.4f,.28f,.18f),f):new Color(.64f,.34f,.15f);
  }
  baitMark.gameObject.SetActive(!doorOpen);doorSignal.GetComponent<Renderer>().sharedMaterial.color=doorOpen?new Color(.35f,1,.65f):new Color(.95f,.6f,.15f);
  var guard=foes[0];
  if(!doorOpen&&(guard.state==1||guard.state==2)){
   float length=7.2f;guard.warning.localScale=new Vector3(length,.065f,.6f);guard.warning.position=new Vector3(guard.x+guard.face*length/2,.045f,-1.6f);
   guard.warning.GetComponent<Renderer>().sharedMaterial.color=guard.state==2?new Color(1,.15f,.08f):guard.t>=.7f?Color.Lerp(new Color(1,.12f,.03f),new Color(1,.65f,.12f),.5f+.5f*Mathf.Sin(guard.t*45)):new Color(.95f,.62f,.1f);
  }
  for(int i=0;i<beacons.Count;i++)beacons[i].GetComponent<Renderer>().sharedMaterial.color=i==checkpoint?new Color(.4f,.95f,.72f):i<=furthestCheckpoint?new Color(.22f,.6f,.48f):new Color(.2f,.42f,.47f);
  foreach(var coin in coins){coin.shape.gameObject.SetActive(!coin.taken);if(!coin.taken){coin.shape.position=new Vector3(coin.spot.x,coin.spot.y+.045f*Mathf.Sin(clock*3+coin.id),-2.1f);coin.shape.rotation=Quaternion.Euler(90,0,clock*35+coin.id*17);}}
  hero.transform.localScale=Vector3.one*1.5f;hero.flipX=face<0;hero.color=inv>0&&Mathf.Sin(clock*35)>0?new Color(1,.5f,.5f,.65f):Color.white;
  string state=hp<=0?"Death":inv>.35f&&roll<0?"Hurt":roll>=0?"Roll":atk>=0?"Attack"+combo:!grounded?(vy>0?"Jump":"Fall"):Mathf.Abs(inputMove)>.1f?"Run":"Idle";Play(state);
  keyWorld.gameObject.SetActive(relayStage==0);keyCarried.gameObject.SetActive(relayStage==1);
  keyWorld.position=new Vector3(CoreX,.95f+.06f*Mathf.Sin(clock*3),-2.1f);keyWorld.rotation=Quaternion.Euler(0,0,10*Mathf.Sin(clock*2));
  keyCarried.position=new Vector3(x-face*.4f,y+1.3f,-2.3f);
  finalGate.gameObject.SetActive(relayStage<2);
  relayScreen.GetComponent<Renderer>().sharedMaterial.color=relayStage==2?new Color(.3f,1,.65f):relayStage==1?new Color(.2f,.85f,1):new Color(.85f,.5f,.12f);
  foreach(var v in hiddenVisuals)v.gameObject.SetActive(relayStage==2);foreach(var v in hatchVisuals)v.gameObject.SetActive(relayStage<2);
  if(storyVisual!=null){storyVisual.gameObject.SetActive(!storyItemOwned);storyVisual.GetComponent<Renderer>().sharedMaterial.color=lowerCleared?new Color(1,.8f,.25f):new Color(.4f,.4f,.4f);}
  if(storyCarried!=null){storyCarried.gameObject.SetActive(storyItemOwned);storyCarried.position=new Vector3(x-face*.4f,y+1.3f,-2.3f);}
  if(bossGate!=null)bossGate.gameObject.SetActive(inLower&&bossEngaged&&!lowerCleared);
  foreach(var e in lowerFoes)if(e.exitGuard&&e.hp>0){e.root.localScale=Vector3.one*1.2f;var r=BossDanger(e);e.warning.position=new Vector3(r.center.x,r.center.y,-2);e.warning.localScale=new Vector3(r.width,r.height,.08f);e.warning.GetComponent<Renderer>().sharedMaterial.color=e.state==2?new Color(1,.1f,.1f,.8f):new Color(1,.65f,.1f,.65f);}
  if(verify)Present();
 }
 IEnumerable<Foe> AllFoes(){foreach(var e in foes)yield return e;foreach(var e in lowerFoes)yield return e;}
 // Render interpolation keeps the actor and camera on the same presented sample.
 // Fixed forward composition never flips with facing; the horizon is authored and stationary.
 void SyncPresentation(){previousX=x;previousY=y;accumulator=0;inputMove=0;jumpPressed=rollPressed=attackPressed=interactPressed=false;}
 void Present(){
  if(hero==null||cam==null)return;
  float alpha=Mathf.Clamp01(accumulator/.02f);
  float rx=Mathf.Lerp(previousX,x,alpha),ry=Mathf.Lerp(previousY,y,alpha);
  hero.transform.position=new Vector3(rx,ry-.06f,-1.7f);
  cam.orthographicSize=inLower&&bossEngaged?8.5f:inLower?6.75f:5.625f;cam.transform.position=new Vector3(inLower&&bossEngaged?Mathf.Clamp(rx+1,132.5f,143.5f):Mathf.Clamp(rx+1,inLower?77:7,inLower?128:121),inLower?-13.1f:3.2f,-10);cam.backgroundColor=inLower?new Color(.055f,.055f,.1f):new Color(.07f,.1f,.14f);
 }
 void LateUpdate(){Present();}
 void Update(){
  if(verify)return;
  if(Input.GetKeyDown(KeyCode.F1))showHitboxes=!showHitboxes;
  if(Input.GetKeyDown(KeyCode.R))ResetAll();
  if(Input.GetKeyDown(KeyCode.Return)){if(hp<=0)Respawn();else started=true;}
  if(Input.GetKeyDown(KeyCode.Escape))paused=!paused;
  if(Input.GetKeyDown(KeyCode.Tab)){slow=!slow;Time.timeScale=slow?.4f:1;}
  if(!started||paused){Visual(0);return;}
  inputMove=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
  jumpPressed|=Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.W)||Input.GetKeyDown(KeyCode.UpArrow);
  rollPressed|=Input.GetKeyDown(KeyCode.J)||Input.GetKeyDown(KeyCode.LeftShift);attackPressed|=Input.GetKeyDown(KeyCode.K)||Input.GetMouseButtonDown(0);jumpHeld=Input.GetKey(KeyCode.Space)||Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow);
  interactPressed|=Input.GetKeyDown(KeyCode.E);
  accumulator+=Mathf.Min(Time.deltaTime,.1f);while(accumulator>=.02f){Tick(.02f,inputMove,jumpPressed,jumpHeld,rollPressed,attackPressed,interactPressed);jumpPressed=rollPressed=attackPressed=interactPressed=false;accumulator-=.02f;}Visual(Time.deltaTime);
 }
 void Text(float xx,float yy,float w,float h,string s,int size=18,Color? color=null){var st=new GUIStyle(GUI.skin.label){font=font,fontSize=size,wordWrap=true};st.normal.textColor=color??Color.white;GUI.Label(new Rect(xx,yy,w,h),s,st);}
 void OnGUI(){
  GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
  Text(24,14,1100,40,inLower?"下层关卡 / 分岔暗渠 · 多层Boss场 · 剧情道具":"骑士试炼 / 高低路线 · 六段遭遇 · 抵达出口",27);
  Text(24,57,1240,30,"A/D 移动   SPACE/W 跳跃（松开短跳）   J/Shift 翻滚   K/左键 攻击   Enter 复活   R 重开   Esc 暂停",17);
  Text(24,94,1000,30,"生命 "+hp+"/"+tuning.playerHP+(inLower?"    下层检查点 "+(lowerCheckpointLit?"已启用":"入口")+"    守卫 "+(lowerCleared?"已击败":"待击败")+"    路程 "+Mathf.Max(0,Mathf.RoundToInt(x-72))+"/84m":"    已启用点 "+furthestCheckpoint+"/6    击败 "+kills+"/6    路程 "+Mathf.RoundToInt(x)+"/128m"),20,new Color(.65f,.9f,.85f));
  if(!inLower)Text(24,129,1200,32,"中继任务："+RelayStatus()+"    |    E 交互    |    当前复活点 "+(inLower?"下层 "+lowerRespawnX+"米":"上层 "+Mathf.RoundToInt(checkpointX[checkpoint])+"米"),18,new Color(.5f,.9f,1));
  Text(1000,94,260,36,"金币 "+coinsTaken+"/"+coins.Count,23,new Color(1,.81f,.28f));
  Text(1000,129,260,30,"记录匣 "+(storyItemOwned?"1/1":"0/1")+"  | F1 判定",17,new Color(1,.82f,.4f));
  if(coinFeedback>0){var fp=cam.WorldToScreenPoint(coinFeedbackPosition+Vector3.up*(.7f-coinFeedback));Text(fp.x/Screen.width*1280-30,720-fp.y/Screen.height*720-35,180,35,"+1 金币",21,new Color(1,.87f,.3f));}
  Text(24,635,1220,48,message,23);Text(24,686,1000,28,"Hero Knight - Pixel Art / Sven Thole    |    首门诱敌撞开；后续橙门清敌    |    Tab 慢放",15,new Color(.65f,.75f,.8f));
  if(!started||paused||hp<=0||won){GUI.Box(new Rect(350,218,580,260),"");Text(385,245,520,70,won?"到达出口，试炼完成":hp<=0?"倒下了，检查点仍在":paused?"已暂停":"前往 128m 的绿色出口",30);Text(385,305,510,102,won?"本次金币 "+coinsTaken+"/"+coins.Count+"（高路 "+BonusCoinsCollected()+"/"+BonusCoinsTotal()+"）\n"+RelayStatus()+"\nR：从头挑战":hp<=0?"Enter：检查点复活    R：全关重开":paused?"Esc：继续":"终点取钥匙，返回69米按E，显现隐径，再回终点穿门。",21);if(GUI.Button(new Rect(490,425,300,40),won?"重新开始":hp<=0?"返回检查点":paused?"继续":"Enter / 开始关卡")){if(won)ResetAll();else if(hp<=0)Respawn();else{started=true;paused=false;}}}
  if(x>13&&x<28){
   var doorPos=cam.WorldToScreenPoint(new Vector3(23.8f,4.9f,-1.7f));
   var first=foes[0];
   string hint=doorOpen?"门已撞开 → 可以通过":first.state==1&&first.face>0?(first.t>=.7f?"现在 A + J 向左翻滚！\n也可 Space 原地跳开":"朝门蓄力：先等一下……\n红光闪动时再避开"):"木门：诱敌朝右扑击\n站在门左金色标记上";
   Text(doorPos.x/Screen.width*1280-175,720-doorPos.y/Screen.height*720,370,70,hint,19,doorOpen?new Color(.4f,1,.7f):new Color(1,.8f,.35f));
  }
  if(!inLower&&Mathf.Abs(x-RelayX)<11){
   var q=cam.WorldToScreenPoint(new Vector3(RelayX,3.5f,-2.2f));
   string hint=relayStage==0?"中段中继台：需要终点门旁的钥匙":relayStage==1?"地面靠近按 E：交付信号钥匙":"中继已启动 → 72米安全缺口已打开";
   Text(q.x/Screen.width*1280-190,720-q.y/Screen.height*720,440,70,hint,19,new Color(.6f,.9f,1));
  }
  if(!inLower&&x>119){
   var q=cam.WorldToScreenPoint(new Vector3(126.7f,4,-2.1f));
   string hint=relayStage==0?"终点门已锁：门旁拾取信号钥匙\n再带回69米中继台":relayStage==1?"带钥匙返回69米按 E\n这扇终门才会打开":"终门已打开 → 向右通过";
   Text(q.x/Screen.width*1280-180,720-q.y/Screen.height*720,390,75,hint,20,relayStage==2?new Color(.4f,1,.7f):new Color(1,.75f,.3f));
  }
  if(relayFeedback>0&&relayStage==2&&!inLower)Text(360,185,750,40,"终点门已打开 · 72米安全缺口通往下层关卡",24,new Color(.55f,1,.75f));
  if(!inLower&&relayStage==2&&x>63&&x<83){
   var q=cam.WorldToScreenPoint(new Vector3(72.5f,1.9f,-2.1f));
   Text(q.x/Screen.width*1280-180,720-q.y/Screen.height*720,390,85,"安全缺口 ↓ 下层关卡\n从这里跳下不会死亡",24,new Color(.55f,.95f,1));
  }
  if(inLower){
   if(x>104&&x<118)Text(420,230,790,65,"必经升降台：自动往返 → 搭到顶端 → 向右跳上出口",22,new Color(.55f,1,1));
   Text(24,129,1200,28,storyItemOwned?"记录匣已取得 → 末端蓝色出口按 E 返回":"低路战斗 / 高路金币 → 末端取得记录匣 → E 返回上层",18,new Color(.65f,1,.8f));
   foreach(var b in lowerFoes)if(b.exitGuard&&bossEngaged&&!lowerCleared)Text(390,200,750,40,"BOSS "+b.hp+"/"+b.maxHP+" · "+BossLabel(b),23,new Color(1,.75f,.4f));
   if(x>128&&!bossEngaged)Text(700,300,520,95,storyItemOwned?"剧情道具已入手，靠近蓝色出口按 E":"末端记录匣：击败守卫后靠近拾取",22,new Color(1,.85f,.4f));

  }
  if(!inLower&&spikes!=null&&Mathf.Abs(x-spikes.transform.position.x)<9)Text(350,220,850,65,"伸缩地刺：绿灯通行 · 黄灯预警 · 红灯危险，可跳跃通过",21,new Color(1,.85f,.4f));
  if(showHitboxes&&spikes!=null&&!inLower)DrawCombatRect(SpikeRect(),SpikePhase()==2?Color.red:Color.yellow);
  // World-space signs and enemy health labels stay tied to landing locations.
  foreach(var e in inLower?lowerFoes:foes)if(e.hp>0&&Mathf.Abs(e.x-x)<10){var p=cam.WorldToScreenPoint(new Vector3(e.x,e.y+2.7f,0));Text(p.x/Screen.width*1280-35,720-p.y/Screen.height*720,100,30,e.hp+"/"+e.maxHP,18,new Color(1,.5f,.3f));}  if(showHitboxes){DrawCombatRect(PlayerBody(),Color.cyan);DrawCombatRect(PlayerAttack(),atk>=tuning.attackWindup&&atk<=tuning.attackActiveEnd?Color.red:Color.gray);
   foreach(var e in inLower?lowerFoes:foes)if(e.hp>0){DrawCombatRect(EnemyBody(e),Color.green);DrawCombatRect(e.exitGuard?BossDanger(e):EnemyAttack(e),e.state==2?Color.red:new Color(1,.6f,.2f));}
   Text(24,170,1000,28,"判定：蓝=玩家受击 绿=敌人受击 红=有效攻击 灰/橙=攻击范围预览",17,Color.white);
  }
  if(transitionFade>0){GUI.color=new Color(0,0,0,Mathf.Clamp01(transitionFade/.35f));GUI.DrawTexture(new Rect(0,0,1280,720),Texture2D.whiteTexture);GUI.color=Color.white;}

 }
 void DrawCombatRect(Rect r,Color c){
  var a=cam.WorldToViewportPoint(new Vector3(r.xMin,r.yMax,-2));var b=cam.WorldToViewportPoint(new Vector3(r.xMax,r.yMin,-2));
  float xx=a.x*1280,yy=(1-a.y)*720,ww=(b.x-a.x)*1280,hh=(a.y-b.y)*720;if(xx+ww<0||xx>1280||yy+hh<0||yy>720)return;
  GUI.color=c;GUI.DrawTexture(new Rect(xx,yy,ww,2),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(xx,yy+hh,ww,2),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(xx,yy,2,hh),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(xx+ww,yy,2,hh),Texture2D.whiteTexture);GUI.color=Color.white;
 }
 void Assert(bool yes,string name){if(yes)passedChecks++;else failedChecks++;Debug.Log((yes?"TEST_PASS ":"TEST_FAIL ")+name);if(!yes){System.IO.File.WriteAllText(RootPath+"level-test-failed.txt",name);Application.Quit(2);throw new System.Exception(name);}}

 IEnumerator VerifyCamera(){
  yield return null;ResetAll();x=8;checkpoint=0;SyncPresentation();
  var csv=new System.Text.StringBuilder("frame,scenario,heroX,heroY,cameraX,cameraY,screenX,screenY\n");
  bool pendingJump=false,pendingDodge=false; for(int f=0;f<240;f++){
   string scenario=f<30?"right":f<60?"left":f<90?"stop":f<150?"jump":f<190?"roll":f<210?"respawn":"boundary";
   float move=f<30?1:f<60?-1:0;bool jump=f==90,dodge=f==150;
   if(f==190){x=35;y=-6;Respawn();}
   if(f==210){x=-1.4f;y=0;vy=0;SyncPresentation();}
   if(f>=210)move=-1;
   pendingJump|=jump;pendingDodge|=dodge;inputMove=move;accumulator+=1f/60;
   while(accumulator>=.02f){Tick(.02f,move,pendingJump,true,pendingDodge,false);pendingJump=pendingDodge=false;accumulator-=.02f;}
   Visual(1f/60);
   var sp=cam.WorldToViewportPoint(hero.transform.position);
   csv.AppendLine(f+","+scenario+","+hero.transform.position.x+","+hero.transform.position.y+","+cam.transform.position.x+","+cam.transform.position.y+","+sp.x+","+sp.y);
   if(f==28||f==31||f==88||f==111||f==151||f==191)ScreenCapture.CaptureScreenshot(RootPath+"camera-after-"+f+".png");
   yield return null;
  }
  System.IO.File.WriteAllText(RootPath+"camera-after.csv",csv.ToString());Debug.Log("CAMERA_PROBE_COMPLETE");Application.Quit();
 }
 void LowerVerificationInput(bool highRoute,ref int step,out float move,out bool jump,out bool attack,out bool interact){
  move=1;jump=attack=interact=false;
  if(highRoute&&step<8){
   if(step==0){move=x<76.6f?1:0;jump=grounded&&x>74.4f&&y<-17.8f;if(grounded&&y>-16.7f)step=1;}
   else if(step==1){if(grounded&&x>77.4f){jump=true;step=2;}}
   else if(step==2){move=x<80.6f?1:0;if(grounded&&y>-15.3f)step=3;}
   else if(step==3){if(grounded&&x>82){jump=true;step=4;}}
   else if(step==4){move=x<85.5f?1:0;if(grounded&&y>-14.1f)step=5;}
   else if(step==5){if(grounded&&x>86.5f){jump=true;step=6;}}
   else if(step==6){move=x<90.5f?1:0;if(grounded&&y>-14.1f)step=7;}
   else if(step==7&&x>105.5f&&y<-17)step=8;
   return;
  }
  if(lift!=null&&x>=110.8f&&x<114.9f&&!liftCrossed){
   move=Mathf.Abs(x-111.5f)>.12f?Mathf.Sign(111.5f-x):0;attack=true;
   if(y>=-13.65f){move=1;jump=grounded;}return;
  }
  var boss=lowerFoes.Find(e=>e.exitGuard&&e.hp>0);
  if(boss!=null&&bossEngaged){
   float dist=boss.x-x;move=Mathf.Abs(dist)>1.4f?Mathf.Sign(dist):0;face=dist>=0?1:-1;attack=true;
   if(boss.state==1&&boss.bossMove==2){move=x<boss.targetX?-1:1;if(x<119.5f)move=1;if(x>156.5f)move=-1;}
   jump=grounded&&((boss.state==1&&boss.bossMove!=2&&boss.t>BossWindup(boss)-.32f)||(boss.y>y+1&&Mathf.Abs(dist)<4));return;
  }
  Foe near=null;foreach(var e in lowerFoes)if(e.hp>0&&Mathf.Abs(e.x-x)<2.05f&&Mathf.Abs(y-e.y)<.3f){near=e;break;}
  if(near!=null){move=Mathf.Abs(near.x-x)>1.55f?(near.x>x?1:-1):0;face=near.x>x?1:-1;attack=true;}
  jump=grounded&&((x>80.1f&&x<81.1f)||(x>97.1f&&x<98.1f)||(x>114.1f&&x<115.1f));
  interact=Mathf.Abs(x-lowerReturnX)<.8f&&grounded&&storyItemOwned;
 }
 IEnumerator VerifyLevel(){
  yield return null;ResetAll();int frame=0;float maxY=0;Sprite runA=null;bool runChanged=false,jumpSeen=false,attackSeen=false;
  // Continuous control-driven playthrough: no position warps, no removed enemies.
  int relayBotPhase=0,lowerBotStep=0;bool lockedGateSeen=false,returnCheckpointSeen=false;
  while(!won&&frame<10000){
   float move=1;bool attack=false,dodge=false,jump=false,interact=false;
   Foe near=null;foreach(var e in foes)if(e.hp>0&&Mathf.Abs(e.x-x)<2.05f&&Mathf.Abs(y)<.3f){near=e;break;}
   if(doorOpen&&near!=null){move=Mathf.Abs(near.x-x)>1.55f?(near.x>x?1:-1):0;face=near.x>x?1:-1;attack=true;}
   if(!doorOpen&&x>14){
    var guard=foes[0];attack=false;
    move=x<22.85f?1:0;
    if(guard.state==1&&guard.face>0&&guard.t>.76f&&roll<0){move=-1;dodge=true;}
    if(guard.state==1&&guard.face>0&&guard.t>.7f&&guard.t<.76f)ScreenCapture.CaptureScreenshot(RootPath+"ram-door-windup.png");
    if(guard.state==2&&guard.face>0&&guard.x>22)ScreenCapture.CaptureScreenshot(RootPath+"ram-door-impact.png");
   }
   if(grounded&&((x>10.8f&&x<12.1f)||(x>24.2f&&x<25.2f)||(x>42.2f&&x<43.1f)||(x>77.2f&&x<78.1f)||(x>93.2f&&x<94.1f)||(x>109.2f&&x<110.1f)))jump=true;
   if(relayBotPhase==0&&relayStage==1&&x>=126.4f){
    lockedGateSeen=!won&&relayStage==1;relayBotPhase=1;ScreenCapture.CaptureScreenshot(RootPath+"relay-end-locked.png");
   }
   if(relayBotPhase==1){
    move=-1;attack=false;dodge=false;
    jump=grounded&&((x<114.8f&&x>114.1f)||(x<98.8f&&x>98.1f)||(x<81.8f&&x>81.1f));
    if(checkpoint<6)returnCheckpointSeen=true;
    if(Mathf.Abs(x-RelayX)<.4f&&grounded){move=0;jump=false;interact=true;relayBotPhase=2;}
   }else if(relayBotPhase==2){
    attack=dodge=jump=false;move=1;
    if(!inLower){
     move=x<72.4f?1:0;
     if(hiddenRouteVisited){relayBotPhase=3;move=1;}
    }else{
     LowerVerificationInput(false,ref lowerBotStep,out move,out jump,out attack,out interact);
     if(!storyItemOwned&&x>86&&x<86.2f)ScreenCapture.CaptureScreenshot(RootPath+"lower-level-playing.png");
    }
   }
   if(!inLower&&spikes!=null&&grounded&&((move>0&&x>48.5f&&x<49)||(move<0&&x<54.2f&&x>53.7f)))jump=true;
   inputMove=move;Tick(.02f,move,jump,true,dodge,attack,interact);Visual(.02f);maxY=Mathf.Max(maxY,y);frame++;
   if(animState=="Run"){if(runA==null)runA=hero.sprite;else if(runA!=hero.sprite)runChanged=true;}
   if(animState=="Jump")jumpSeen=true;if(animState.StartsWith("Attack"))attackSeen=true;
   if(inLower&&!storyItemOwned&&x>74&&x<74.2f)ScreenCapture.CaptureScreenshot(RootPath+"lower-hub-overview.png");
   if(inLower&&storyItemOwned&&x>153&&x<153.2f)ScreenCapture.CaptureScreenshot(RootPath+"story-item-inventory.png");
   if(!inLower&&hiddenRouteVisited&&transitionFade<.02f&&x>76.8f&&x<77.1f)ScreenCapture.CaptureScreenshot(RootPath+"lower-return-surface.png");
   if(relayStage==0&&x>68.7f&&x<68.9f)ScreenCapture.CaptureScreenshot(RootPath+"relay-middle-first-visit.png");
   if(interact&&relayStage==2)ScreenCapture.CaptureScreenshot(RootPath+"relay-middle-delivered.png");
   if(doorOpen&&doorEffect<.03f)ScreenCapture.CaptureScreenshot(RootPath+"ram-door-open.png");
   if(doorOpen&&doorEffect>.4f&&doorEffect<.43f)ScreenCapture.CaptureScreenshot(RootPath+"ram-door-break-feedback.png");
   if(doorOpen&&x>24.5f&&x<24.65f)ScreenCapture.CaptureScreenshot(RootPath+"ram-door-passed.png");
   if(frame%3==0)yield return null;
   if(frame==100)ScreenCapture.CaptureScreenshot(RootPath+"level-start.png");
   if(checkpoint==1&&frame%70==0)ScreenCapture.CaptureScreenshot(RootPath+"level-middle.png");
  }

  Debug.Log("ROUTE_END x="+x+" y="+y+" hp="+hp+" liftPassed="+liftCrossed+" frame="+frame);
  Assert(won&&kills==6&&checkpoint==6,"normal entry story pickup end return and final completion");
  Assert(runChanged&&jumpSeen&&attackSeen&&returnCheckpointSeen&&lockedGateSeen,"movement animation and relay backtracking remain valid");
  Assert(doorOpen&&ramImpacts==1&&relayStage==2&&relayDeliveries==1,"ram door and relay remain coherent");
  Assert(storyItemOwned&&storyPickups==1&&hiddenRouteVisited,"story archive collected once and carried to surface");
  Assert(lowerModules.Count==20,"return loop removed and eight boss platforms present");
  ScreenCapture.CaptureScreenshot(RootPath+"story-final.png");yield return null;
  ResetAll();relayStage=2;corePickups=relayDeliveries=1;EnterLower();int highStep=0,highFrames=0;bool bypass=false;
  while(inLower&&highFrames<4200){float mv;bool j,a,e;LowerVerificationInput(true,ref highStep,out mv,out j,out a,out e);inputMove=mv;Tick(.02f,mv,j,true,false,a,e);Visual(.02f);highFrames++;if(x>102&&lowerFoes[0].hp==lowerFoes[0].maxHP)bypass=true;if(highFrames%3==0)yield return null;}
  Assert(!inLower&&storyItemOwned&&bypass,"high route bypasses first guard and returns directly from end");
  Assert(LowerCoinsCollected()==3,"high branch keeps three collectible rewards without a return loop");
  EnterLower();hp=0;Respawn();Visual(.02f);
  Assert(inLower&&storyItemOwned&&storyPickups==1&&lowerCleared,"death keeps story inventory and defeated end guard");
  x=lowerStoryX;CollectStoryItem();CollectStoryItem();Assert(storyPickups==1,"story archive cannot be duplicated");
  x=lowerReturnX;y=-18;UseLowerInteraction();Assert(!inLower&&storyItemOwned,"end interaction returns without consuming story item");
  ResetAll();relayStage=2;corePickups=relayDeliveries=1;EnterLower();x=lowerReturnX;y=-18;UseLowerInteraction();Assert(inLower&&!storyItemOwned,"exit refuses before archive is acquired");
  x=lowerCheckpointX;y=-18;Tick(.02f,0,false,true,false,false);hp=0;Respawn();Assert(inLower&&x==lowerCheckpointX&&!storyItemOwned,"death before archive restores valid lower checkpoint");
  ResetAll();Visual(.02f);Assert(!inLower&&!storyItemOwned&&storyPickups==0&&relayStage==0&&coinsTaken==0,"restart restores item world and all progression");
  // Isolated boundary checks use the exact same runtime rectangles and damage methods.
  var enemy=foes[0];foreach(var f in foes)f.hp=0;enemy.hp=enemy.maxHP=tuning.enemyHP;enemy.y=0;enemy.state=0;x=20;y=0;face=1;combo=1;atk=tuning.attackWindup;
  enemy.x=22.19f;swingHits.Clear();AttackHit();Assert(enemy.hp==40,"front inside 2.20m center limit hits for 20");
  enemy.hp=60;enemy.x=22.21f;swingHits.Clear();AttackHit();Assert(enemy.hp==60,"beyond front rectangle boundary misses");
  enemy.x=19.9f;swingHits.Clear();AttackHit();Assert(enemy.hp==60,"behind target cannot be hit despite overlapping body edge");
  enemy.x=21;enemy.y=1.71f;swingHits.Clear();AttackHit();Assert(enemy.hp==60,"nonoverlapping vertical ranges miss");
  enemy.y=1.69f;swingHits.Clear();AttackHit();Assert(enemy.hp==40,"overlapping vertical edge hits");
  enemy.y=0;enemy.hp=60;atk=.11f;swingHits.Clear();AttackHit();Assert(enemy.hp==60,"attack windup deals no damage");
  atk=.25f;AttackHit();Assert(enemy.hp==60,"attack recovery deals no damage");
  atk=.12f;for(int i=0;i<10;i++)AttackHit();Assert(enemy.hp==40,"one target takes damage only once per swing");
  enemy.hp=60;combo=3;swingHits.Clear();AttackHit();Assert(enemy.hp==30,"third strike deals 30");
  enemy.hp=60;combo=1;enemy.state=3;swingHits.Clear();AttackHit();Assert(enemy.hp==30,"counter strike applies 1.5 multiplier");
  enemy.state=2;enemy.face=-1;enemy.x=22.34f;hp=100;inv=0;roll=-1;Damage(enemy);Assert(hp==80,"enemy rectangle inside 2.35m center reach hits for 20");
  enemy.x=22.36f;hp=100;inv=0;Damage(enemy);Assert(hp==100,"outside enemy reach misses");
  enemy.x=21.5f;enemy.face=1;Damage(enemy);Assert(hp==100,"enemy cannot hit behind its facing");
  enemy.face=-1;roll=.1f;inv=0;Damage(enemy);Assert(hp==100,"dodge invulnerability window prevents damage");
  roll=.43f;Damage(enemy);Assert(hp==80,"after dodge invulnerability player is vulnerable");
  Damage(enemy);Assert(hp==80,"post hit invulnerability prevents immediate repeat damage");
  enemy.exitGuard=true;inv=0;roll=-1;Damage(enemy);Assert(hp==55,"end guard hit deals 25");
  enemy.exitGuard=false;inv=0;Damage(enemy,true);Assert(hp==25,"ram hit deals 30");
  Assert(tuning.playerHP==100&&tuning.enemyHP==60&&tuning.bossHP==240,"authoritative health tuning loaded");
  ResetAll();relayStage=2;corePickups=relayDeliveries=1;x=72.4f;y=0;grounded=true;int beforeFalls=falls;
  for(int i=0;i<65&&!inLower;i++){Tick(.02f,0,false,true,false,false);Visual(.02f);if(i%3==0)yield return null;}
  Assert(inLower&&hp==100&&falls==beforeFalls,"safe drop remains nonlethal");
  ResetAll();Assert(!storyItemOwned&&lowerFoes[0].hp==60&&lowerFoes[2].hp==240,"restart restores normal and guard health values");
  showHitboxes=true;x=18;y=0;foes[0].x=19.5f;face=1;atk=.15f;SyncPresentation();Visual(.02f);ScreenCapture.CaptureScreenshot(RootPath+"combat-hitboxes.png");yield return null;showHitboxes=false;
  Debug.Log("LEVEL_VERIFY_OK assertions="+passedChecks+" frames="+frame);
  System.IO.File.WriteAllText(RootPath+"level-verified.json",$@"{{""passed"":{passedChecks},""failed"":{failedChecks},""normalEntryStoryReturn"":true,""simulationFrames"":{frame}}}");
  yield return new WaitForSeconds(.15f);Application.Quit();
 }
}
