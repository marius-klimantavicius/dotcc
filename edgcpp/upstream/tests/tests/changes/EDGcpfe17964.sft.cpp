//type:fp
//options_all:--c++17
//remark:[4.14] C++17 compatibility: __has_unique_object_representations type traits helper
// 4/19/17  [EDGcpfe/17964,EDGcpfe/17999]
//
// C++17 compatibility: __has_unique_object_representations type traits helper
//
// The front end now recognizes the __has_unique_object_representations type
// traits helper, which supports the std::has_unique_object_representations
// trait (described in Committee document P0258R2).  It returns TRUE if every
// bit of the object representation is part of the value and every bit pattern
// represents a different value.  The current implementation assumes that the
// result is true for integers, void, pointers, and pointers to members and
// false for other fundamental data types.  For architectures and/or ABIs
// where that assumption is not correct, the logic of the function
// type_has_unique_object_representations (folding.c) will need to be adjusted
// appropriately.
struct A {
  char c;
  int  i;
};
bool b = __has_unique_object_representations(A);  // false: padding bits
                                                  // are not part of value
