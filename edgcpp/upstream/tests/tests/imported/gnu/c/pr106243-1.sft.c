//type: rp
//options: 
# 0 "./pr106243-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr106243-1.c"




# 1 "./pr106243.c" 1







__attribute__((noipa)) int foo (int x) {
    return -x & 1;
}


__attribute__((noipa)) int bar (int x) {
    return (0 - x) & 1;
}


__attribute__((noipa)) int baz (int x) {
    x = -x;
    return x & 1;
}


__attribute__((noipa)) int qux (int x) {
    return 1 & -x;
}


__attribute__((noipa)) __attribute__((vector_size(4*sizeof(int)))) int waldo (__attribute__((vector_size(4*sizeof(int)))) int x) {
    return -x & 1;
}


__attribute__((noipa)) int thud (int x) {
    return -x & 2;
}


__attribute__((noipa)) int corge (int x) {
    return -x & -1;
}
# 6 "./pr106243-1.c" 2

int main () {

    if (foo(3) != 1
        || bar(-6) != 0
        || baz(17) != 1
        || qux(-128) != 0
        || foo(127) != 1) {
            __builtin_abort();
        }

    return 0;
}
