//remark: [[indeterminate]]
//options:--c++26;fn

int f(int);

int f([[indeterminate]] int p) {
  [[indeterminate]] int x;
  return p+x;
}
[[indeterminate]] int e;
