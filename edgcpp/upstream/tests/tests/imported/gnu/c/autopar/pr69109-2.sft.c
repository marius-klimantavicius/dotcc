//type: fp
//options: 
# 0 "./autopar/pr69109-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./autopar/pr69109-2.c"



# 1 "./autopar/../../gcc.c-torture/compile/pr32399.c" 1


void f(unsigned char *src, unsigned char *dst, int num, unsigned char *pos, unsigned char *diffuse, int hasdiffuse, unsigned char *specular, int hasspecular) {
    int i;

    for(i=num;i--;) {
 float *p = (float *) ((long unsigned int) dst + (long unsigned int) pos);
        if(hasdiffuse) {
            unsigned int *dstColor = (unsigned int *) (dst + i + (long unsigned int) diffuse);
            *dstColor = * (unsigned int *) ( ((long unsigned int) src + (long unsigned int) diffuse) + i);
        }
        if(hasspecular) {
            unsigned int *dstColor = (unsigned int *) (dst + i + (long unsigned int) specular);
            *dstColor = * (unsigned int *) ( ((long unsigned int) src + (long unsigned int) specular) + i);
        }
    }
}
# 5 "./autopar/pr69109-2.c" 2
