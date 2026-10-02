//type:fp
//options_all:--c++17 -tused -A
int main()
{
    volatile int i;
    int n = sizeof(i=5);
}

//cwg: 2427
//title: Deprecation of volatile operands and unevaluated contexts
//meeting: Belfast 11/19
//edg_status: Passes
