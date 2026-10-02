//type:fp
//options_all:--gnu_version 80100 --c++17 -w -tused
//remark:[6.8] Core issue 2356: Inheriting constructors in copy/move-like operations
// 4/4/25   [EDGcpfe/21675,EDGcpfe/21867,EDGcpfe/27856]
//
// Core issue 2356: Inheriting constructors in copy/move-like operations
//
// The front end now implements the resolution of core issue 2356, which causes
// inheriting constructors to be ignored for copy/move-like operations.  This
// impacts fairly subtle overload situations.
struct B {
  B(B&&);
  template<typename T> B(T&&);
};
struct D: B {
  using B::B;
  D(D const&);
  D(D&&) = default;
  struct X { X(X&&) = delete; } x;
};
D&& g();
D d(g());  // Previously an error because the D::x subobject cannot be moved.
           // Now, the move constructors are considered non-viable and the
           // copy constructor of D is selected instead.
