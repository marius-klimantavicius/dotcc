//type: fp
//options:  --c++20 --modules
# 0 "./modules/tpl-friend-merge-1_d.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/tpl-friend-merge-1_d.C"


import "tpl-friend-merge-1_a.H";
import "tpl-friend-merge-1_b.H";
import "tpl-friend-merge-1_c.H";

# 1 "./modules/tpl-friend-merge-1.cc" 1
void __istream_extract (int)
{
  (void)basic_streambuf<char>::field;
  (void)basic_streambuf<int>::field;
  (void)basic_streambuf<long>::field;
}
# 8 "./modules/tpl-friend-merge-1_d.C" 2
