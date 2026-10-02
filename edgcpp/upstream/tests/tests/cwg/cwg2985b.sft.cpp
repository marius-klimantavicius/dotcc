//type:fp
//options:--c++23 -A
struct X { X() = default; X(X&&) = delete; };
using CX = const X;
X x = CX();    // OK, default-initializes x
