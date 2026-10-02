//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cp:;ln

typedef __EDG_SIZE_TYPE__ size_t;
char buffer[10];
char *pc = (char *)&buffer;
struct X {
        void *operator new(size_t sz);
        template <class T> void *operator new(size_t sz, T);
        void operator delete(void *);
        template <class T> void operator delete(void *, T);
        X(int);
        static void f() {
//          X *px = new X(0);
          X *px2 = new (pc) X(0);
        }
};
int main() {
  X::f();
}

