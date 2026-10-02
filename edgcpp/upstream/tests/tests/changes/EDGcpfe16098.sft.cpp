//type:fp
//options_all:--microsoft
//remark:[4.10.1] Microsoft compatibility: __declspec attributes on lambdas
// 3/19/15  [EDGcpfe/16098]
//
// Microsoft compatibility: __declspec attributes on lambdas
//
// In Microsoft C++ modes that accept lambdas, the front end now accepts
// __declspec attributes on such lambdas.
//
// Such attributes apply to the call operator of the associated closure type.
auto fn = [](int x) __declspec(dllexport) { return x; };
