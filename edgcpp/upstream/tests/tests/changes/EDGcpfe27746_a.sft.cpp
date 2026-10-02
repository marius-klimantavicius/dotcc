//type:fp
//options_all:--gnu=140100 --no_strict_gnu
//remark:[6.7] GNU and clang compatibility: _Float16 and complex floating point types
// 12/3/24  [EDGcpfe/27746]
//
// GNU and clang compatibility: _Float16 and complex floating point types
//
// The front end previously had several issues with respect to the handling
// of the 16-bit floating point types and with complex floating point types:
//
// 1. In clang mode, the front end previously treated std::float16_t and
// _Float16 as distinct types, when the clang compiler interprets them as the
// same.
//
// 2. The front end incorrectly treated the GNU and clang extended floating
// point imaginary suffixes if16, if32, if64, if128, and ibf16 as user-defined
// literal suffixes.  This is now fixed in the appropriate emulation modes.
// (Note that g++ also treats these suffixes as user-defined literal suffixes
// unless the -std=gnu++XX or -fext-numeric-literals command-line options are
// specified; the front end emulates this via the --no_strict_gnu command-line
// option.)
//
// and with --clang_version=180100:
//
// 3. 16-bit complex floating point types were not correctly lowered and could
// result in assertion failures in, e.g., dump_initializer_part in C-generating
// back end configurations.
auto cf32 = 0.0if32;
