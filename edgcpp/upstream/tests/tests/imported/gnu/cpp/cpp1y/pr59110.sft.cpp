//type: fn
//options: --c++14
// PR c++/59110
// { dg-do compile { target c++14 } }

int i = *(auto*)0; // { dg-error "" }
