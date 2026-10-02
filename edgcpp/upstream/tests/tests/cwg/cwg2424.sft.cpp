//type:fp
//options_all:--c++20 -A -tused
struct S
{
    union
    {
        int i;
    };
    constexpr S() {};
};

//cwg: 2424
//title: constexpr initialization requirements for variant members
//meeting: Belfast 11/19
//edg_status: EDGcpfe/22126
//fixed_in: 6.1
