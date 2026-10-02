//options_all:--c++23 -tused -A
#include<initializer_list>
 static const char test1 = 'x';
  static const char (&r) [] = "x";
  static const char *s = "x";  
  static std::initializer_list<char> il = { 'x' };
  const bool b2 = r != il.begin();        // unspecified result
  const bool b3 = r != s;                 // unspecified result
  const bool b4 = il.begin() != &test1;   // always true
  const bool b5 = r != &test1;            // always true

//cwg: 2753
//title: Storage reuse for string literal objects and backing arrays
//meeting: Kona 11/23
//edg_status: Passes
