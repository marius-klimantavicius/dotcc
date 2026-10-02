//type: fp
//options:  --c++20 --modules
# 0 "./modules/tdef-inst-1_a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/tdef-inst-1_a.C"






module;

# 1 "./modules/tdef-inst-1.h" 1

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
# 10 "./modules/tdef-inst-1_a.C" 2

export module foo;


export inline int greeter (string const &bob)
{
  return sizeof (bob);
}
