//remark:Conversion from conditional string
//options:--microsoft --c++11;fp:--c++11;fn

int f();
void test(char * x) {
    char * s = f() ? x : "bar";
}
