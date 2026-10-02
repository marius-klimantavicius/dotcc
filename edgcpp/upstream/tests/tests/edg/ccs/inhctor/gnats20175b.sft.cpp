//type:rp
//options:--c++17

extern "C" int printf(const char *, ...);

struct V { V(int) { printf("Calling V::V(int)\n"); } };
struct W : virtual V { using V::V; };
struct X : virtual W, virtual V {
  using W::W;
  using V::V;
};
X x(0); // Both V::V(int) inherited virtually - not ambiguous

int main() {}
