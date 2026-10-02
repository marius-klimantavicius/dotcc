//type: fp
//options:  --c++20 --modules
# 0 "./modules/pr99294_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/pr99294_b.C"



# 1 "./modules/pr99294.h" 1

template <typename T>
class basic_string;

typedef basic_string<char> string;

template <typename T>
class basic_string
{
 public:
  string Frob ();

  basic_string (int);
};
# 5 "./modules/pr99294_b.C" 2
import foo;

string Quux ()
{
  return 1;
}
