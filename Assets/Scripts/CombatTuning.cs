using UnityEngine;
[CreateAssetMenu(menuName="白模/战斗数值")]
public class CombatTuning : ScriptableObject {
 [Header("生命与伤害")]
 public int playerHP=100,enemyHP=60,bossHP=240;
 public int attackDamage=20,thirdAttackDamage=30,enemyDamage=20,bossDamage=25,ramDamage=30;
 public float counterMultiplier=1.5f;
 [Header("移动与闪避")]
 public float walkSpeed=5.2f,jumpVelocity=11.5f,gravity=24;
 public float dodgeSpeed=8,dodgeDuration=.6f,dodgeInvStart=.08f,dodgeInvEnd=.42f,hitInvulnerability=.65f;
 [Header("玩家攻击：秒与米")]
 public float attackWindup=.12f,attackActiveEnd=.24f,attackDuration=.52f,thirdAttackDuration=.6f;
 public float attackNear=.15f,attackReach=1.85f,attackBottom=.25f,attackHeight=1.5f;
 public float playerHalfWidth=.3f,playerBodyBottom=.05f,playerBodyHeight=1.7f;
 public float enemyHalfWidth=.35f,enemyBodyBottom=.05f,enemyBodyHeight=1.8f;
 [Header("Boss：冲撞 / 地波 / 落点") ]
 public float bossChargeWindup=.95f,bossWaveWindup=1.05f,bossSlamWindup=1.25f;
 public float bossChargeSpeed=10,bossChargeDuration=.65f,bossWaveDuration=.28f,bossSlamDuration=.2f,bossRecovery=1.6f;
 public int bossChargeDamage=25,bossWaveDamage=20,bossSlamDamage=30;
 [Header("敌人攻击")]
 public float enemyChase=1.6f,enemyWindup=.8f,enemyActive=.2f,enemyRecovery=1.3f;
 public float enemyAttackNear=.1f,enemyAttackReach=2.05f,enemyAttackBottom=.2f,enemyAttackHeight=1.6f;
}
