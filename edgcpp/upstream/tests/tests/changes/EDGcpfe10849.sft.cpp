//type:fp
//options_all:--c --gcc
//remark:[4.2] Abort on GNU C transparent union with large unnamed bit field
// 7/22/10  [EDGcpfe/10849]
//
// Abort on GNU C transparent union with large unnamed bit field
//
// In GNU C mode, the front end previously aborted with an internal error in
// check_if_fill_in_used ("provided diagnostic fill-in was not used") if a union
// with the transparent_union attribute contains an unnamed bit field whose size
// exceeds the first field of the union (which is invalid for a transparent
// union).
//
// This is now fixed (a warning is emitted for the invalid attribute).
union U {
  char c;
  long long : 48;  // Previously triggered an internal error.
} __attribute((transparent_union));
