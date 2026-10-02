//type: fn
//options: --c++20
// PR c++/111173
// { dg-do compile { target c++20 } }

using Function = int();
constinit Function f; // { dg-error ".constinit. specifier invalid for function" }
