static int first_calls,expected_calls;
static long value(long x){++first_calls;return x;}
static long expected(void){++expected_calls;return 1;}
int main(void){
 if(__builtin_expect(value(-7),1)!=-7 || first_calls!=1)return 1;
 if(__builtin_expect(value(0x123456789abcdefL),0)!=0x123456789abcdefL || first_calls!=2)return 2;
 if(__builtin_expect(value(-9223372036854775807L-1),1)!=(-9223372036854775807L-1) || first_calls!=3)return 3;
 if(__builtin_expect(value(5),expected())!=5 || first_calls!=4 || expected_calls!=1)return 4;
 volatile long observed=-2;if(__builtin_expect(observed,0)!=-2)return 5;
 return 0;
}
