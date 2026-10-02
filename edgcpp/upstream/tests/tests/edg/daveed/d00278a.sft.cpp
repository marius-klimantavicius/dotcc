//remark:GNU explicit alignment specification
//type:rp
//name:
//options:
//options_all:--gcc
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

typedef int int_a64 __attribute__ ((aligned(64)));
typedef const int cint;
typedef cint cint_a64 __attribute__ ((aligned(64)));
typedef const int_a64 int_a64c;

int_a64 a;
cint_a64 b;
int_a64c c;

int main () {
    /* should all return 64 */
    int a1=__alignof__ (int_a64); // 64
    int a2=__alignof__ a;         // 4
    int a3=__alignof__ (cint_a64);// 64
    int a4=__alignof__ b;         // 64
    int a5=__alignof__ (int_a64c);// 4 
    int a6=__alignof__ c;         // 4

    return !(a1 == 64 && a2 == 64 && a3 == 64 && a4 == 64 && a5 == 64 && a6 == 64);
}
