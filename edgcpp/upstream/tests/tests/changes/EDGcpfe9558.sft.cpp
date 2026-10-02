//type:fp
//options_all:--c --gcc
//remark:[4.1] GNU C compatibility: Field conflicts with anonymous unions
// 2/25/09  [EDGcpfe/9558]
//
// GNU C compatibility: Field conflicts with anonymous unions
//
// In GNU C mode, more cases of fields from anonymous unions (a GNU extension in
// C mode) conflicting with a field of the same name in an enclosing class are
// now accepted.  (See also the Changes entries of 1/12/05 and 2/27/03).
struct S {
  int c;
  union {
    int u;
    float c;  // Conflicts with field c of type int.  Now accepted in
  };          // GNU C mode.
};
