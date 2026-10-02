//options_all:-r -x -tused
//options: --strict;cn

// Comments below represent current behavior, not necessarily how it should
// be.  Note especially:
//   Array element should probably be initialized exactly the way a variable
//     is -- i.e., a ctor should be called in both cases or in neither
//   Cfront issues an error on initialization of bb -- claims ambiguity betw
//     calling B(long) and B(const B&).
//
struct A { long x; A(long); };     // non-aggregate; bitwise copy okay
A a = {1L};                        // error
A a2 = 2L;                         // A(long) is called
A aa[] = { {1L}, {2L} };           // no ctor is called
A aa2[] = { 1L, 2L };              // A(long) is called

struct B { long x; B(long); B(const B&); };  // non-aggregate, cctor
B b = {1L};                                  // error
B b2 = 2L;                                   // B(long) is called
B bb[] = { {1L}, {2L} };                     // no ctor is called
B bb2[] = { 1L, 2L };                        // B(long) is called


