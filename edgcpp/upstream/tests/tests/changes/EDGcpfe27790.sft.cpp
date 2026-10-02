//type:fp
//options_all:--gcc --gn 80300
//remark:[6.7] GNU/Clang C compatibility: Address constant initializers
// 12/18/24 [EDGcpfe/27790]
//
// GNU/Clang C compatibility: Address constant initializers
//
// Previously this was an error because "= { pvar }" is not a constant
// initializer.  However, GCC and Clang accept this in their C modes.  The front
// end now emulates that behavior.
unsigned long var;
const unsigned long pvar = (unsigned long)(&var);
const unsigned long arr[] = { pvar };
