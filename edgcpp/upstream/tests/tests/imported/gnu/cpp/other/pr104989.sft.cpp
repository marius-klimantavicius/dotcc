//type: s
//options:
//remark: sometimes triggering memory region allocation must not occur after front end processing has ended
// PR rtl-optimization/104989
// { dg-do compile }
// { dg-options "-fnon-call-exceptions" }

struct a {
  short b : -1ULL;
};imported/gnu/cpp/other/pr84964.sft.cpp
void c(...) { c(a()); }
// { dg-excess-errors "" }
