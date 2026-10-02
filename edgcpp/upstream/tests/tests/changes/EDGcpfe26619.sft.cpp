//type:fp
//options_all:--c++14 --gnu=50500
//remark:[6.8] GNU C++ compatibility: removal of GNU min/max operators
// 5/23/25  [EDGcpfe/26619,EDGcpfe/27100,EDGcpfe/27117,EDGcpfe/28102]
//
// GNU C++ compatibility: removal of GNU min/max operators
//
// Previously, the front end recognized the binary operators "<?" (min) and ">?"
// (max) in GNU C++ mode.  However, g++ removed support for these operators in
// version 4.3.
template<int> bool b = false;
template<int I> int v = b<I>?1:2;  // Previously a spurious error.  Now okay.
