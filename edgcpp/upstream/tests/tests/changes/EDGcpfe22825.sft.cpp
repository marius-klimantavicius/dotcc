//type:fp
//options_all:--c++14 --gnu_v 70300 -w -tused
//remark:[6.1] Partial specialization ordering failure with cast in nontype argument
// 6/5/20   [EDGcpfe/22825]
//
// Partial specialization ordering failure with cast in nontype argument
//
// In some complex cases, the front end sometimes spuriously failed to order
// partial specializations that involved nontype template argument expressions
// containing a cast.
//
// Previously, this example triggered an error claiming that more than one partial
// specialization of Assign matches its use, because the cast to "bool" was
// handled slightly differently in the two partial specializations.  This is now
// fixed.
template<int V> struct IntVal { static constexpr int val = V; };
template<bool> struct BoolVal;
template<typename, typename, typename> struct Assign;
template<typename T, typename U>
  struct Assign<T, U, BoolVal<bool(1+(unsigned)T::val)>>;
template<typename T>
  struct Assign<T, T, BoolVal<bool(1+(unsigned)T::val)>> {};
Assign<IntVal<0>, IntVal<0>, BoolVal<1>> ai;  // Previously an error.
                                              // Now okay.
