//type: fp
//options: --c++11
// PR c++/45401
// { dg-do compile { target c++11 } }

typedef int &__restrict restrictLvref;
typedef restrictLvref &&rvrefToRestrictLvref;
typedef restrictLvref rvrefToRestrictLvref;
