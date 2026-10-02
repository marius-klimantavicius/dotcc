//type:fp
//remark:[4.3] const/volatile on function types in template arguments
// 3/14/11  [EDGcpfe/11494]
//
// const/volatile on function types in template arguments
//
// The front end now accepts const- and/or volatile function types in template
// arguments.
//
// This agrees with the pending resolution of core issue 547.
template<typename T> struct X {};
X<void()const> x;  // Now accepted
