//remark: [[indeterminate]]
//options:--c++26;fp

template<typename T>
int f([[indeterminate]] int p) {
  [[indeterminate]] int x;
  return p+x;
}
