//type:fp
//options_all:--c++11 --microsoft
//remark:[4.5] Defaulted member functions in Microsoft C++11 mode
// 9/27/12  [EDGcpfe/13215]
//
// Defaulted member functions in Microsoft C++11 mode
//
// When Microsoft mode with microsoft_version >= 1400 and C++11 mode were
// combined, the front end did not accept explicitly defaulted member functions
// because "default" is not a keyword in such modes.  This is now fixed.
//
// Note that "default" is still not a keyword in the combined mode.
struct S {
  S() = default;  // Previously not accepted with e.g. options
};                // "--microsoft_version=1600 --c++11".
