//options_all:-r -x -tused
//options: --strict;cn:;rp

void operator delete(void*);
void operator delete(void*, int);
main() {
  int* p = new int(0);
  delete p;
}

