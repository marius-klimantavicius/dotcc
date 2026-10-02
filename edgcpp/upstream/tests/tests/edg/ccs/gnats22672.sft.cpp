//type:cp
//options::--c++11:--gnu_version 70300 --c++14
//options_all:-tused

void foo ();
template <typename Compare, Compare = foo>
class A {};

A<void (*)()> a;
