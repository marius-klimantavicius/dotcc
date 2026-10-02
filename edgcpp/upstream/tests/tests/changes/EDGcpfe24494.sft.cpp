//type:fp
//options_all:--gn 100200
//remark:[6.4] GNU C++ compatibility: Initializers for flexible array members
// 1/18/22  [EDGcpfe/24494]
//
// GNU C++ compatibility: Initializers for flexible array members
//
// In GNU C++ mode with gnu_version >= 60000, the front end now accepts aggregate
// initializers for flexible array members with trivial destruction.
//
// (The front end already accepted such cases in Microsoft modes: See the entry
// of 9/1/04.)
struct X { int i; };
struct Y {
  int i;
  X x[];
};
Y y = {0, {0}};
