//remark:Vector shift-assign
//options:--gnu=40700;fn:--gnu=40800;fp:--clang_v=30900;fn:--clang_v=40000;fp

typedef int __attribute((vector_size(16))) V4;
void g(V4 *p) {
  *p <<= 2;
}

