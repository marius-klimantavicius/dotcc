//type:fp
//options_all:--microsoft
//remark:[4.8] Microsoft compatibility: Static data members in unions
// 5/23/13  [EDGcpfe/14082]
//
// Microsoft compatibility: Static data members in unions
//
// In Microsoft mode, the front end now accepts static data member declarations
// in unions.
//
// This is standard in C++11, but not in C++03 (it is now, however, accepted in
// all Microsoft C++ modes; not just Microsoft C++11 mode).
union U {
  static int count;  // Previously an error in Microsoft mode; now okay.
};
