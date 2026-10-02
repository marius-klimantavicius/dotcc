//type:fp
//options_all:--c++17 -tused -A
struct A {
         char : 4;
};
struct B:A {
         int i;
};

int main()
{
    static_assert(__is_standard_layout(A));
    static_assert(!__is_standard_layout(B));
}

//cwg: 1881
//title: Standard-layout classes and unnamed bit-fields (Resolved by 1813)
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
