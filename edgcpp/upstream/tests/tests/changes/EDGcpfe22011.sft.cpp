//type:fp
//options_all:--c++20
//remark:[6.1] C++20: Allow defaulting comparisons by value
// 12/18/19 [EDGcpfe/22011,EDGcpfe/22134]
//
// C++20: Allow defaulting comparisons by value
//
// The initial specification of defaulted comparison operators required that
// each parameter be a reference to the const-qualified type of the class
// being compared.  C++ Committee document P1946R0 extended that rule to allow
// defaulted friend comparison functions to take both arguments either by
// value or by const reference.  The front end has now been modified to
// support this extension.
struct C {
  int i;
  friend bool operator==(C, C) = default;  // Previously an error, now okay
};
