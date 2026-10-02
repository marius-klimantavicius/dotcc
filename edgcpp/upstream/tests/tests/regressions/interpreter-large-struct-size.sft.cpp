//type:fp
//options_all: --c++11

// This test exercises the handling of large struct sizes resulting
// from opportunistic constant folding in the frontend. Under certain
// scenarios, EDG will attempt to constant fold an expression even if
// not strictly required for conformance. In this case, interpreter
// may fail if the struct is too large but the failure does not yield
// a diagnostic. Later on, a subsequent call to fold this expression
// may trigger a cache lookup to get the results of of the previous
// attempt. Previously, this cache entry did not distinguish between a
// failure due to an invalid input expression and a failure due to a
// the struct size limit.  As a result, the cache lookup would
// incorrectly report a failure due to an invalid input expression
// causing the interpreter to yield an error node despite the program
// being valid. This error node would trigger an internal assertion
// during il_lowering.

struct arr {
  // The struct size limit in EDG is currently 2^30.
  unsigned char x[(unsigned long long)1 << 31];
};

int sink(arr) { return 0; };

int main() {
  arr x{};
  int y = sink(x);
  int z = sink(x);
}
