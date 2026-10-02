//options_all:-r -x -tused
//options: --strict;cn:;cn

//    V1        V2
//    | \      / |
//    A  B    C  D
//     \_ \  / _/
//       \ || / 
//         S
struct V1 {
  int i, j, k;
};
struct V2 {
  int j, k, l;
};
struct A : public V1 {
  int i, j;       // override V1::i, V1::j
};
struct B : public V1 {
  int l;
};
struct C : {
  int 
