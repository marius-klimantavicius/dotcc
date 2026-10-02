//type:fp
//options_all:--gnu=60100
//remark:[4.12] GNU compatibility: assertion failures in walk_parents with abi_tag attribute
// 9/19/16  [EDGcpfe/17490,EDGcpfe/17541]
//
// GNU compatibility: assertion failures in walk_parents with abi_tag attribute
//
// A couple of issues that ended up failing an assertion check in walk_parents
// as a result of using the abi_tag attribute have been fixed.
// (with --gnu_version 60100):
//
// or:
inline namespace N __attribute__((abi_tag)) {}
template <class T> T x;
