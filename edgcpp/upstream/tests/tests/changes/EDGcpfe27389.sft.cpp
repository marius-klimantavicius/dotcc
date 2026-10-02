//type:fp
//options_all:--c++20
//remark:[6.7] Incorrect handling of concept in unnamed namespace
// 7/19/24  [EDGcpfe/27389]
//
// Incorrect handling of concept in unnamed namespace
//
// The front end previously did not recognize the angle bracket following N::c
// (while caching the initializer for S::b), instead treating it like a "less
// than" operator.  That in turn resulted in misleading spurious errors.  That
// problem is now fixed.
namespace N {
  namespace {
    template <class, class> concept c = true;
  }
  class S {
    template<typename T> static constexpr bool b = N::c<T, T>;
  };
}
