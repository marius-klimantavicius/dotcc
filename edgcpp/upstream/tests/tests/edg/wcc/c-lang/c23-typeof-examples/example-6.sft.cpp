//type:fp
//options:--c23

int main () {
  typeof(typeof(const char*)[4]) y = {
    "a",
    "b",
    "c",
    "d"
  }; // 4-element array of "pointer to const char"
  return 0;
}
