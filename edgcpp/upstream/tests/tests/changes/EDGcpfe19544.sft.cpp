//type:fp
//options_all:--c++14
//remark:[5.0] New-expressions and decltype(auto)
// 4/12/18  [EDGcpfe/19544]
//
// New-expressions and decltype(auto)
//
// In C++14 mode, the front end now accepts new-expressions where the type is
// specified as decltype(auto).
int *p = new decltype(auto)(42); // Now accepted.
