//type:fp
//options_all:--gnu_version 80000
//remark:[6.1] Spurious ambiguity error on partial specialization selection
// 4/2/20   [EDGcpfe/20832,EDGcpfe/22458]
//
// Spurious ambiguity error on partial specialization selection
//
// In some cases where a nontype template argument is implicitly converted to the
// corresponding parameter's type, the front end failed to select among partial
// specializations that are not in fact ambiguous.
//
// This is now fixed.
enum E { e, f };
template<typename, int N, int = (N == 1 ? f : e)> struct X {};
                    // Default argument implicitly converted from E to int.
template <typename, typename> struct S {};
template <typename T, typename U, int R>
   struct S <X<T, R>, X<U, R>> {};  // (1)
template <typename T, int R> struct S<X<T, R>, X<T, R>> {};  // (2)
using XD = X<double, -1, 0>;
S<XD, XD> sxd;  // Previously ambiguous.  Now correctly selects (2).
