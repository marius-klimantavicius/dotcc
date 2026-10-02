//type: fp
//options: 
# 0 "./auto-init-uninit-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./auto-init-uninit-1.c"




# 1 "./uninit-1.c" 1





struct list
{
  struct list *next;
  int id;
};

extern void free (void *);

void remove_dupes (struct list *el)
{
  struct list *p, *q, *r;

  for (p = el; p; p = p->next)
  {
    for (q = el; q != p; q = q->next)
      if (q->id == p->id)
      {
 r->next = p->next;
 free (p);
 p = r;
 break;
      }
    r = p;
  }
}
# 6 "./auto-init-uninit-1.c" 2
