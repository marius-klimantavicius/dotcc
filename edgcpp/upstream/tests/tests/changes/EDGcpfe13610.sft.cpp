//type:fp
//remark:[4.6] Internal error on use of Microsoft __w64-annotated type
// 2/1/13   [EDGcpfe/13610]
//
// Internal error on use of Microsoft __w64-annotated type
//
// In Microsoft-compatible configurations, the front end could abort with an
// internal error in ilp64_will_narrow (types.c) when converting a 64-bit type
// with a __w64 annotation to a 32-bit integral type.
//
// This is now fixed.
typedef int Int;
typedef __w64 long Long;

void g(Long x) {
  Int i = (Int)x;  // Previously triggered an internal error in some
}                  // 64-bit configurations.
