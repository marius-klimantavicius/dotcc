//type:fp
//options_all:--gn 100100
//remark:[6.5] GNU C++ compatibility: Defaulted exception-specification compatibility
// 12/9/22  [EDGcpfe/23759,EDGcpfe/25277,EDGcpfe/25650,EDGcpfe/25838]
//
// GNU C++ compatibility: Defaulted exception-specification compatibility
//
// The changes for EDGcpfe/20918 (which implement the C++ standardization
// committee's P1286R2) carved out some exceptions, including GNU C++ modes that
// do not enable C++20.  Now, all GNU C++ modes with gnu_version >= 100000
// support the changes of P1286R2.
struct X { X() noexcept(false); };
struct S {
  X x;
  S() noexcept(true) = default;
};
S s;  // Previously an error in all GNU C++11 modes because S::S() was
      // considered deleted.  Now okay when gnu_version >= 100000.
