//remark:Null pointer constants
//options:--c++11;fp:--c++03;fn

  int const zero = 0;
  int f(char);
  int f(const char*);
  int r = f(zero);  // Previously always ambiguous because "zero" was
                    // considered a valid null pointer constant.  Now okay
                    // in C++11 (and later) modes.
