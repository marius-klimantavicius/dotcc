//type:fp
//options_all:--c++20
//remark:[5.1] Exception specifications in explicitly-defaulted functions
// 6/6/19   [EDGcpfe/20918]
//
// Exception specifications in explicitly-defaulted functions
//
// Committee paper P1286R2 changed how a mismatch between a routine's explicitly-
// specified exception specification and its generated default behaves.  The new
// behavior is to use the explicitly-specified exception specification but
// otherwise generate the routine as usual - even if this means an exception may
// be thrown in spite of a noexcept(true) specification (std::terminate will be
// called).  This change has been made to apply retroactively to older C++ modes
// (e.g., C++11), however in GCC, Clang or MSVC compatibility modes the original
// behavior has been preserved outside of C++20 mode.
struct T {
  T();
  T(T &&) noexcept(false);
};
struct U {
  T t;
  U();
  U(U &&) noexcept = default; // Previously implicitly deleted, now kept
};
U u1;
U u2 = static_cast<U&&>(u1); // Previously an error, now accepted
