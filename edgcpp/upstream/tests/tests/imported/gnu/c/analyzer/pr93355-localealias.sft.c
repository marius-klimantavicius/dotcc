//type: fp
//options: 
# 0 "./analyzer/pr93355-localealias.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr93355-localealias.c"
# 29 "./analyzer/pr93355-localealias.c"
typedef long unsigned int size_t;



typedef struct _IO_FILE FILE;
extern FILE *fopen(const char *__restrict __filename,
     const char *__restrict __modes);
extern int feof_unlocked(FILE *__stream) __attribute__((__nothrow__, __leaf__));
extern char *fgets_unlocked(char *__restrict __s, int __n,
       FILE *__restrict __stream);
extern int fclose(FILE *__stream);



extern char *strchr(const char *__s, int __c)
    __attribute__((__nothrow__, __leaf__)) __attribute__((__pure__))
    __attribute__((__nonnull__(1)));
extern void *memcpy(void *__restrict __dest, const void *__restrict __src,
      size_t __n) __attribute__((__nothrow__, __leaf__))
  __attribute__((__nonnull__(1, 2)));
extern void *mempcpy(void *__restrict __dest, const void *__restrict __src,
       size_t __n) __attribute__((__nothrow__, __leaf__))
  __attribute__((__nonnull__(1, 2)));

extern size_t strlen(const char *__s) __attribute__((__nothrow__, __leaf__))
  __attribute__((__pure__)) __attribute__((__nonnull__(1)));

extern int strcasecmp(const char *__s1, const char *__s2)
    __attribute__((__nothrow__, __leaf__)) __attribute__((__pure__))
    __attribute__((__nonnull__(1, 2)));

extern int isspace(int) __attribute__((__nothrow__, __leaf__));

extern void *realloc(void *__ptr, size_t __size)
    __attribute__((__nothrow__, __leaf__))
    __attribute__((__warn_unused_result__));

typedef int (*__compar_fn_t)(const void *, const void *);
extern void *bsearch(const void *__key, const void *__base, size_t __nmemb,
       size_t __size, __compar_fn_t __compar)
    __attribute__((__nonnull__(1, 2, 5)));

extern __inline __attribute__((__gnu_inline__)) void *
bsearch(const void *__key, const void *__base, size_t __nmemb, size_t __size,
 __compar_fn_t __compar) {
  size_t __l, __u, __idx;
  const void *__p;
  int __comparison;

  __l = 0;
  __u = __nmemb;
  while (__l < __u) {
    __idx = (__l + __u) / 2;
    __p = (void *)(((const char *)__base) + (__idx * __size));
    __comparison = (*__compar)(__key, __p);
    if (__comparison < 0)
      __u = __idx;
    else if (__comparison > 0)
      __l = __idx + 1;
    else
      return (void *)__p;
  }

  return ((void *)0);
}

extern void qsort(void *__base, size_t __nmemb, size_t __size,
    __compar_fn_t __compar) __attribute__((__nonnull__(1, 4)));






extern const char *libintl_relocate(const char *pathname);
# 121 "./analyzer/pr93355-localealias.c"
struct alias_map
{
  const char *alias;
  const char *value;
};



static char *string_space;
static size_t string_space_act;
static size_t string_space_max;
static struct alias_map *map;
static size_t nmap;
static size_t maxmap;



static size_t read_alias_file (const char *fname, int fname_len)
     ;
static int extend_alias_table (void);
static int alias_compare (const struct alias_map *map1, const struct alias_map *map2)
                                    ;


const char *
_nl_expand_alias (name)
    const char *name;
{
  static const char *locale_alias_path;
  struct alias_map *retval;
  const char *result = ((void *) 0);
  size_t added;





  if (locale_alias_path == ((void *) 0))
    locale_alias_path = "value for LOCALE_ALIAS_PATH";

  do
    {
      struct alias_map item;

      item.alias = name;

      if (nmap > 0)
 retval = (struct alias_map *) bsearch (&item, map, nmap,
            sizeof (struct alias_map),
            (int (*) (const void *, const void *)

      ) alias_compare);
      else
 retval = ((void *) 0);


      if (retval != ((void *) 0))
 {
   result = retval->value;
   break;
 }


      added = 0;
      while (added == 0 && locale_alias_path[0] != '\0')
 {
   const char *start;

   while (locale_alias_path[0] == ':')
     ++locale_alias_path;
   start = locale_alias_path;

   while (locale_alias_path[0] != '\0'
   && locale_alias_path[0] != ':')
     ++locale_alias_path;

   if (start < locale_alias_path)
     added = read_alias_file (start, locale_alias_path - start);
 }
    }
  while (added != 0);





  return result;
}


