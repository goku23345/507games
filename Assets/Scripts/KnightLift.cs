using UnityEngine;
using System.Collections;
public partial class KnightLevel {
 WhiteboxLift lift;float liftTime;bool liftCrossed;
 void PrepareLift(){foreach(var m in lowerModules){var l=m.GetComponent<WhiteboxLift>();if(l!=null){lift=l;break;}}}
 float LiftHeight(float time){float travel=(lift.top-lift.bottom)/lift.speed;float cycle=2*(travel+lift.stopSeconds);float t=Mathf.Repeat(time,cycle);if(t<lift.stopSeconds)return lift.bottom;t-=lift.stopSeconds;if(t<travel)return lift.bottom+t*lift.speed;t-=travel;if(t<lift.stopSeconds)return lift.top;t-=lift.stopSeconds;return lift.top-t*lift.speed;}
 void TickLift(float dt){
  if(lift==null)return;float old=lift.transform.position.y;liftTime+=dt;float next=LiftHeight(liftTime);
  if(inLower&&grounded&&Mathf.Abs(y-old)<.001f&&Mathf.Abs(x-lift.transform.position.x)<1.65f)y+=next-old;
  var pos=lift.transform.position;pos.y=next;lift.transform.position=pos;RefreshLowerSurfaces();
 }
 void LiftWall(){
  if(!inLower||lift==null)return;
  if(y<-13.61f){if(previousX<=113.7f&&x>113.7f)x=113.7f;else if(previousX>=114.9f&&x<114.9f)x=114.9f;}
  if(previousX<114.7f&&x>=114.7f&&y>=-13.61f&&!liftCrossed){liftCrossed=true;message="升降台通过！向右落入Boss场。";Debug.Log("MANDATORY_LIFT_CROSSED");}
 }
 IEnumerator VerifyLift(){
  yield return null;PrepareLower();ResetAll();started=true;EnterLower();x=111.5f;y=-18;vy=0;grounded=true;transitionFade=0;
  Assert(lift!=null,"authored elevator exists");Assert(lowerModules.Count==20,"boss and elevator platform modules preserved");
  Assert(Mathf.Abs(LiftHeight(0)+18)<.001f&&Mathf.Abs(LiftHeight(1)+18)<.001f,"bottom dwell 1.2 seconds");
  Assert(Mathf.Abs(LiftHeight(3.95f)+13.6f)<.001f&&Mathf.Abs(LiftHeight(5)+13.6f)<.001f,"top dwell and 4.4 meter travel");
  previousX=113.5f;x=114.5f;y=-18;LiftWall();Assert(x==113.7f,"ground route cannot bypass elevator");previousX=113.5f;x=114.5f;y=-15.4f;LiftWall();Assert(x==113.7f,"ordinary ground jump cannot bypass high wall");
  x=111.5f;y=-18;grounded=true;liftTime=0;lift.transform.position=new Vector3(111.5f,-18,0);RefreshLowerSurfaces();
  for(int i=0;i<205;i++){Tick(.02f,0,false,true,false,false);Visual(.02f);if(i%3==0)yield return null;}
  Assert(Mathf.Abs(y+13.6f)<.06f&&grounded,"rider is carried to top without jump");
  for(int i=0;i<42;i++){Tick(.02f,1,i==0,true,false,false);Visual(.02f);if(i%3==0)yield return null;}
  Assert(liftCrossed&&x>114.7f,"player jumps from lift across mandatory upper exit");
  for(int i=0;i<60;i++){Tick(.02f,0,false,true,false,false);if(i%3==0)yield return null;}
  Assert(grounded&&Mathf.Abs(y+13.6f)<.06f,"upper landing is playable");
  x=111.5f;y=-16;vy=-2;grounded=false;for(int i=0;i<250;i++){Tick(.02f,0,false,true,false,false);if(i%3==0)yield return null;}Assert(hp>0&&y>=-18,"missed ride has safe ground below");
  x=111.5f;y=-18;grounded=true;liftTime=7.88f;var lowPos=lift.transform.position;lowPos.y=LiftHeight(liftTime);lift.transform.position=lowPos;TickLift(.02f);Assert(y>=-18,"descending lift does not pull a ground rider through floor");
  paused=true;float before=liftTime;Tick(.5f,0,false,true,false,false);Assert(liftTime==before,"pause stops elevator");paused=false;
  ResetAll();Assert(!liftCrossed&&Mathf.Abs(lift.transform.position.y+18)<.001f,"restart resets elevator phase");
  EnterLower();x=111.5f;y=-13.6f;grounded=true;liftTime=4;TickLift(0);transitionFade=0;showHitboxes=false;SyncPresentation();Visual(.02f);yield return null;ScreenCapture.CaptureScreenshot(RootPath+"mandatory-lift-verified.png");yield return null;
  Debug.Log("LIFT_VERIFY_OK passed="+passedChecks+" failed="+failedChecks);System.IO.File.WriteAllText(RootPath+"lift-verified.json","{\"passed\":"+passedChecks+",\"failed\":"+failedChecks+"}");yield return new WaitForSeconds(.3f);Application.Quit();
 }
}
