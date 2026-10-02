//type: rp
//options: 
# 0 "./pr94920-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr94920-1.C"




# 1 "./pr94920.C" 1




typedef int __attribute__((vector_size(4*sizeof(int)))) vint;


__attribute__((noipa)) unsigned int foo(int x) {
    return (x >= 0 ? x : 0) + (x <= 0 ? -x : 0);
}


__attribute__((noipa)) unsigned int corge(int x) {
    int w = (x >= 0 ? x : 0);
    int y = -x;
    int z = (y >= 0 ? y : 0);
    return w + z;
}


__attribute__((noipa)) vint thud(vint x) {
    vint t = (x >= 0 ? x : 0) ;
    vint xx = -x;
    vint t1 = (xx >= 0 ? xx : 0);
    return t + t1;
}


__attribute__((noipa)) int bar(int x) {
    return (x >= 0 ? x : 0) + (x <= 0 ? -x : 0);
}


__attribute__((noipa)) unsigned int baz(int x) {
    return (x <= 0 ? -x : 0) + (x >= 0 ? x : 0);
}


__attribute__((noipa)) unsigned int quux(int x) {
    return (0 <= x ? x : 0) + (0 >= x ? -x : 0);
}


__attribute__((noipa)) unsigned int waldo(int x) {
    return (x >= 4 ? x : 4) + (x <= 4 ? -x : 4);
}


__attribute__((noipa)) unsigned int fred(int x) {
    return (x >= -4 ? x : -4) + (x <= -4 ? -x : -4);
}


__attribute__((noipa)) unsigned int goo(int x) {
    return (x <= 0 ? x : 0) + (x >= 0 ? -x : 0);
}


__attribute__((noipa)) int qux(int x) {
    return (x >= 0 ? x : 0) + (x >= 0 ? x : 0);
}
# 6 "./pr94920-1.C" 2

int main() {

    if (foo(0) != 0
        || foo(-42) != 42
        || foo(42) != 42
        || baz(-10) != 10
        || baz(-10) != 10) {
            __builtin_abort();
        }

    return 0;
}
