//type:cp
//options:--gcc:--g++
//options_all:--gnu_version 70400
//require:GCC_IS_GENERATED_CODE_TARGET 1

struct a {} b,d;
struct a foo()
{ return ( {b;d;}); }
