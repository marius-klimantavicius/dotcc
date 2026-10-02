//type:fp
//options_all:--c --gcc
//remark:[4.2] Infinite loop on designator into nonstandard anonymous union (GNU C mode)
// 2/16/10  [EDGcpfe/10349]
//
// Infinite loop on designator into nonstandard anonymous union (GNU C mode)
//
// In GNU C mode, a designated initializer for a member of a nonstandard
// anonymous union (or struct) could previously result in an infinite loop.
//
// This is now fixed.
struct X {
  union {
    struct {
      int i;
    };
  };
} x = {{{ .i = 1 }}};  // Previously caused the front end to "hang" in
                       // GNU C mode.
