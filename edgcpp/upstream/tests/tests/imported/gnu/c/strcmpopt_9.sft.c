//type: rp
//options: 
# 0 "./strcmpopt_9.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strcmpopt_9.c"





# 1 "./strlenopt.h" 1





typedef long unsigned int size_t;
extern void abort (void);
void *calloc (size_t, size_t);
void *malloc (size_t);
void free (void *);
char *strdup (const char *);
size_t strlen (const char *);
size_t strnlen (const char *, size_t);
void *memcpy (void *__restrict, const void *__restrict, size_t);
void *memmove (void *, const void *, size_t);
char *strcpy (char *__restrict, const char *__restrict);
char *strcat (char *__restrict, const char *__restrict);
char *strchr (const char *, int);
int strcmp (const char *, const char *);
int strncmp (const char *, const char *, size_t);
void *memset (void *, int, size_t);
int memcmp (const void *, const void *, size_t);
int strcmp (const char *, const char *);





int sprintf (char * __restrict, const char *__restrict, ...);
int snprintf (char * __restrict, size_t, const char *__restrict, ...);
# 7 "./strcmpopt_9.c" 2



__attribute__ ((noclone, noinline, noipa)) size_t
ident (size_t x)
{
  return x;
}

int nfails;

__attribute__ ((noclone, noinline, noipa)) void
failure_on_line (int line)
{
  __builtin_printf ("failure on line %i\n", line);
  ++nfails;
}

# 1 "./strcmpopt_8.c" 1
# 11 "./strcmpopt_8.c"
typedef long unsigned int size_t;





