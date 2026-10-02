//type: fp
//options: 
# 0 "./auto-init-uninit-20.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./auto-init-uninit-20.c"



# 1 "./uninit-20.c" 1



struct os { struct o *o; };
struct o { struct o *next; struct os *se; };
void f(struct o *o){
  struct os *s;
  if(o) s = o->se;
  while(o && s == o->se){
    s++;
    s == o->se
      ? (o = o->next, o ? s = o->se : 0)
      : 0;
  }
}
# 5 "./auto-init-uninit-20.c" 2
