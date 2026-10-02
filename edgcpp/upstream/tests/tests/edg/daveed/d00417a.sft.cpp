//remark:GNU __builtin_classify_type
//type:rp
//name:
//options:--gcc --c99 --gnu_version=30300:--gcc --c99 --gnu_version=30400:--g++ --gnu_version=30300:--g++ --gnu_version=30400
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

#ifdef __cplusplus
extern "C"
#endif
int printf(char const*, ...);

#define P(T) printf("%20s %d\n", #T, __builtin_classify_type(T))

enum E { enum_ };
struct S { int i; } s;
#ifdef __cplusplus
class C { public: void f(); } c;
#endif
union U { int i; } u;

int main() {
	P((int)0);
	P((unsigned char)'a');
	P((enum E)0);
#ifdef __cplusplus
	P((bool)0);
#endif
	P((void*)0);
#ifdef __cplusplus
	P((int&)s);
	P(&S::i);
#endif
	P(1.2);
	P((_Complex float)(3.0+2.0i));
	P(*(void(*)())0);
#ifdef __cplusplus
	//P(C::f);
	P(&C::f);
#endif
	P(s);
#ifdef __cplusplus
	P(c);
#endif
	P(u);
	P(((int[2]){1, 2}));
	P("Hi");
	P(((char const[2]){'a', '\0'}));
	return 0;
}
