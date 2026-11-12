using UnityEngine;
using System.Collections;
public partial class KnightLevel {
 WhiteboxSpikes spikes;float spikeTime;int spikeHitCycle=-1;Transform spikeBase;readonly System.Collections.Generic.List<Transform> spikeTeeth=new System.Collections.Generic.List<Transform>();
 float SpikeCycle{get{return spikes.safeSeconds+spikes.warningSeconds+spikes.activeSeconds+spikes.retractSeconds;}}
 int SpikePhase(){float t=Mathf.Repeat(spikeTime,SpikeCycle);return t<spikes.safeSeconds?0:t<spikes.safeSeconds+spikes.warningSeconds?1:t<spikes.safeSeconds+spikes.warningSeconds+spikes.activeSeconds?2:3;}
 Rect SpikeRect(){var p=spikes.transform.position;return new Rect(p.x-spikes.width/2,p.y,spikes.width,spikes.height);}
 void PrepareSpikes(){
  spikes=Object.FindAnyObjectByType<WhiteboxSpikes>();if(spikes==null)return;
  var p=spikes.transform.position;
  spikeBase=Box("Spike base / warning strip",p+new Vector3(0,.025f,-1.6f),new Vector3(spikes.width,.05f,.6f),Color.cyan);spikeBase.SetParent(spikes.transform,true);
  for(int i=0;i<7;i++){
   var tooth=new GameObject("Spike tooth "+i);tooth.transform.SetParent(spikes.transform,false);tooth.transform.localPosition=new Vector3(-spikes.width/2+.2f+i*(spikes.width-.4f)/6,0,-1.6f);
   var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-.15f,0,-.18f),new Vector3(.15f,0,-.18f),new Vector3(.15f,0,.18f),new Vector3(-.15f,0,.18f),new Vector3(0,spikes.height,0)};mesh.triangles=new[]{0,4,1,1,4,2,2,4,3,3,4,0,0,1,2,0,2,3};mesh.RecalculateNormals();tooth.AddComponent<MeshFilter>().sharedMesh=mesh;tooth.AddComponent<MeshRenderer>().sharedMaterial=Mat(new Color(1,.25f,.12f));spikeTeeth.Add(tooth.transform);
  }
 }
 void TickSpikes(float dt){
  if(spikes==null)return;spikeTime+=dt;
  int cycle=Mathf.FloorToInt(spikeTime/SpikeCycle);
  if(inLower||SpikePhase()!=2||spikeHitCycle==cycle||inv>0||DodgeInvulnerable()||!SpikeRect().Overlaps(PlayerBody()))return;
  spikeHitCycle=cycle;hp=Mathf.Max(0,hp-spikes.damage);inv=tuning.hitInvulnerability;message="地刺命中 -"+spikes.damage+"：观察黄灯，等待回缩或跳过。";Debug.Log("SPIKE_HIT damage="+spikes.damage+" hp="+hp);
 }
 void VisualSpikes(){
  if(spikes==null)return;int phase=SpikePhase();
  spikeBase.GetComponent<Renderer>().sharedMaterial.color=phase==0?new Color(.2f,.85f,.7f):phase==1?new Color(1,.7f,.1f):phase==2?new Color(1,.18f,.08f):new Color(.35f,.4f,.45f);
  float scale=phase==2?1:phase==3?1-(Mathf.Repeat(spikeTime,SpikeCycle)-spikes.safeSeconds-spikes.warningSeconds-spikes.activeSeconds)/spikes.retractSeconds:.02f;
  foreach(var tooth in spikeTeeth)tooth.localScale=new Vector3(1,Mathf.Clamp(scale,.02f,1),1);
 }
 IEnumerator VerifySpikes(){
  yield return null;ResetAll();started=true;PrepareLower();Assert(spikes!=null,"upper scene spike module exists");Assert(spikes.transform.position.x>24&&spikes.transform.position.x<127,"trap between ram door and final gate");
  x=spikes.transform.position.x;y=0;hp=100;inv=0;roll=-1;spikeTime=0;TickSpikes(.02f);Assert(hp==100&&SpikePhase()==0,"retracted phase is safe");
  spikeTime=1.6f;TickSpikes(.02f);Assert(hp==100&&SpikePhase()==1,"warning phase is harmless");
  spikeTime=2.21f;TickSpikes(.02f);Assert(hp==80,"active spikes deal 20");inv=0;TickSpikes(.02f);Assert(hp==80,"one hit per extension");
  spikeTime=2.21f;spikeHitCycle=-1;inv=0;hp=100;y=1;TickSpikes(.02f);Assert(hp==100,"jump above spike rectangle is safe");
  y=0;roll=.15f;TickSpikes(.02f);Assert(hp==100,"dodge iframe avoids spikes");roll=-1;inv=.5f;TickSpikes(.02f);Assert(hp==100,"post-hit immunity applies to spikes");
  inv=0;x=spikes.transform.position.x+spikes.width/2+tuning.playerHalfWidth+.01f;TickSpikes(.02f);Assert(hp==100,"outside spike bounds is safe");
  x=spikes.transform.position.x;spikeTime=2.91f;TickSpikes(.02f);Assert(hp==100&&SpikePhase()==3,"retraction has no damage");
  inLower=true;spikeTime=2.21f;TickSpikes(.02f);Assert(hp==100,"upper trap cannot hurt underground player");inLower=false;
  paused=true;float before=spikeTime;Tick(.2f,0,false,true,false,false);Assert(spikeTime==before,"pause freezes trap cycle");paused=false;
  ResetAll();Assert(spikeTime==0&&spikeHitCycle==-1,"restart resets trap cycle");
  started=true;x=spikes.transform.position.x-3;y=0;vy=0;grounded=true;spikeTime=2.21f;spikeHitCycle=-1;int beforeHP=hp;
  for(int i=0;i<57;i++){Tick(.02f,1,i==0,true,false,false);Visual(.02f);if(i%3==0)yield return null;}Assert(x>spikes.transform.position.x+spikes.width/2&&hp==beforeHP,"control-driven jump crosses whole trap safely");
  x=48.5f;y=0;spikeTime=2.4f;transitionFade=0;showHitboxes=true;SyncPresentation();Visual(.02f);yield return null;ScreenCapture.CaptureScreenshot(RootPath+"upper-spikes-verified.png");yield return null;
  Debug.Log("SPIKE_VERIFY_OK passed="+passedChecks+" failed="+failedChecks);System.IO.File.WriteAllText(RootPath+"spikes-verified.json","{\"passed\":"+passedChecks+",\"failed\":"+failedChecks+"}");yield return new WaitForSeconds(.3f);Application.Quit();
 }
}

