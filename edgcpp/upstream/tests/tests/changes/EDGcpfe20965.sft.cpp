//type:fp
//options_all:--c++17
//remark:[5.1] Reference to function parameters and noexcept specifiers
// 4/15/19  [EDGcpfe/20965]
//
// Reference to function parameters and noexcept specifiers
//
// The front end previously did not consider candidate (1) viable for the call
// g(f) and as a result it selected the deleted candidate (2) and issued an error.
// Now (1) is selected instead and the example is accepted.  (Note that in non-
// overloaded contexts -- e.g., in the absence of candidate (2) -- the front end
// already accepted such calls.)
void f() noexcept(true) {}
void g(void(&func)(void));  // (1)
void g(...) = delete;       // (2)
int main() {
  g(f);  // Previously an error.  Now okay.
}
