//type:fp
//options_all:--c++14 --clang_version=100000
//remark:[6.8] Clang C++ compatibility: Vector conversions
// 3/7/25   [EDGcpfe/27886]
//
// Clang C++ compatibility: Vector conversions
//
// Clang permits implicit conversions between equal-sized vector types (such as
// VD and VL above).  The front end previously erroneously always treated such
// conversions as identity conversions when considered as a step in user-defined
// conversions.  That caused the example above to be ambiguous, but candidate (1)
// should be preferred over (2) since the standard conversion needed for (1) is
// truly an identity and that for candidate (2) is not.  This is now fixed.
using VD = double __attribute((vector_size(16)));
using VL = long __attribute((vector_size(16)));
int g(VD);  // (1)
int g(VL);  // (2)
struct X { operator VD(); };
int r = g(X{});  // Previously ambiguous.  Now selects (1).
