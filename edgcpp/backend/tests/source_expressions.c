/* Local expression plans must preserve both source syntax and consumed results. */
struct Bits { unsigned ready:1; unsigned mode:3; signed delta:4; unsigned guard:8; };
struct __attribute__((packed)) Split { unsigned char before; unsigned long long low:3; unsigned long long value:64; unsigned tail:5; unsigned char after; };
static struct Bits bits;
static int calls, scalar, trace;
static struct Bits *receiver(void) { ++calls; trace=trace*10+2; return &bits; }
static unsigned argument(unsigned value) { trace=trace*10+1; return value; }
static int change_scalar(void) { scalar=20; return 3; }
static int combine(int a,int b) { return a*100+b; }
static void store_scalar(int value) { scalar=value; }
void readable_assignments(struct Bits *flags,int *value) {
 flags->ready=1;flags->mode+=2;flags->delta=-3;*value+=2;(*value)++;
}
static int bitfield_values(void) {
 struct Bits local={0,0,0,213}; int result;
 readable_assignments(&local,&scalar);
 if(local.ready!=1 || local.mode!=2 || local.delta!=-3 || local.guard!=213)return 1;
 result=(local.mode=11);if(result!=3 || local.mode!=3)return 2;
 result=(local.delta=15);if(result!=-1 || local.delta!=-1)return 3;
 local.mode=7;result=++local.mode;if(result!=0 || local.mode!=0)return 4;
 local.mode=7;result=local.mode++;if(result!=7 || local.mode!=0)return 5;
 local.mode=0;result=--local.mode;if(result!=7 || local.mode!=7)return 6;
 local.mode=7;result=(local.mode/=-1);if(result!=1 || local.mode!=1)return 7;
 local.mode=7;result=(local.mode%=-3);if(result!=1 || local.mode!=1)return 8;
 local.delta=-3;result=(local.delta+=0.5);if(result!=-2 || local.delta!=-2)return 9;
 result=(local.delta=-3.75);if(result!=-3 || local.delta!=-3)return 10;
 if(local.guard!=213)return 11;
 return 0;
}
static int branch_values(void) {
 bits.mode=0;calls=trace=0;
 int choice=scalar-scalar;
 int result=choice && (receiver()->mode=11);if(result || calls)return 1;
 result=!choice || (receiver()->mode=11);if(result!=1 || calls)return 2;
 result=choice ? (receiver()->mode=12) : (receiver()->mode=13);
 if(result!=5 || calls!=1)return 3;
 int loops=0;while((bits.mode=bits.mode+1)<7)++loops;
 if(loops!=1 || bits.mode!=7)return 4;
 result=combine((bits.mode=10),(bits.delta=7));if(result!=207)return 5;
 result=(bits.mode=1,bits.mode+=9);if(result!=2 || bits.mode!=2)return 6;
 return 0;
}
static int split_values(void) {
 struct Split s;unsigned char *bytes=(unsigned char*)&s;
 for(unsigned i=0;i<sizeof s;++i)bytes[i]=0xa5;
 s.before=0x5a;s.low=5;s.tail=19;s.after=0x3c;
 s.value=0xfedcba9876543210ULL;
 if(s.value!=0xfedcba9876543210ULL || s.before!=0x5a || s.low!=5 || s.tail!=19 || s.after!=0x3c)return 1;
 s.value+=1;if(s.value!=0xfedcba9876543211ULL)return 2;
 return 0;
}
static int numeric_conversions(void) {
 int negative=-1;unsigned one=1;long long minus=-1;unsigned long long wide_one=1;
 if(negative<one || minus<wide_one)return 1;
 struct Bits local={0,7,-3,0};if(!(local.mode>negative) || !(local.delta<negative))return 2;
 unsigned long long wide_unsigned=9;int amount=-3;wide_unsigned+=amount;
 if(wide_unsigned!=6)return 3;
 long long wide_signed=-4;unsigned long long add=2;wide_signed+=add;
 if(wide_signed!=-2)return 4;
 unsigned shifted=3;unsigned long long shift=2;shifted<<=shift;
 if(shifted!=12)return 5;
 unsigned char byte_value=0;int mask=257;byte_value|=mask;
 if(byte_value!=1)return 6;
 int narrow=-2;long wide=3;narrow+=wide;if(narrow!=1)return 7;
 unsigned uvalue=3;long decrement=-1;uvalue+=decrement;if(uvalue!=2)return 8;
 float single=1.5f;double fraction=0.25;single+=fraction;if(single!=1.75f)return 9;
 int incoming=513;byte_value=incoming;if(byte_value!=1)return 10;
 (scalar=3,store_scalar(7));if(scalar!=7)return 11;
 (scalar==7?store_scalar(9):store_scalar(11));if(scalar!=9)return 12;
 (scalar!=9?store_scalar(13):store_scalar(15));if(scalar!=15)return 13;
 (scalar==15?(void)0:store_scalar(17));if(scalar!=15)return 14;
 return 0;
}
struct Stride { long long words[3]; };
static struct Stride *cursor;
static struct Stride **cursor_address(void) { ++calls;trace=trace*10+2;return &cursor; }
static int displacement(int count) { trace=trace*10+1;return count; }
static int move_cursor(void) { cursor+=2;return -1; }
static int pointer_compounds(void) {
 struct Stride storage[8];struct Stride *position=storage,*result;
 position+=2;if(position!=storage+2)return 1;
 result=(position+=3);if(position!=storage+5 || result!=position)return 2;
 position-=2;if(position!=storage+3)return 3;
 result=(position-=1);if(result!=storage+2 || position!=result)return 4;
 position+=-1;if(position!=storage+1)return 5;
 result=(position-=-2);if(result!=storage+3)return 6;
 struct Holder { struct Stride *position; } holder;
 holder.position=(position+=2);if(holder.position!=storage+5 || position!=holder.position)return 7;
 int matrix[6][3];int (*rows)[3]=matrix;int (*saved)[3];
 rows+=2;if(rows!=matrix+2)return 8;
 saved=(rows+=2);if(saved!=matrix+4 || rows!=saved)return 9;
 rows-=3;if(rows!=matrix+1)return 10;
 saved=(rows-=-2);if(saved!=matrix+3 || rows!=saved)return 11;
 long signed_count=-1;unsigned count=2;rows+=signed_count;rows+=count;
 if(rows!=matrix+4)return 12;
 cursor=storage;calls=trace=0;result=(*cursor_address()+=displacement(3));
 if(result!=storage+3 || cursor!=result || calls!=1)return 13;
#ifdef __cplusplus
 if(trace!=12)return 14;
 (cursor+=1)-=2;if(cursor!=storage+2)return 15;
 cursor=storage;cursor+=move_cursor();if(cursor!=storage+1)return 16;
#endif
 return 0;
}
#ifdef __cplusplus
static int sequencing(void) {
 scalar=1;scalar+=change_scalar();if(scalar!=23)return 1;
 bits.mode=0;trace=calls=0;
 int result=(receiver()->mode=argument(11));
 if(result!=3 || trace!=12 || calls!=1)return 2;
 trace=calls=0;result=((receiver()->mode=argument(7))+=argument(3));
 if(result!=2 || bits.mode!=2 || trace!=112 || calls!=1)return 3;
 trace=calls=0;result=((++receiver()->mode)=argument(14));
 if(result!=6 || bits.mode!=6 || trace!=12 || calls!=1)return 4;
 return 0;
}
#endif
int main(void) {
 int result=bitfield_values();if(result)return 10+result;
 result=branch_values();if(result)return 30+result;
 result=split_values();if(result)return 50+result;
 result=numeric_conversions();if(result)return 60+result;
 result=pointer_compounds();if(result)return 90+result;
#ifdef __cplusplus
 result=sequencing();if(result)return 70+result;
#endif
 return 0;
}