extern void failure_on_line (int);
# 26 "./strcmpopt_8.c"
void test_literal (void)
{
  size_t max;

  do { max = ident (0); if (!(0 == __builtin_strncmp ("123", "1234", max))) failure_on_line (30); } while (0);
  do { max = ident (1); if (!(0 == __builtin_strncmp ("123", "1234", max))) failure_on_line (31); } while (0);
  do { max = ident (2); if (!(0 == __builtin_strncmp ("123", "1234", max))) failure_on_line (32); } while (0);
  do { max = ident (3); if (!(0 == __builtin_strncmp ("123", "1234", max))) failure_on_line (33); } while (0);
  do { max = ident (4); if (!(0 > __builtin_strncmp ("123", "1234", max))) failure_on_line (34); } while (0);
  do { max = ident (5); if (!(0 > __builtin_strncmp ("123", "1234", max))) failure_on_line (35); } while (0);
  do { max = ident (0xffffffffffffffffUL - 2); if (!(0 > __builtin_strncmp ("123", "1234", max))) failure_on_line (36); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(0 > __builtin_strncmp ("123", "1234", max))) failure_on_line (37); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(0 > __builtin_strncmp ("123", "1234", max))) failure_on_line (38); } while (0);

  do { max = ident (0); if (!(0 == __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (40); } while (0);
  do { max = ident (1); if (!(0 < __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (41); } while (0);
  do { max = ident (2); if (!(0 < __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (42); } while (0);
  do { max = ident (3); if (!(0 < __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (43); } while (0);
  do { max = ident (4); if (!(0 < __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (44); } while (0);
  do { max = ident (0xffffffffffffffffUL - 2); if (!(0 < __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (45); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(0 < __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (46); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(0 < __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (47); } while (0);

  do { max = ident (0); if (!(0 == __builtin_strncmp ("123" + 1, "1234" + 1, max))) failure_on_line (49); } while (0);
  do { max = ident (1); if (!(0 == __builtin_strncmp ("123" + 1, "1234" + 1, max))) failure_on_line (50); } while (0);
  do { max = ident (2); if (!(0 == __builtin_strncmp ("123" + 1, "1234" + 1, max))) failure_on_line (51); } while (0);
  do { max = ident (3); if (!(0 > __builtin_strncmp ("123" + 1, "1234" + 1, max))) failure_on_line (52); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(0 > __builtin_strncmp ("123" + 1, "1234" + 1, max))) failure_on_line (53); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(0 > __builtin_strncmp ("123" + 1, "1234" + 1, max))) failure_on_line (54); } while (0);

  do { max = ident (0); if (!(0 == __builtin_strncmp ("123" + 3, "1234" + 1, max))) failure_on_line (56); } while (0);
  do { max = ident (1); if (!(0 > __builtin_strncmp ("123" + 3, "1234" + 1, max))) failure_on_line (57); } while (0);
  do { max = ident (2); if (!(0 > __builtin_strncmp ("123" + 3, "1234" + 1, max))) failure_on_line (58); } while (0);
  do { max = ident (3); if (!(0 > __builtin_strncmp ("123" + 3, "1234" + 1, max))) failure_on_line (59); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(0 > __builtin_strncmp ("123" + 3, "1234" + 1, max))) failure_on_line (60); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(0 > __builtin_strncmp ("123" + 3, "1234" + 1, max))) failure_on_line (61); } while (0);

  int zero = 0;

  do { max = ident (0); if (!(zero == __builtin_strncmp ("123", "1234", max))) failure_on_line (65); } while (0);
  do { max = ident (1); if (!(zero == __builtin_strncmp ("123", "1234", max))) failure_on_line (66); } while (0);
  do { max = ident (2); if (!(zero == __builtin_strncmp ("123", "1234", max))) failure_on_line (67); } while (0);
  do { max = ident (3); if (!(zero == __builtin_strncmp ("123", "1234", max))) failure_on_line (68); } while (0);
  do { max = ident (4); if (!(zero > __builtin_strncmp ("123", "1234", max))) failure_on_line (69); } while (0);
  do { max = ident (5); if (!(zero > __builtin_strncmp ("123", "1234", max))) failure_on_line (70); } while (0);
  do { max = ident (0xffffffffffffffffUL - 2); if (!(zero > __builtin_strncmp ("123", "1234", max))) failure_on_line (71); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(zero > __builtin_strncmp ("123", "1234", max))) failure_on_line (72); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(zero > __builtin_strncmp ("123", "1234", max))) failure_on_line (73); } while (0);

  do { max = ident (0); if (!(zero == __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (75); } while (0);
  do { max = ident (1); if (!(zero < __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (76); } while (0);
  do { max = ident (2); if (!(zero < __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (77); } while (0);
  do { max = ident (3); if (!(zero < __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (78); } while (0);
  do { max = ident (4); if (!(zero < __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (79); } while (0);
  do { max = ident (0xffffffffffffffffUL - 2); if (!(zero < __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (80); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(zero < __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (81); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(zero < __builtin_strncmp ("123" + 1, "1234", max))) failure_on_line (82); } while (0);

  do { max = ident (0); if (!(zero == __builtin_strncmp ("123" + 1, "1234" + 1, max))) failure_on_line (84); } while (0);
  do { max = ident (1); if (!(zero == __builtin_strncmp ("123" + 1, "1234" + 1, max))) failure_on_line (85); } while (0);
  do { max = ident (2); if (!(zero == __builtin_strncmp ("123" + 1, "1234" + 1, max))) failure_on_line (86); } while (0);
  do { max = ident (3); if (!(zero > __builtin_strncmp ("123" + 1, "1234" + 1, max))) failure_on_line (87); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(zero > __builtin_strncmp ("123" + 1, "1234" + 1, max))) failure_on_line (88); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(zero > __builtin_strncmp ("123" + 1, "1234" + 1, max))) failure_on_line (89); } while (0);

  do { max = ident (0); if (!(zero == __builtin_strncmp ("123" + 3, "1234" + 1, max))) failure_on_line (91); } while (0);
  do { max = ident (1); if (!(zero > __builtin_strncmp ("123" + 3, "1234" + 1, max))) failure_on_line (92); } while (0);
  do { max = ident (2); if (!(zero > __builtin_strncmp ("123" + 3, "1234" + 1, max))) failure_on_line (93); } while (0);
  do { max = ident (3); if (!(zero > __builtin_strncmp ("123" + 3, "1234" + 1, max))) failure_on_line (94); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(zero > __builtin_strncmp ("123" + 3, "1234" + 1, max))) failure_on_line (95); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(zero > __builtin_strncmp ("123" + 3, "1234" + 1, max))) failure_on_line (96); } while (0);
}

const char s123[] = "123";
const char s1234[] = "1234";

void test_cst_array (void)
{
  size_t max;

  do { max = ident (0); if (!(0 == __builtin_strncmp (s123, s1234, max))) failure_on_line (106); } while (0);
  do { max = ident (1); if (!(0 == __builtin_strncmp (s123, s1234, max))) failure_on_line (107); } while (0);
  do { max = ident (2); if (!(0 == __builtin_strncmp (s123, s1234, max))) failure_on_line (108); } while (0);
  do { max = ident (3); if (!(0 == __builtin_strncmp (s123, s1234, max))) failure_on_line (109); } while (0);
  do { max = ident (4); if (!(0 > __builtin_strncmp (s123, s1234, max))) failure_on_line (110); } while (0);
  do { max = ident (5); if (!(0 > __builtin_strncmp (s123, s1234, max))) failure_on_line (111); } while (0);
  do { max = ident (0xffffffffffffffffUL - 2); if (!(0 > __builtin_strncmp (s123, s1234, max))) failure_on_line (112); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(0 > __builtin_strncmp (s123, s1234, max))) failure_on_line (113); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(0 > __builtin_strncmp (s123, s1234, max))) failure_on_line (114); } while (0);

  do { max = ident (0); if (!(0 == __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (116); } while (0);
  do { max = ident (1); if (!(0 < __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (117); } while (0);
  do { max = ident (2); if (!(0 < __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (118); } while (0);
  do { max = ident (3); if (!(0 < __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (119); } while (0);
  do { max = ident (4); if (!(0 < __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (120); } while (0);
  do { max = ident (0xffffffffffffffffUL - 2); if (!(0 < __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (121); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(0 < __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (122); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(0 < __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (123); } while (0);

  do { max = ident (0); if (!(0 == __builtin_strncmp (s123 + 1, s1234 + 1, max))) failure_on_line (125); } while (0);
  do { max = ident (1); if (!(0 == __builtin_strncmp (s123 + 1, s1234 + 1, max))) failure_on_line (126); } while (0);
  do { max = ident (2); if (!(0 == __builtin_strncmp (s123 + 1, s1234 + 1, max))) failure_on_line (127); } while (0);
  do { max = ident (3); if (!(0 > __builtin_strncmp (s123 + 1, s1234 + 1, max))) failure_on_line (128); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(0 > __builtin_strncmp (s123 + 1, s1234 + 1, max))) failure_on_line (129); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(0 > __builtin_strncmp (s123 + 1, s1234 + 1, max))) failure_on_line (130); } while (0);

  do { max = ident (0); if (!(0 == __builtin_strncmp (s123 + 3, s1234 + 1, max))) failure_on_line (132); } while (0);
  do { max = ident (1); if (!(0 > __builtin_strncmp (s123 + 3, s1234 + 1, max))) failure_on_line (133); } while (0);
  do { max = ident (2); if (!(0 > __builtin_strncmp (s123 + 3, s1234 + 1, max))) failure_on_line (134); } while (0);
  do { max = ident (3); if (!(0 > __builtin_strncmp (s123 + 3, s1234 + 1, max))) failure_on_line (135); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(0 > __builtin_strncmp (s123 + 3, s1234 + 1, max))) failure_on_line (136); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(0 > __builtin_strncmp (s123 + 3, s1234 + 1, max))) failure_on_line (137); } while (0);

  int zero = 0;

  do { max = ident (0); if (!(zero == __builtin_strncmp (s123, s1234, max))) failure_on_line (141); } while (0);
  do { max = ident (1); if (!(zero == __builtin_strncmp (s123, s1234, max))) failure_on_line (142); } while (0);
  do { max = ident (2); if (!(zero == __builtin_strncmp (s123, s1234, max))) failure_on_line (143); } while (0);
  do { max = ident (3); if (!(zero == __builtin_strncmp (s123, s1234, max))) failure_on_line (144); } while (0);
  do { max = ident (4); if (!(zero > __builtin_strncmp (s123, s1234, max))) failure_on_line (145); } while (0);
  do { max = ident (5); if (!(zero > __builtin_strncmp (s123, s1234, max))) failure_on_line (146); } while (0);
  do { max = ident (0xffffffffffffffffUL - 2); if (!(zero > __builtin_strncmp (s123, s1234, max))) failure_on_line (147); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(zero > __builtin_strncmp (s123, s1234, max))) failure_on_line (148); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(zero > __builtin_strncmp (s123, s1234, max))) failure_on_line (149); } while (0);

  do { max = ident (0); if (!(zero == __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (151); } while (0);
  do { max = ident (1); if (!(zero < __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (152); } while (0);
  do { max = ident (2); if (!(zero < __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (153); } while (0);
  do { max = ident (3); if (!(zero < __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (154); } while (0);
  do { max = ident (4); if (!(zero < __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (155); } while (0);
  do { max = ident (0xffffffffffffffffUL - 2); if (!(zero < __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (156); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(zero < __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (157); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(zero < __builtin_strncmp (s123 + 1, s1234, max))) failure_on_line (158); } while (0);

  do { max = ident (0); if (!(zero == __builtin_strncmp (s123 + 1, s1234 + 1, max))) failure_on_line (160); } while (0);
  do { max = ident (1); if (!(zero == __builtin_strncmp (s123 + 1, s1234 + 1, max))) failure_on_line (161); } while (0);
  do { max = ident (2); if (!(zero == __builtin_strncmp (s123 + 1, s1234 + 1, max))) failure_on_line (162); } while (0);
  do { max = ident (3); if (!(zero > __builtin_strncmp (s123 + 1, s1234 + 1, max))) failure_on_line (163); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(zero > __builtin_strncmp (s123 + 1, s1234 + 1, max))) failure_on_line (164); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(zero > __builtin_strncmp (s123 + 1, s1234 + 1, max))) failure_on_line (165); } while (0);

  do { max = ident (0); if (!(zero == __builtin_strncmp (s123 + 3, s1234 + 1, max))) failure_on_line (167); } while (0);
  do { max = ident (1); if (!(zero > __builtin_strncmp (s123 + 3, s1234 + 1, max))) failure_on_line (168); } while (0);
  do { max = ident (2); if (!(zero > __builtin_strncmp (s123 + 3, s1234 + 1, max))) failure_on_line (169); } while (0);
  do { max = ident (3); if (!(zero > __builtin_strncmp (s123 + 3, s1234 + 1, max))) failure_on_line (170); } while (0);
  do { max = ident (0xffffffffffffffffUL - 1); if (!(zero > __builtin_strncmp (s123 + 3, s1234 + 1, max))) failure_on_line (171); } while (0);
  do { max = ident (0xffffffffffffffffUL); if (!(zero > __builtin_strncmp (s123 + 3, s1234 + 1, max))) failure_on_line (172); } while (0);
}
# 26 "./strcmpopt_9.c" 2

int main (void)
{
  test_literal ();
  test_cst_array ();

  if (nfails)
    __builtin_abort ();
}
