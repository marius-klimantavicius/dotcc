//type:fn
//options_all:--g++ --c++11
//remark:[4.4] Missing diagnostic on invalid C++0x attributes in GNU C++0x mode
// 7/26/11  [EDGcpfe/11943]
//
// Missing diagnostic on invalid C++0x attributes in GNU C++0x mode
//
// In some GNU C++0x modes (--g++ --c++0x) the front end accepts GNU attributes
// after a parenthesized initializer.  It accidentally also silently accepted
// (and ignored) C++0x-style attributes that followed the GNU attributes in these
// modes.
//
// This is now fixed (an error is issued).
int i(3) __attribute((used)) [[]];  // Previously accepted in GNU C++0x
                                    // mode; now an error.
