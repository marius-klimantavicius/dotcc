//type:fp
//options_all:--g++
//remark:[4.4] GNU compatibility, C++-generating back end: spelling of "decltype"
// 4/25/11  [EDGcpfe/11611]
//
// GNU compatibility, C++-generating back end: spelling of "decltype"
//
// In --g++ mode, the front end accepts decltype and __decltype as equivalent
// keywords.  The C++-generating back end always generated a decltype
// construct using the "decltype" spelling; however, current versions of g++
// only accept that spelling of the keyword with -std=c++0x.  Consequently,
// such constructs are now generated using the "__decltype" spelling when
// gcc_is_generated_code_target is TRUE.
__decltype(0) i;   // Previously generated as "decltype(0) i;"
