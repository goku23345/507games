using UnityEngine;
using System.Collections;
public class DodgeStudy : MonoBehaviour {
 public float duration=1.65f, distance=2.8f;
 Transform[] pieces=new Transform[11]; Material shirt,skin,pants;
 float t=-1, origin=-2.5f, pos=-2.5f; int face=1,count=0; bool slow,verify; float elapsed; bool[] shots=new bool[4];
 // hip, shoulder, head, elbow/hand front, elbow/hand back, knee/foot front, knee/foot back
 Vector2[][] poses={
 new[]{new Vector2(0,1.02f),new Vector2(-.08f,1.53f),new Vector2(-.12f,1.87f),new Vector2(.25f,1.38f),new Vector2(.35f,1.65f),new Vector2(-.35f,1.33f),new Vector2(-.18f,1.6f),new Vector2(.15f,.56f),new Vector2(.25f,.09f),new Vector2(-.2f,.56f),new Vector2(-.28f,.09f)},
 new[]{new Vector2(0,.97f),new Vector2(.4f,1.12f),new Vector2(.7f,1.25f),new Vector2(.72f,.93f),new Vector2(1.02f,.67f),new Vector2(.02f,1.4f),new Vector2(-.2f,1.55f),new Vector2(-.4f,.72f),new Vector2(-.78f,.67f),new Vector2(-.28f,.46f),new Vector2(-.58f,.2f)},
 new[]{new Vector2(0,.51f),new Vector2(.4f,.7f),new Vector2(.58f,.98f),new Vector2(.7f,.37f),new Vector2(.86f,.08f),new Vector2(.1f,.95f),new Vector2(.22f,1.19f),new Vector2(.25f,.10f),new Vector2(-.14f,.08f),new Vector2(-.43f,.28f),new Vector2(-.76f,.08f)},
 new[]{new Vector2(0,.65f),new Vector2(.15f,1.1f),new Vector2(.08f,1.45f),new Vector2(.5f,.72f),new Vector2(.58f,.09f),new Vector2(-.15f,.88f),new Vector2(.24f,.62f),new Vector2(.43f,.42f),new Vector2(.56f,.08f),new Vector2(-.3f,.1f),new Vector2(-.6f,.08f)},
 new[]{new Vector2(0,1.05f),new Vector2(-.22f,1.51f),new Vector2(-.33f,1.84f),new Vector2(.17f,1.36f),new Vector2(.47f,1.51f),new Vector2(-.53f,1.26f),new Vector2(-.65f,1.5f),new Vector2(.26f,.6f),new Vector2(.42f,.09f),new Vector2(-.37f,.58f),new Vector2(-.58f,.09f)},
 new[]{new Vector2(0,1.02f),new Vector2(-.08f,1.53f),new Vector2(-.12f,1.87f),new Vector2(.25f,1.38f),new Vector2(.35f,1.65f),new Vector2(-.35f,1.33f),new Vector2(-.18f,1.6f),new Vector2(.15f,.56f),new Vector2(.25f,.09f),new Vector2(-.2f,.56f),new Vector2(-.28f,.09f)}
 };
 float[] times={0,.16f,.38f,.61f,.85f,1};
 Material Mat(Color c){var m=new Material(Resources.Load<Material>("Block"));m.color=c;return m;}
 Transform Box(string n, Material m){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=n;o.GetComponent<Renderer>().sharedMaterial=m;Destroy(o.GetComponent<Collider>());return o.transform;}
 Transform enemy,zone,weapon; Animator enemyAnim;
 float ex=2.6f,phaseTime=0,attackTime=-1,hurt=0,enemyFlash=0; int hp=3,ehp=4,enemyFace=-1; bool attackApplied=false; 
 enum Phase { Approach, Windup, Strike, Recover, Dead } Phase phase;
 string message="Watch the red warning. Dodge, then get close and counter.";
 Font font; bool started;
 void Start(){
 Application.targetFrameRate=60; verify=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-verify")>=0;
 shirt=Mat(new Color(.38f,.66f,.73f));skin=Mat(new Color(.85f,.77f,.66f));pants=Mat(new Color(.24f,.3f,.37f));
 for(int i=0;i<11;i++) pieces[i]=Box("Boy_Block_"+i,i==2||i==4||i==6?skin:i>6?pants:shirt);
 var ground=Box("Simple ground",Mat(new Color(.25f,.29f,.32f))); ground.position=new Vector3(0,-.18f,0);ground.localScale=new Vector3(70,.3f,3);
 var line=Mat(new Color(.38f,.42f,.44f));
 for(int i=-30;i<=30;i++){var b=Box("Meter mark",line);b.position=new Vector3(i,-.015f,-.1f);b.localScale=new Vector3(.025f,.012f,2.5f);}
 RenderSettings.ambientLight=new Color(.72f,.76f,.8f);
 var light=new GameObject("Soft key").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1;light.transform.rotation=Quaternion.Euler(35,-30,0);
 enemy=Instantiate(Resources.Load<GameObject>("Enemy")).transform;enemyAnim=enemy.GetComponentInChildren<Animator>();
 zone=Box("Attack warning",Mat(new Color(.85f,.2f,.12f)));weapon=Box("Scrap plank",Mat(new Color(.75f,.63f,.4f)));
 font=Font.CreateDynamicFontFromOSFont("Microsoft YaHei",18);
 ResetFight(); started=verify; Tick(0,0,false,false); if(verify)StartCoroutine(VerifyFight());
 }
 Vector3 V(Vector2 v,float z=0){return new Vector3(pos+face*v.x,v.y,z);}
 void Limb(int i,Vector2 a,Vector2 b,float width,float z){var v=V(b,z)-V(a,z);pieces[i].position=(V(a,z)+V(b,z))/2;pieces[i].localScale=new Vector3(width,v.magnitude,width);pieces[i].rotation=Quaternion.FromToRotation(Vector3.up,v);}
 void Pose(float u){
 int k=0;while(k<times.Length-2 && u>times[k+1])k++;float a=Mathf.SmoothStep(0,1,Mathf.InverseLerp(times[k],times[k+1],u));
 Vector2[] q=new Vector2[11];for(int j=0;j<11;j++)q[j]=Vector2.Lerp(poses[k][j],poses[k+1][j],a);
 Limb(0,q[0],q[1],.36f,0);pieces[1].position=V(q[0]);pieces[1].localScale=new Vector3(.38f,.22f,.28f);
 pieces[2].position=V(q[2]);pieces[2].localScale=new Vector3(.3f,.35f,.3f);pieces[2].rotation=Quaternion.Euler(0,0,face*(u>.16f&&u<.6f?-18:8));
 Limb(3,q[1],q[3],.13f,-.22f);Limb(4,q[3],q[4],.12f,-.22f);
 Limb(5,q[1],q[5],.13f,.22f);Limb(6,q[5],q[6],.12f,.22f);
 Limb(7,q[0],q[7],.17f,-.13f);Limb(8,q[7],q[8],.15f,-.13f);
 Limb(9,q[0],q[9],.17f,.13f);Limb(10,q[9],q[10],.15f,.13f);
 }

