//type:fp
//remark:[4.10.1] Conversion function template to "reference to abstract class"
// 2/12/15   [EDGcpfe/16002]
//
// Conversion function template to "reference to abstract class"
//
// Previously, when considering user-defined implicit conversions, the front end
// accidentally discarded conversion function templates whose destination type is
// a reference to a complete abstract class.
//
// This is now fixed.
template<typename T> struct A { virtual ~A() = 0; };
template struct A<int>;  // Ensure A<int> is complete.
struct W {
  template<typename T> operator A<T>&();
} w;
int g(A<int>&);
int r = g(w);  // Previously an error because the conversion template was
               // incorrectly discarded.  Now okay.
