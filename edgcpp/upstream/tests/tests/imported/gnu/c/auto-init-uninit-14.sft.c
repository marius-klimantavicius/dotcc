//type: fp
//options: 
# 0 "./auto-init-uninit-14.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./auto-init-uninit-14.c"



# 1 "./uninit-14.c" 1




struct p {
        short x, y;
};

struct s {
        int i;
        struct p p;
};

struct s f()
{
        struct s s;
        s.p = (struct p){};
        s.i = (s.p.x || s.p.y);
        return s;
}
# 5 "./auto-init-uninit-14.c" 2
