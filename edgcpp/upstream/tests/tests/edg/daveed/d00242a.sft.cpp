//remark:Microsoft __alignof results
//type:rp
//name:
//options:
//options_all:--microsoft
//cases:
//source_files:
//input_files:  
//output_files:
//ulimit:
//linker_options:
//execution_args:

extern "C" int printf(char const*, ...);

struct A {
        int a;
} a;
#define __alignof__ __alignof

struct X;
template<typename T> struct XX;

int main() {
        printf("%d\n", __alignof__(a));
//        printf("%d\n", __alignof__(a).a);
//        printf("%d\n", __alignof__ a);
        printf("%d\n", __alignof__(int));
        printf("%d\n", __alignof__(void));
        printf("%d\n", __alignof__(X));
        printf("%d\n", __alignof__(XX<int>));
        return __alignof(a) == 0 || __alignof(void) == 0 || __alignof(X) != 0;
}
