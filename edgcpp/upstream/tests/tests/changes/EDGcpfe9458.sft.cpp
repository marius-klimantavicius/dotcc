//type:fp
//remark:[4.1] Cast of non-class expression to reference-to-const non-class now allowed
// 1/23/09  [EDGcpfe/9458, EDGcpfe/4641]
//
// Cast of non-class expression to reference-to-const non-class now allowed
//
// A cast of an expression with non-class type to a reference to const of
// a different type is now accepted, if the source expression can be
// converted to the underlying type of the reference.
//
// The source expression is converted into a temporary of the underlying
// type of the cast, and an lvalue for the temporary is returned as the
// result.
int main() {
  static_cast<const long &>(123);  // Now accepted
}
