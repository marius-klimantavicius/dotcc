/* Enum source types must survive while EDG integer promotions retain C ABI. */
#include <stdarg.h>
enum Mode { ModeSkip=-1, ModeAdd=2, ModeDouble=4 };
typedef enum { AliasFirst=3, AliasSecond=6 } Alias;
enum Flag { FlagNone=0, FlagRead=1, FlagWrite=2, FlagAll=7 };
struct State { enum Mode mode; enum Flag flags:3; Alias alias; };
static enum Mode default_mode=ModeAdd;
static enum Mode zero_mode;
#ifdef __cplusplus
static thread_local enum Mode thread_mode=ModeDouble;
#else
static _Thread_local enum Mode thread_mode=ModeDouble;
#endif
enum Mode enum_api(enum Mode value){return value==ModeAdd?ModeDouble:ModeSkip;}
#ifndef __cplusplus
static enum Mode implicit_return(int value){return value;}
static int implicit_argument(enum Mode value){return value;}
#endif
static enum Mode choose_mode(enum Mode mode) { return mode==ModeSkip?ModeDouble:mode; }
static enum Mode *mode_address(void) { return &default_mode; }
static int enum_varargs(int count,...) {
  va_list args;va_start(args,count);int total=0;
  for(int i=0;i<count;++i)total+=va_arg(args,int);
  va_end(args);return total;
}
static int switch_mode(enum Mode mode) {
  switch(mode){case ModeSkip:return -1;case ModeAdd:return 2;case ModeDouble:return 4;case (enum Mode)-2:return -2;default:return 0;}
}
int source_enum_cases(void) {
  struct State state={ModeAdd,FlagRead,AliasFirst};
  enum Mode local=ModeSkip;
  int slots[8]={0,1,2,3,4,5,6,7};
  int *walker=slots;enum Flag distance=FlagWrite;
  if(*(walker+distance)!=2 || *(distance+walker)!=2)return 13;
  int *advanced=(walker+=distance);
  if(*advanced!=2 || *walker!=2)return 14;
  distance=FlagRead;advanced=(walker-=distance);
  if(*advanced!=1 || *walker!=1)return 15;
  const enum Mode fixed=ModeAdd;
  volatile enum Mode watched=ModeDouble;
  enum Mode values[2]={ModeAdd,ModeDouble};
  enum Mode *elements=values;
  if(zero_mode!=(enum Mode)0 || thread_mode!=ModeDouble || fixed!=ModeAdd || watched!=ModeDouble || elements[1]!=ModeDouble || enum_api(fixed)!=ModeDouble)return 6;
  enum Mode (*callback)(enum Mode)=choose_mode;
  enum Mode *pointer=mode_address();
  *pointer=callback(local);
  state.mode=*pointer;state.flags=FlagWrite;state.alias=AliasSecond;
  if(state.mode!=ModeDouble || state.flags!=FlagWrite || state.alias!=AliasSecond)return 1;
  if(enum_varargs(3,local,state.flags,state.alias)!=7)return 2;
  if(switch_mode(state.mode)!=4 || state.mode+state.alias!=10 || switch_mode((enum Mode)-2)!=-2)return 3;
#ifndef __cplusplus
  local=5;
  enum Mode before=local++,after=++local;
  if(before!=5 || after!=7 || local!=7)return 7;
  local=6;state.flags|=FlagRead;
  if(local!=6 || state.flags!=3)return 4;
  local=state.alias;
  if(local!=6 || implicit_return(5)!=5 || implicit_argument(4)!=4)return 8;
  if(-6/FlagWrite!=-3 || -2+FlagRead!=-1 || -1<FlagRead!=1)return 9;
  int side=0;
  enum Mode selected=(state.flags|=FlagWrite)?(++side,ModeAdd):(++side,local);
  if(selected!=ModeAdd || side!=1 || state.flags!=3)return 10;
  selected=side?state.mode:implicit_return(++side);
  if(selected!=ModeDouble || side!=1)return 11;
#else
  local=(enum Mode)5;local=(enum Mode)(local+1);state.flags=(enum Flag)(state.flags|FlagRead);
  if(local!=(enum Mode)6 || state.flags!=(enum Flag)3)return 4;
#endif
  return 0;
}
#ifdef __cplusplus
namespace Model {
enum class Signed:int { Low=-3,High=4 };
int signed_case(){Signed unknown=(Signed)-2;switch(unknown){case (Signed)-2:return 1;default:return 0;}}
enum class Strong:unsigned { One=1,Two=2 };
enum class Wide:unsigned long long { Low=1,High=0x8000000000000000ULL };
struct Scoped { Strong state; Wide wide; };
Strong choose(Strong value){return value==Strong::One?Strong::Two:Strong::One;}
int cases(){if(signed_case()!=1)return 16;Scoped object={Strong::One,Wide::High};object.state=choose(object.state);Strong values[2]={Strong::One,Strong::Two};const Strong* pointer=values;int side=0;Strong selected=object.state==Strong::Two?(++side,object.state):Strong::One;switch(selected){case Strong::Two:break;default:return 12;}return object.state==Strong::Two && pointer[1]==Strong::Two && side==1 && (unsigned long long)object.wide==0x8000000000000000ULL?0:5;}
}
#endif
int main(void) {
  int result=source_enum_cases();if(result)return result;
#ifdef __cplusplus
  return Model::cases();
#else
  return 0;
#endif
}
