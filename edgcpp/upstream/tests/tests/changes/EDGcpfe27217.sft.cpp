//type:fp
//options_all:--gn 120000 --c++20
//remark:[6.7] Lambdas in template argument lists and the C++-generating back end
// 10/17/24 [EDGcpfe/27217]
//
// Lambdas in template argument lists and the C++-generating back end
//
// This previously triggered an internal error in the C++-generating back end (in
// bypass_prototyped_param_src_seq_entries) because no source sequence entries
// were recorded for lambdas in template argument lists.  That is now fixed.
template<int> class S {};
template <int N> void g() {
  S<[](int n){ return n; }(42)>();
}
