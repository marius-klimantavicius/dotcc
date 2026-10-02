//type:fn
//options_all:--c++11
//remark:[6.2] Core issue 903: Null pointer constants
// 12/9/20  [EDGcpfe/14164,EDGcpfe/19415,EDGcpfe/23273]
//
// Core issue 903: Null pointer constants
//
// The resolution of Core issue 903 (treated as a defect report against C++11)
// restricts null pointer constants to zero literals.  Other expressions that
// produce an integer constant of value zero are no longer treated as null pointer
// constants by the front end, unless matching specific behavior of other
// compilers (for example, permissive Microsoft mode still retains the prior
// behavior, and in GNU modes casting a zero literal to an integer type can still
// produce a null pointer constant).
//
// (See also the entry of 11/11/20 for EDGcpfe/23533,EDGcpfe/23541, which covers
// constant variables evaluating to a zero value.)
int *p = 4-4;  // Now an error in default C++11 mode.
