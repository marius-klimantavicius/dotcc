//type:fp
//options_all:--c --gcc
//remark:[4.3] GNU C compatibility: Redefining an integer typedef from a system header
// 12/8/10  [EDGcpfe/11038]
//
// GNU C compatibility: Redefining an integer typedef from a system header
//
// In GNU C mode, a typedef for an integer type declared in a system header can
// now be redeclared to an enum type with the same underlying type (a warning is
// issued in such cases).
# 1 "/a/system/file.h" 3
typedef unsigned X;

# 1 "/any/file.c" 
typedef enum { e } X;  // Now accepted in GNU C mode (with a warning)