 void ResetFight(){started=true;hp=3;ehp=4;pos=-2.5f;ex=2.6f;face=1;t=-1;attackTime=-1;phase=Phase.Approach;phaseTime=0;hurt=0;enemyFlash=0;slow=false;Time.timeScale=1;message="等待红色预兆 → 闪避 → 靠近反击";SetEnemy("Idle");}
 void SetEnemy(string state){if(enemyAnim!=null)enemyAnim.CrossFadeInFixedTime(state,.12f);}
 void Dodge(){if(t>=0||hp<=0||ehp<=0)return;origin=pos;t=0;count++;message="扑出去！撑住，再站起来";Debug.Log("DODGE_STARTED");}
 bool Invulnerable(){return t>=0&&t<.67f;}
 void Attack(){if(attackTime>=0||hp<=0||ehp<=0||(t>=0&&t<.92f))return;attackTime=0;attackApplied=false;}
 bool InEnemyArc(){float dx=pos-ex;return Mathf.Abs(dx)<2.7f&&dx*enemyFace>-.35f;}
 void EnemyHit(){
  if(!InEnemyArc()){message="打空了！靠近按 K 反击";return;}
  if(Invulnerable()){message="险险躲开！现在反击";Debug.Log("DODGE_EVADE_OK");return;}
  if(hurt>0)return; hp--;hurt=.65f;pos=Mathf.Clamp(pos+enemyFace*.35f,-6,6);
  message=hp<=0?"没撑住。按 R 重新交战":"被击中！别硬扛，看到预兆再闪避";
  Debug.Log("PLAYER_HIT hp="+hp);
 }
 void PlayerHit(){
  if(Mathf.Abs(ex-pos)>2.25f||(ex-pos)*face<-.3f){message="挥空了，靠近并面向对手";return;}
  bool counter=phase==Phase.Recover;ehp-=counter?2:1;enemyFlash=.25f;
  message=counter?"反击命中！趁它没站稳":"命中了，但正面硬拼很危险";
  Debug.Log("PLAYER_ATTACK_HIT counter="+counter+" hp="+ehp);
  if(ehp<=0){ehp=0;phase=Phase.Dead;SetEnemy("Fall");message="活下来了。按 R 再来一场";}
 }
 void Tick(float dt,float move,bool dodge,bool attack){
  hurt=Mathf.Max(0,hurt-dt);enemyFlash=Mathf.Max(0,enemyFlash-dt);
  if(hp>0&&ehp>0){
   if(Mathf.Abs(move)>.01f){face=move>0?1:-1;if(t<0)pos=Mathf.Clamp(pos+move*3.3f*dt,-6,6);}
   if(dodge)Dodge();if(attack)Attack();
  }
  if(t>=0){t+=dt;float u=Mathf.Clamp01(t/duration);pos=Mathf.Clamp(origin+face*distance*(u<.42f?Mathf.Sin(u/.42f*Mathf.PI/2):1),-6,6);Pose(u);if(u>=1){t=-1;Debug.Log("DODGE_COMPLETE");}}
  else Pose(0);
  if(attackTime>=0){
   attackTime+=dt;
   if(attackTime>.13f&&!attackApplied){attackApplied=true;if(hp>0&&ehp>0)PlayerHit();}
   if(attackTime>.48f)attackTime=-1;
  }
  if(hp>0&&ehp>0){
   phaseTime+=dt;
   if(phase==Phase.Approach){
    enemyFace=pos>ex?1:-1;
    if(Mathf.Abs(pos-ex)>1.8f){ex+=enemyFace*1.1f*dt; if(enemyAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))SetEnemy("Walk");}
    else {phase=Phase.Windup;phaseTime=0;SetEnemy("Idle");message="它要扑过来了！SPACE 闪避";}
   }else if(phase==Phase.Windup&&phaseTime>.85f){phase=Phase.Strike;phaseTime=0;SetEnemy("Attack");EnemyHit();}
   else if(phase==Phase.Strike&&phaseTime>.3f){phase=Phase.Recover;phaseTime=0;message="它扑空后僵住了，靠近按 K 反击";}
   else if(phase==Phase.Recover&&phaseTime>1.8f){phase=Phase.Approach;phaseTime=0;SetEnemy("Walk");}
  }
  enemy.position=new Vector3(ex,0,.12f);enemy.rotation=Quaternion.Euler(0,enemyFace>0?90:-90,0);
  zone.gameObject.SetActive(phase==Phase.Windup||phase==Phase.Strike);
  zone.position=new Vector3(ex+enemyFace*1.15f,.015f,-.25f);zone.localScale=new Vector3(2.7f,.04f,1.1f);
  zone.GetComponent<Renderer>().sharedMaterial.color=phase==Phase.Strike?new Color(1,.5f,.15f):new Color(.7f+.25f*Mathf.Sin(phaseTime*20),.16f,.1f);
  Vector3 hand=pieces[4].position;weapon.position=hand+new Vector3(face*.12f,0,-.13f);weapon.localScale=new Vector3(.11f,.65f,.12f);
  weapon.rotation=Quaternion.Euler(0,0,face*(attackTime>=0?Mathf.Lerp(70,-100,attackTime/.4f):28));
  foreach(var b in pieces)b.GetComponent<Renderer>().enabled=!(hurt>0&&Mathf.Sin(hurt*50)>0);
  Camera.main.transform.position=new Vector3(0,2.15f,-10);
 }
 void Update(){
  if(Input.GetKeyDown(KeyCode.Tab)){slow=!slow;Time.timeScale=slow?.35f:1;}
  if(Input.GetKeyDown(KeyCode.R))ResetFight();
  if(verify)return;
  if(!started){if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.Space))started=true;return;}
  float move=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
  Tick(Time.deltaTime,move,Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.J),Input.GetKeyDown(KeyCode.K)||Input.GetMouseButtonDown(0));
 }
 void Check(bool ok,string label){Debug.Log((ok?"TEST_PASS ":"TEST_FAIL ")+label);if(!ok)throw new System.Exception(label);}
 IEnumerator VerifyFight(){
  yield return null;ResetFight();
  ex=pos+1.7f;enemyFace=-1;phase=Phase.Windup;phaseTime=.5f;
  for(int i=0;i<25;i++){Tick(.02f,0,false,false);yield return null;}
  Check(hp==2,"enemy telegraph transitions to damaging strike");
  ResetFight();ex=pos+1.7f;enemyFace=-1;t=.1f;EnemyHit();Check(hp==3,"dodge invulnerability");
  t=-1;pos=-5;ex=3;EnemyHit();Check(hp==3,"out of range misses");
  ResetFight();ex=0;pos=-1.7f;face=1;phase=Phase.Windup;phaseTime=.45f;Dodge();
  for(int i=0;i<55;i++){Tick(.02f,0,false,false);if(i==12)ScreenCapture.CaptureScreenshot(Application.dataPath+"/../../battle-dive.png");if(i==26)ScreenCapture.CaptureScreenshot(Application.dataPath+"/../../battle-handplant.png");yield return null;}
  Check(hp==3,"full dodge survives telegraphed attack");
  // Exercise the real attack timing and counter damage.
  t=-1;pos=ex-1.5f;face=1;phase=Phase.Recover;phaseTime=0;Attack();
  for(int i=0;i<26;i++){Tick(.02f,0,false,false);yield return null;}
  Check(ehp==2,"recovery counter damage");
  phase=Phase.Recover;phaseTime=0;Attack();
  for(int i=0;i<26;i++){Tick(.02f,0,false,false);yield return null;}
  Check(ehp==0&&phase==Phase.Dead,"victory");
  ScreenCapture.CaptureScreenshot(Application.dataPath+"/../../battle-victory.png");yield return null;
  ResetFight();ex=pos+1.5f;enemyFace=-1;for(int i=0;i<3;i++){hurt=0;EnemyHit();}
  Check(hp==0,"defeat");ResetFight();Check(hp==3&&ehp==4&&t<0,"retry resets");
  // Show both original asset motion and complete arena.
  for(int i=0;i<90;i++){Tick(.02f,0,false,false);yield return null;}
  ScreenCapture.CaptureScreenshot(Application.dataPath+"/../../battle-arena.png");yield return new WaitForSeconds(.2f);
  Debug.Log("PLAY_VERIFY_OK combat assertions=8");Application.Quit();
 }
 void Label(float x,float y,float w,float h,string text,int size=18,Color? color=null){
  var style=new GUIStyle(GUI.skin.label);style.font=font;style.fontSize=size;style.normal.textColor=color??Color.white;style.wordWrap=true;GUI.Label(new Rect(x,y,w,h),text,style);
 }
 void OnGUI(){
  GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
  Label(28,20,760,42,"少年求生 / 一场短交战",28);
  Label(28,68,1100,30,"A / D 移动    SPACE / J 侧扑闪避    K / 鼠标左键 反击    TAB 慢放    R 重试",18);
  Label(28,112,500,30,"少年  "+new string('■',Mathf.Max(0,hp))+new string('□',3-Mathf.Max(0,hp)),22,new Color(.5f,.85f,.9f));
  Label(880,112,380,30,"对手  "+new string('■',Mathf.Max(0,ehp))+new string('□',4-Mathf.Max(0,ehp)),22,new Color(.95f,.48f,.35f));
  Label(28,603,1220,60,message,24);
  Label(28,674,1100,28,"单手撑地 · 膝盖着地 · 踉跄站起    |    "+(slow?"0.35 倍速":"正常速度")+"    |    闪避次数 "+count,16,new Color(.7f,.75f,.78f));
  if(!started){Label(380,260,650,75,"看准预兆，狼狈求生",32);if(GUI.Button(new Rect(535,350,210,56),"ENTER / 开始交战"))started=true;}
  if(hp<=0||ehp<=0){if(GUI.Button(new Rect(535,350,210,56),"R / 重新交战"))ResetFight();}
 }
}
