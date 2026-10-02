//type:fp
//remark:[4.12] GNU/clang C++ compatibility: imaginary vs user-defined literals
// 8/4/16   [EDGcpfe/17283]
//
// GNU/clang C++ compatibility: imaginary vs user-defined literals
//
// In g++ and clang modes in which user-defined literals are enabled, numeric
// literals with suffixes "i", "il", and "if" were previously treated as
// imaginary literals.  They are now parsed as user-defined literals, matching
// the behavior of g++ and clang.
void * operator "" il(long double ld) { return nullptr; }
void *p = 12.34il;   // Previously treated as imaginary literal, eliciting
                     // an error; now a user-defined literal with no error
