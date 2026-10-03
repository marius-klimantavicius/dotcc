struct Bits { unsigned char tag:1; unsigned char size:7; };
struct ShortString { Bits bits; char padding[0]; char data[23]; };
struct Enclosed { int before; ShortString text; int after; };
static ShortString global_zero{};
static ShortString global_value{{1,3},{},{'a','b','c'}};
static Enclosed enclosed{17,{{0,2},{},{'o','k'}},29};
static thread_local ShortString thread_zero{};
static unsigned calls;
static ShortString dynamic_value() { ++calls;return ShortString{{1,1},{},{'x'}}; }
static ShortString dynamic_global=dynamic_value();
static ShortString &local_static() { static ShortString value{};return value; }
template<class T> struct SizedPadding { Bits bits;char padding[sizeof(T)-1];T data[3]; };
static SizedPadding<char> character{{1,2},{},{'a','b'}};
static SizedPadding<unsigned short> word{{0,3},{19},{101,202,303}};
static SizedPadding<unsigned> integer{{1,3},{11,12,13},{1001,2002,3003}};
static int blank(const ShortString &value) {
 if(value.bits.tag||value.bits.size)return 0;
 for(unsigned i=0;i<sizeof value.data;++i)if(value.data[i])return 0;
 return 1;
}
int main() {
 ShortString local{};
 ShortString values[3]={{},{ {1,2},{},{'A','B'} },{}};
 if(sizeof(ShortString)!=24 || sizeof(Bits)!=1)return 1;
 if(!blank(global_zero)||!blank(thread_zero)||!blank(local)||!blank(local_static()))return 2;
 if(!blank(values[0])||!blank(values[2])||values[1].bits.tag!=1||values[1].bits.size!=2||values[1].data[0]!='A'||values[1].data[1]!='B')return 3;
 if(global_value.bits.tag!=1||global_value.bits.size!=3||global_value.data[0]!='a'||global_value.data[2]!='c'||global_value.data[3])return 4;
 if(enclosed.before!=17||enclosed.after!=29||enclosed.text.bits.size!=2||enclosed.text.data[1]!='k')return 5;
 if(calls!=1||dynamic_global.data[0]!='x'||dynamic_global.bits.size!=1)return 6;
 if(character.data[1]!='b'||word.padding[0]!=19||word.data[2]!=303||integer.padding[2]!=13||integer.data[1]!=2002)return 7;
 if((void*)local.padding!=(void*)local.data || (char*)local.data-(char*)&local!=1)return 8;
 ShortString copied=global_value;local=copied;local.data[1]='Q';
 if(global_value.data[1]!='b'||copied.data[1]!='b'||local.data[1]!='Q')return 9;
 global_zero.data[0]=11;thread_zero.data[0]=12;local_static().data[0]=13;
 return 0;
}
