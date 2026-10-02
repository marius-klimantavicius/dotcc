//type:fp
//options_all:--gnu_version=80100
//remark:[6.3] Spurious error on reinterpret_cast
// 6/1/21   [EDGcpfe/20413,EDGcpfe/22068,EDGcpfe/22974,EDGcpfe/24354]
//
// Spurious error on reinterpret_cast
//
// In some cases, the front end issued a spurious error about casting away
// constness with a reinterpret_cast expression.
//
// That is now fixed.
char data[42];
auto r = reinterpret_cast<char const**>(&data);
           // Previously resulted in a (spurious) error.  Now okay.
