int scoped_assume(int x) {
  [[assume(x > 0)]];
  return x + 1;
}
