//type:fp
//options_all:--c++11
//remark:[4.6] Lowering of array variable in nested lambdas
// 1/28/13  [EDGcpfe/13603]
//
// Lowering of array variable in nested lambdas
//
// In cases where an array variable is captured by reference in a lambda, then
// subsequently captured by a nested lambda, lowering had failed an assertion
// check (in implied_copy_of_source).
void f(const int*) {}
int main() {
  int A[2] = {0, 1};
  [&](){
     [=]() { f(A); }();
  }();
}
