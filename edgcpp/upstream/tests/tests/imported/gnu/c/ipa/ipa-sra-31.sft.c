//type: fp
//options: 
# 0 "./ipa/ipa-sra-31.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ipa/ipa-sra-31.c"


# 1 "./ipa/ipa-sra-30.c" 1


struct list
{
  struct list *next;
  int val;
};
__attribute__ ((noinline))
static int reta (int *a)
{
 return *a;
}
__attribute__ ((noinline))
static int
kill(struct list *l, int *a)
{
 int v;
 while (l)
 {
  v = l->val;
  l=l->next;
 }
 return reta (a) + v;
}
int
test(struct list *l, int *a)
{
 return kill (l, a);
}
# 4 "./ipa/ipa-sra-31.c" 2
