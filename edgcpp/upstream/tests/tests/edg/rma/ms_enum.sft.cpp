//options_all:-r -x -tused
//options: --microsoft -n;cp

enum E x;
int i = sizeof(x);
enum E f(enum E e) { return e; }
enum E { a,b,c,d,e };
int j = e;

