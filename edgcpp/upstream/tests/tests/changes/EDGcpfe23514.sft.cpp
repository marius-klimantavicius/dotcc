//type:fp
//options_all:--microsoft --c++20
//remark:[6.2] Abort on use of disambiguated operator== template
// 11/3/20  [EDGcpfe/23514]
//
// Abort on use of disambiguated operator== template
//
// The changes for EDGcpfe/23009 (in version 6.1) introduced a disambiguation
// measure for operator== members that are normally ambiguous because of a missing
// const qualifier.  Unfortunately, that measure did not correctly handle
// operator== member templates, and attempting to use such a template could lead
// to an abort (in different places depending on the configuration).
//
// That issue is now fixed.
template<typename T> struct S {
  template<typename U> bool operator==(S<U> const&) /*not const*/;
};                               
bool f() {
  S<int> s;
  return s == s;  // Previously triggered an abort.  Now okay.
}
