//type: fp
//options: --c++11
# 0 "./abi/abi-tag25a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./abi/abi-tag25a.C"




# 1 "./abi/abi-tag25.C" 1



template<class T>
[[gnu::abi_tag("foo")]] void fun() { }

template void fun<int>();
# 6 "./abi/abi-tag25a.C" 2
