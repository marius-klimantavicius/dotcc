//type:fp
//options_all:--c++23
//remark:Instantiation of C++23 lambda without a parameter declaration clause
// 3/5/26   [EDGcpfe/28024]
//
// Instantiation of C++23 lambda without a parameter declaration clause
//
// The front end failed to instantiate C++23 lambdas that are declared without a
// parameter declaration clause.
int i = []<int = 0> -> int { return 1; }();  // Previously an error.
                                             // Now okay.
