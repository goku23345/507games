using UnityEngine;
using System.Collections;
public partial class KnightLevel {
 string BossLabel(Foe b){return (b.state==3?"恢复：可以反击":b.state==0?"观察":b.state==1?"预警：":"攻击：")+(b.state==0||b.state==3?"":b.bossMove==0?"横向冲撞":b.bossMove==1?"地面冲击：跳跃":"锁定落点：换位");}
 float BossSupport(float px,float feet){float top=-18;foreach(var p in lowerPlatforms)if(px>=p.left&&px<=p.right&&p.top<=feet+.1f)top=Mathf.Max(top,p.top);return top;}
 float BossWindup(Foe b){return b.bossMove==0?tuning.bossChargeWindup:b.bossMove==1?tuning.bossWaveWindup:tuning.bossSlamWindup;}
 float BossActive(Foe b){return b.bossMove==0?tuning.bossChargeDuration:b.bossMove==1?tuning.bossWaveDuration:tuning.bossSlamDuration;}
 Rect BossDanger(Foe b){
  if(b.bossMove==0)return b.state==1?new Rect(Mathf.Min(b.x,b.x+b.face*tuning.bossChargeSpeed*tuning.bossChargeDuration)-.65f,b.y+.1f,tuning.bossChargeSpeed*tuning.bossChargeDuration+1.3f,1.8f):new Rect(b.x-.65f,b.y+.1f,1.3f,1.8f);
  if(b.bossMove==1)return new Rect(118,b.y,40,.75f);
  return new Rect(b.targetX-1.4f,b.targetY,2.8f,2.1f);
 }
 void BossDamage(Foe b,Rect danger){
  if(b.ramHit||inv>0||DodgeInvulnerable()||!danger.Overlaps(PlayerBody()))return;
  b.ramHit=true;int amount=b.bossMove==0?tuning.bossChargeDamage:b.bossMove==1?tuning.bossWaveDamage:tuning.bossSlamDamage;
  hp=Mathf.Max(0,hp-amount);inv=tuning.hitInvulnerability;message="Boss 命中 -"+amount+"，留意橙色预兆。";
  Debug.Log("BOSS_HIT move="+b.bossMove+" damage="+amount);
 }
 void TickBoss(Foe b,float dt){
  if(!bossEngaged){if(x<119||x>157.5f)return;bossEngaged=true;lowerRespawnX=118.7f;message="进入多层Boss场：橙色预警，红色生效；恢复时反击。";}
  b.y=Mathf.Max(BossSupport(b.x,b.y),b.y-12*dt);
  b.t+=dt;
  if(b.state==0){
   b.face=x>=b.x?1:-1;
   if(b.t<.6f)return;
   b.bossMove=b.bossCycle++%3;b.targetX=Mathf.Clamp(x,119.5f,156.5f);b.targetY=BossSupport(x,y);b.state=1;b.t=0;b.ramHit=false;b.anim.CrossFadeInFixedTime("Idle",.08f);
  }else if(b.state==1&&b.t>=BossWindup(b)){
   b.state=2;b.t=0;b.ramHit=false;if(b.bossMove==2){b.x=b.targetX;b.y=b.targetY;}b.anim.CrossFadeInFixedTime("Attack",.08f);
  }else if(b.state==2){
   Rect danger=BossDanger(b);
   if(b.bossMove==0){float old=b.x;b.x=Mathf.Clamp(b.x+b.face*tuning.bossChargeSpeed*dt,118.7f,157.3f);danger=new Rect(Mathf.Min(old,b.x)-.65f,b.y+.1f,Mathf.Abs(b.x-old)+1.3f,1.8f);}
   BossDamage(b,danger);
   if(b.t>=BossActive(b)){b.state=3;b.t=0;b.anim.CrossFadeInFixedTime("Idle",.08f);}
  }else if(b.state==3&&b.t>=tuning.bossRecovery){b.state=0;b.t=0;}
 }
 IEnumerator VerifyBoss(){
  yield return null;PrepareLower();ResetAll();EnterLower();x=120;y=-18;grounded=true;
  Foe b=lowerFoes.Find(e=>e.exitGuard);Assert(b!=null&&b.maxHP==240,"boss health tuning 240");
  Assert(lowerModules.Count==20,"eight authored boss platforms added");
  TickBoss(b,.02f);Assert(bossEngaged,"arena entry starts encounter");
  hp=100;inv=0;roll=-1;b.x=125;b.y=-18;b.state=1;b.bossMove=0;b.t=0;x=124;y=-18;TickBoss(b,.1f);Assert(hp==100,"boss telegraph is harmless");
  b.state=2;b.t=0;b.face=-1;b.ramHit=false;x=124.4f;TickBoss(b,.02f);Assert(hp==75,"swept charge deals 25");TickBoss(b,.02f);Assert(hp==75,"charge hits once");
  b.bossMove=1;b.state=2;b.t=0;b.ramHit=false;hp=100;inv=0;x=121;y=-16;TickBoss(b,.02f);Assert(hp==100,"upper platform avoids floor shock");
  y=-18;TickBoss(b,.02f);Assert(hp==80,"floor wave deals 20");
  b.bossMove=2;b.state=1;b.t=0;b.targetX=132;b.targetY=-15.8f;hp=100;inv=0;x=136;y=-15.8f;TickBoss(b,1.26f);TickBoss(b,.02f);Assert(hp==100&&b.x==132&&b.y==-15.8f,"slam locks old landing and can reach upper tier");
  x=132;y=-15.8f;b.ramHit=false;TickBoss(b,.02f);Assert(hp==70,"slam deals 30");
  hp=100;inv=0;roll=.15f;b.ramHit=false;TickBoss(b,.02f);Assert(hp==100,"dodge iframe avoids boss");roll=-1;
  b.state=3;b.t=0;b.hp=240;x=b.x-1.5f;y=b.y;face=1;atk=.15f;combo=1;swingHits.Clear();AttackHit();Assert(b.hp==210,"recovery counter deals 30");AttackHit();Assert(b.hp==210,"one hit per swing");
  b.state=0;b.t=0;b.bossCycle=0;int seen=0;
  for(int i=0;i<850;i++){hp=100;inv=2;TickBoss(b,.02f);if(b.state==2)seen|=1<<b.bossMove;}
  Assert(seen==7,"all three moves cycle through active states");
  b.state=3;b.hp=20;b.x=125;b.y=-18;x=123.5f;y=-18;face=1;atk=.15f;combo=1;swingHits.Clear();AttackHit();Assert(lowerCleared,"boss defeat unlocks archive");
  x=lowerStoryX;CollectStoryItem();Assert(storyItemOwned,"archive granted after boss");x=lowerReturnX;UseLowerInteraction();Assert(!inLower&&storyItemOwned,"archive exit returns to surface");
  EnterLower();Respawn();Assert(storyItemOwned&&lowerCleared,"death keeps defeated boss and archive");ResetAll();Assert(!bossEngaged&&!storyItemOwned&&b.hp==240,"restart restores boss encounter");
  EnterLower();x=123;y=-18;bossEngaged=true;b.x=128;b.y=-18;b.state=1;b.bossMove=1;b.t=.5f;showHitboxes=true;transitionFade=0;SyncPresentation();Visual(.02f);yield return null;ScreenCapture.CaptureScreenshot(RootPath+"boss-arena-verified.png");yield return null;
  Debug.Log("BOSS_VERIFY_OK passed="+passedChecks+" failed="+failedChecks);System.IO.File.WriteAllText(RootPath+"boss-verified.json","{\"passed\":"+passedChecks+",\"failed\":"+failedChecks+"}");yield return new WaitForSeconds(.3f);Application.Quit();
 }
}
