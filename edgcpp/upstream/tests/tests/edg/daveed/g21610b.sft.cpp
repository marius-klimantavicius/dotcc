//remark:No-effect warnings
//options:--gcc --diag_error=174;fp

  int main() {
    ({});  // Previously a warning.  Now no warning.
  }
