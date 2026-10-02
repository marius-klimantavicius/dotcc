//remark:Microsoft __alignof results
//type:rp
//name:
//options:--microsoft --c;rp:--microsoft;rp:-A;fn:--g++;fn
//options_all:
//cases:
//source_files:
//input_files:  
//output_files:
//ulimit:
//linker_options:
//execution_args:

#if defined(__cplusplus)
extern "C"
#endif
int printf(char const*, ...);

extern int a[];
struct S;
extern struct S b[1];

int main() {
	printf("a: %d\n", __alignof(a));
	printf("b: %d\n", __alignof(b));
	printf("int[]: %d\nconst void: %d\n", __alignof(int[]),
                                         __alignof(const void));
  __alignof(struct S);
	return 0;
}

struct S { int i; };
