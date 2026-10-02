//type:fn
//options_all:--c++11 --g++
//remark:[4.9] GNU C++11 compatibility: Conversion from lambda to function pointer
// 10/23/13 [EDGcpfe/14585]
//
// GNU C++11 compatibility: Conversion from lambda to function pointer
//
// In standard C++11, a closure type includes a conversion function to a pointer
// to function if the associated lambda is introduced with "[]" (see the entry of
int i;
struct S { S(void (*pf)()); };
S s([&]{ i = 0; });  // Accepted when the 4.9 change was made (implicit
                     // conversion to function pointer in GNU C++11 mode).
                     // Later versions diagnose no matching constructor.
