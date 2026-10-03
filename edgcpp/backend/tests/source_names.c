/* Source identities remain distinct across C/C++ and C# collision domains. */
typedef struct handle handle;
struct Core { int value; };
struct same { int event; };
struct Item { int value; unsigned weight; };
struct PropertyNames { unsigned ready:1; int get_ready; int set_ready; };
static int property=12;
int get_property(void){return property;}
enum Mode { ModeSkip=-1, ModeAdd=2, ModeDouble=4 };
static int adjustment=3;
static int _=17;
#ifdef __cplusplus
static thread_local int current=5;
#else
static _Thread_local int current=5;
#endif
static int contribution(int value) { return value+adjustment+current; }
int keyword_names(int params,int out) { int lock=params+out; return lock; }
int same(int value) { struct same item={value}; return item.event; }
int Run(int value) { return value+1; }
int Shutdown(int value) { return value+2; }
int Core(int value) { struct Core core={value}; return core.value+3; }
int f123(int value) { return value+4; }
handle *echo_handle(handle *value) { return value; }
int named_labels(int value) { if(value)goto event;return 0;event:return value; }
int shadowing(int value) { int total=value;{int value=7;total+=value;}return total+value; }
int mode_value(enum Mode mode) { switch(mode){case ModeSkip:return -1;case ModeAdd:return 2;case ModeDouble:return 4;default:return 0;} }
int *global_address(void) { return &adjustment; }
int *tls_address(void) { return &current; }
int underscore_parameter(int _) { (_+8); {int _=9;(_+4);if(_!=9)return -1;}return _; }
int underscore_global(void) {(_+5);return _;}
/* Unrelated owner declarations must not rename source parameters/locals. */
static int p=19, a=23, p0=29;
int a0(int value){return value+2;}
int adapter_shadow(int value){int (*callback)(int)=a0;return callback(value);}
int sqlite3PutVarint(unsigned char *p,unsigned long long v){*p=(unsigned char)v;return *p;}
int local_name_preservation(void){int a=7;return a;}
int wrapper_collision(int wrapper_collision__body){return wrapper_collision__body+1;}
int type_name_shadow(void){int Mode=3,Core_Mode=4;struct Item Item={5,6};return Mode+Core_Mode+Item.value+ModeAdd;}
int enum_type_shadow(enum Mode value){int Mode=3,Core_Mode=4;switch(value){case ModeAdd:return Mode+Core_Mode;default:return 0;}}
int helper_shadow(int value){return ({int captured=value;captured+p0+a0(captured);});}
#ifdef __cplusplus
int actual_owner_shadow(void){int p=3,a0=4;return ::p+p+::a0(a0);}
#endif
int source_names_check(void) {
  unsigned char byte=0;
  if(sqlite3PutVarint(&byte,37)!=37 || byte!=37 || local_name_preservation()!=7 || wrapper_collision(9)!=10 || type_name_shadow()!=14 || helper_shadow(4)!=39 || adapter_shadow(4)!=6 || enum_type_shadow(ModeAdd)!=7)return 8;
#ifdef __cplusplus
  if(actual_owner_shadow()!=28)return 9;
#endif
  if(p!=19 || a!=23)return 10;
  struct Item item={4,2};
  struct PropertyNames names={1,7,8};
  if(!names.ready || names.get_ready!=7 || names.set_ready!=8 || get_property()!=12)return 7;
  int adjustment=11;
  *global_address()=6;*tls_address()=8;
  if(contribution(item.value)!=18 || adjustment!=11)return 1;
  if(keyword_names(3,4)!=7 || same(9)!=9 || Run(2)!=3 || Shutdown(2)!=4 || Core(2)!=5 || f123(2)!=6)return 2;
  if(named_labels(7)!=7 || shadowing(3)!=13 || echo_handle((handle*)0)!=(handle*)0)return 3;
  if(mode_value(ModeSkip)!=-1 || mode_value(ModeAdd)!=2 || mode_value(ModeDouble)!=4)return 4;
  if(underscore_parameter(3)!=3 || underscore_global()!=17)return 6;
  return 0;
}
#ifdef __cplusplus
namespace Left { template<class T> T twice(T value){return value+value;} struct Item {int value;}; int overload(int value){Item item={value};return item.value;} }
namespace Right { struct Item {int value;}; int overload(int value){Item item={value};return item.value+1;} }
int overload(int value){return value;}
int overload(int first,int second){return first+second;}
#endif
int main(void) {
  int result=source_names_check();if(result)return result;
#ifdef __cplusplus
  if(Left::overload(5)!=5 || Right::overload(5)!=6 || overload(3)!=3 || overload(3,4)!=7 || Left::twice(6)!=12)return 5;
#endif
  return 0;
}
