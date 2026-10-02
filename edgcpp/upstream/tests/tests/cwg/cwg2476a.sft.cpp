//options_all:--c++23 -A
    int f();
    auto (*fp1)() = f;       // OK
    auto (*fp2)()->int = f;  // OK
    auto (*fp3)()->auto = f; // OK

//cwg: 2476
//title: placeholder-type-specifiers and function declarators
//meeting: Tokyo 3/24
//edg_status: EDGcpfe/27119
