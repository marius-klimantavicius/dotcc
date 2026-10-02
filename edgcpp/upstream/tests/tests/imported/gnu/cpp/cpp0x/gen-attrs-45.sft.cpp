//type: fp
//options: --c++11
// PR c++/52906
// { dg-do compile { target c++11 } }

[[gnu::deprecated]]; // { dg-warning "attribute ignored" }