static size_t

read_alias_file (fname, fname_len)
     const char *fname;
     int fname_len;
{
  FILE *fp;
  char *full_fname;
  size_t added;
  static const char aliasfile[] = "/locale.alias";

  full_fname = (char *) __builtin_alloca (fname_len + sizeof aliasfile);

  mempcpy (mempcpy (full_fname, fname, fname_len),
    aliasfile, sizeof aliasfile);





  fp = fopen (libintl_relocate (full_fname), "r");
  ;
  if (fp == ((void *) 0))
    return 0;






  added = 0;
  while (!feof_unlocked (fp))
    {







      char buf[400];
      char *alias;
      char *value;
      char *cp;

      if (fgets_unlocked (buf, sizeof buf, fp) == ((void *) 0))

 break;

      cp = buf;

      while (isspace ((unsigned char) cp[0]))
 ++cp;


      if (cp[0] != '\0' && cp[0] != '#')
 {
   alias = cp++;
   while (cp[0] != '\0' && !isspace ((unsigned char) cp[0]))
     ++cp;

   if (cp[0] != '\0')
     *cp++ = '\0';


   while (isspace ((unsigned char) cp[0]))
     ++cp;

   if (cp[0] != '\0')
     {
       size_t alias_len;
       size_t value_len;

       value = cp++;
       while (cp[0] != '\0' && !isspace ((unsigned char) cp[0]))
  ++cp;

       if (cp[0] == '\n')
  {



    *cp++ = '\0';
    *cp = '\n';
  }
       else if (cp[0] != '\0')
  *cp++ = '\0';

       if (nmap >= maxmap)
  if (__builtin_expect (extend_alias_table (), 0))
    return added;

       alias_len = strlen (alias) + 1;
       value_len = strlen (value) + 1;

       if (string_space_act + alias_len + value_len > string_space_max)
  {

    size_t new_size = (string_space_max
         + (alias_len + value_len > 1024
     ? alias_len + value_len : 1024));
    char *new_pool = (char *) realloc (string_space, new_size);
    if (new_pool == ((void *) 0))
      return added;

    if (__builtin_expect (string_space != new_pool, 0))
      {
        size_t i;

        for (i = 0; i < nmap; i++)
   {
     map[i].alias += new_pool - string_space;
     map[i].value += new_pool - string_space;
   }
      }

    string_space = new_pool;
    string_space_max = new_size;
  }

       map[nmap].alias = memcpy (&string_space[string_space_act],
     alias, alias_len);
       string_space_act += alias_len;

       map[nmap].value = memcpy (&string_space[string_space_act],
     value, value_len);
       string_space_act += value_len;

       ++nmap;
       ++added;
     }
 }



      while (strchr (buf, '\n') == ((void *) 0))
 if (fgets_unlocked (buf, sizeof buf, fp) == ((void *) 0))


   break;
    }



  fclose (fp);

  if (added > 0)
    qsort (map, nmap, sizeof (struct alias_map),
    (int (*) (const void *, const void *)) alias_compare);

  return added;
}


static int
extend_alias_table ()
{
  size_t new_size;
  struct alias_map *new_map;

  new_size = maxmap == 0 ? 100 : 2 * maxmap;
  new_map = (struct alias_map *) realloc (map, (new_size
      * sizeof (struct alias_map)));
  if (new_map == ((void *) 0))

    return -1;

  map = new_map;
  maxmap = new_size;
  return 0;
}


static int
alias_compare (map1, map2)
     const struct alias_map *map1;
     const struct alias_map *map2;
{
  return strcasecmp (map1->alias, map2->alias);
}
