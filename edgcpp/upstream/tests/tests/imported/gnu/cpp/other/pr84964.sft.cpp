//type: s
//options:
//remark: sometimes triggering memory region allocation must not occur after front end processing has ended
/* { dg-do compile } */

struct a {
  short b : -1ULL;
};
void c(...) { c(a()); }
// { dg-excess-errors "" }
