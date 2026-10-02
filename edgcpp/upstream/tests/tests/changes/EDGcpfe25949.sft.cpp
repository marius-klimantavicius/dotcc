//type:fp
//options_all:--c++17
//remark:[6.5] Regression in template template argument matching
// 2/22/23  [EDGcpfe/25949,EDGcpfe/26013]
//
// Regression in template template argument matching
//
// The changes for EDGcpfe/24912 in version 6.4 (see entry of 3/23/22) introduced
// a regression in some cases involving template template arguments.
//
// That is now fixed.
template<typename... Ts> struct X {};
template<template <typename U, typename V> class> struct S {};
S<X> sx;  // Previously an error claiming X is not a match for the
          // corresponding template template parameter.  Now okay.
