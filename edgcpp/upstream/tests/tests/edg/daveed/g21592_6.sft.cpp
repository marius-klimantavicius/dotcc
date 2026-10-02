//remark:constinit and destructors
//options:--c++20;fp

int counter;
struct T {
   constexpr T(int e) : fld(e) {}
   int fld;
   ~T() { counter++; } // non-trivial
};
constinit T x(42); // Initialization OK. Doesn't check destructor.
constinit T y{42};
