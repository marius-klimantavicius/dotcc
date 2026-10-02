//type:fp
//remark:[5.1] Relaxed range-based-for loop customization point finding rules
// 6/4/19   [EDGcpfe/20020]
//
// Relaxed range-based-for loop customization point finding rules
//
// The C++ standardization committee paper P0962R1 relaxes the range-based-for
// loop customization point finding rules such that if a class contains only one
// member "begin" or "end", this is no longer an error (and the found function is
// not considered a candidate for the range-based-for loop begin/end calls).  This
// allows "begin" and "end" functions in associated namespaces to operate on
// classes that declared a "begin" or "end" function (but not both).  This change
// is retroactive to all modes that support range-based-for loops.
struct A { void end(); };
int* begin(A&);
int* end(A&);
void f() {
  for (auto a : A()) {} // Previously would issue an error that A::begin
                        // doesn't exist. Now uses ::begin and ::end
}
