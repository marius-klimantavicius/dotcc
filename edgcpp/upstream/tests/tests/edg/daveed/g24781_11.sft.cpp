//remark:Explicit-this member functions
//options:--c++23;rp

auto lm = [i=10](this auto &self)->int {
            if (i > 1) {
              return --i*self();
            } else {
              return 1;
            }
          };


extern "C" int printf(char const*, ...);

int main() {
  printf("%d\n", lm());
}

