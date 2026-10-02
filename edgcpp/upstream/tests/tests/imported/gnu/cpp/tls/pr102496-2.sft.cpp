//type: fp
//options: --c++11
// PR c++/102496
// { dg-do compile { target c++11 } }
// { dg-require-effective-target tls }

__thread int t1;
__thread int t2;
