//type:cp
//options_all:--c++11 --egrep_c_pattern 'FooTab\\s*\\+\\s*1'
//script:edg-egrep-c
//require:BACK_END_IS_C_GEN_BE 1

struct Foo
{
 int a;
 const Foo *ptr;
};

static const Foo FooTab[] =
{
 { 1, &FooTab[1] },
 { 2, &FooTab[0] },
};

const Foo * func()
{
 return FooTab;
}
