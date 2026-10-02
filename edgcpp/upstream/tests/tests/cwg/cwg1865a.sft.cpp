//type:fp
//options_all:--c++14 -tused -A

int main() {
         int a = 1;
         int* b[2] = {&a, &a};
         int const * const * c = b;
         int d = (c - b);
}

//cwg: 1865
//title: Pointer arithmetic and multi-level qualification conversions
//meeting: Urbana-Champaign 11/14
//edg_status: EDGcpfe/17572
