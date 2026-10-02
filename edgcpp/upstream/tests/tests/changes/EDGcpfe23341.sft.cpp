//type:fp
//options_all:--c++14 --gnu_version=70500
//remark:[6.2] Pack expansions in member-initializers (IL CHANGE)
// 11/9/20  [EDGcpfe/23341]
//
// Pack expansions in member-initializers (IL CHANGE)
//
// When a pack expansion occurs in the member-initializer portion of a constructor
// definition, the front end would previously issue spurious errors if the pack
// expansion would initialize members in an order different from what is required.
//
// In the above example, the pack expansion for 'X' amounts to:
//
// Note that 'A' is also a virtual base class of 'X<A>'.  Because 'A' is a common
// virtual base, it must be initialized first, despite appearing second in the
// pack expansion.  The front end now correctly handles such initialization/
// pack-expansion order differences.  As part of this change, the front end no
// longer caches member-initializers, resulting in a small IL CHANGE: the
// a_constructor_init field source.expr, a union member, is now a direct non-union
// member named source_expr.
struct A {};
template<class... Ts> struct X : public virtual Ts... {
  X(const Ts &... ts) : Ts(ts)... {}
};
X<X<A>, A> y{{{}}, {}};
