//type: rp
//options: 
# 0 "./strlenopt-88.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-88.c"




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
# 6 "./strlenopt-88.c" 2



unsigned nfails;

char a[8];

void test (int line, const char *func, size_t expect)
{
  size_t len = strlen (a);
  if (len == expect)
    return;

  ++nfails;

  __builtin_printf ("assertion failed in %s on line %i: "
      "strlen (\"%s\") == %zu, got %zu\n",
      func, line, a, expect, len);
}

__attribute__ ((noipa)) const char* str (size_t n)
{
  return "9876543210" + 10 - n;
}
# 44 "./strlenopt-88.c"
__attribute__ ((noipa)) static void len_eq_1_store_nul_0 (void) { const char *s = str (1); if (strlen (s) == 1) { strcpy (a, s); a[0] = 0; test (44, "len_eq_1_store_nul_0", 0); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_eq_1_store_nul_1 (void) { const char *s = str (1); if (strlen (s) == 1) { strcpy (a, s); a[1] = 0; test (45, "len_eq_1_store_nul_1", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_eq_1_store_nul_2 (void) { const char *s = str (1); if (strlen (s) == 1) { strcpy (a, s); a[2] = 0; test (46, "len_eq_1_store_nul_2", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_eq_1_store_nul_3 (void) { const char *s = str (1); if (strlen (s) == 1) { strcpy (a, s); a[3] = 0; test (47, "len_eq_1_store_nul_3", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_eq_1_store_nul_4 (void) { const char *s = str (1); if (strlen (s) == 1) { strcpy (a, s); a[4] = 0; test (48, "len_eq_1_store_nul_4", 1); } } typedef void dummy_type;

__attribute__ ((noipa)) static void len_eq_2_store_nul_0 (void) { const char *s = str (2); if (strlen (s) == 2) { strcpy (a, s); a[0] = 0; test (50, "len_eq_2_store_nul_0", 0); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_eq_2_store_nul_1 (void) { const char *s = str (2); if (strlen (s) == 2) { strcpy (a, s); a[1] = 0; test (51, "len_eq_2_store_nul_1", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_eq_2_store_nul_2 (void) { const char *s = str (2); if (strlen (s) == 2) { strcpy (a, s); a[2] = 0; test (52, "len_eq_2_store_nul_2", 2); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_eq_2_store_nul_3 (void) { const char *s = str (2); if (strlen (s) == 2) { strcpy (a, s); a[3] = 0; test (53, "len_eq_2_store_nul_3", 2); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_eq_2_store_nul_4 (void) { const char *s = str (2); if (strlen (s) == 2) { strcpy (a, s); a[4] = 0; test (54, "len_eq_2_store_nul_4", 2); } } typedef void dummy_type;

__attribute__ ((noipa)) static void len_eq_3_store_nul_0 (void) { const char *s = str (3); if (strlen (s) == 3) { strcpy (a, s); a[0] = 0; test (56, "len_eq_3_store_nul_0", 0); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_eq_3_store_nul_1 (void) { const char *s = str (3); if (strlen (s) == 3) { strcpy (a, s); a[1] = 0; test (57, "len_eq_3_store_nul_1", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_eq_3_store_nul_2 (void) { const char *s = str (3); if (strlen (s) == 3) { strcpy (a, s); a[2] = 0; test (58, "len_eq_3_store_nul_2", 2); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_eq_3_store_nul_3 (void) { const char *s = str (3); if (strlen (s) == 3) { strcpy (a, s); a[3] = 0; test (59, "len_eq_3_store_nul_3", 3); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_eq_3_store_nul_4 (void) { const char *s = str (3); if (strlen (s) == 3) { strcpy (a, s); a[4] = 0; test (60, "len_eq_3_store_nul_4", 3); } } typedef void dummy_type;


__attribute__ ((noipa)) static void len_gt_1_store_nul_0 (void) { const char *s = str (2); if (strlen (s) > 2) { strcpy (a, s); a[0] = 0; test (63, "len_gt_1_store_nul_0", 0); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_gt_1_store_nul_1 (void) { const char *s = str (2); if (strlen (s) > 2) { strcpy (a, s); a[1] = 0; test (64, "len_gt_1_store_nul_1", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_gt_1_store_nul_2 (void) { const char *s = str (2); if (strlen (s) > 2) { strcpy (a, s); a[2] = 0; test (65, "len_gt_1_store_nul_2", 2); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_gt_1_store_nul_3 (void) { const char *s = str (2); if (strlen (s) > 2) { strcpy (a, s); a[3] = 0; test (66, "len_gt_1_store_nul_3", 2); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_gt_1_store_nul_4 (void) { const char *s = str (2); if (strlen (s) > 2) { strcpy (a, s); a[4] = 0; test (67, "len_gt_1_store_nul_4", 2); } } typedef void dummy_type;

__attribute__ ((noipa)) static void len_gt_2_store_nul_0 (void) { const char *s = str (3); if (strlen (s) > 2) { strcpy (a, s); a[0] = 0; test (69, "len_gt_2_store_nul_0", 0); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_gt_2_store_nul_1 (void) { const char *s = str (3); if (strlen (s) > 2) { strcpy (a, s); a[1] = 0; test (70, "len_gt_2_store_nul_1", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_gt_2_store_nul_2 (void) { const char *s = str (3); if (strlen (s) > 2) { strcpy (a, s); a[2] = 0; test (71, "len_gt_2_store_nul_2", 2); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_gt_2_store_nul_3 (void) { const char *s = str (3); if (strlen (s) > 2) { strcpy (a, s); a[3] = 0; test (72, "len_gt_2_store_nul_3", 3); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_gt_2_store_nul_4 (void) { const char *s = str (3); if (strlen (s) > 2) { strcpy (a, s); a[4] = 0; test (73, "len_gt_2_store_nul_4", 3); } } typedef void dummy_type;

__attribute__ ((noipa)) static void len_gt_3_store_nul_0 (void) { const char *s = str (4); if (strlen (s) > 2) { strcpy (a, s); a[0] = 0; test (75, "len_gt_3_store_nul_0", 0); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_gt_3_store_nul_1 (void) { const char *s = str (4); if (strlen (s) > 2) { strcpy (a, s); a[1] = 0; test (76, "len_gt_3_store_nul_1", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_gt_3_store_nul_2 (void) { const char *s = str (4); if (strlen (s) > 2) { strcpy (a, s); a[2] = 0; test (77, "len_gt_3_store_nul_2", 2); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_gt_3_store_nul_3 (void) { const char *s = str (4); if (strlen (s) > 2) { strcpy (a, s); a[3] = 0; test (78, "len_gt_3_store_nul_3", 3); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_gt_3_store_nul_4 (void) { const char *s = str (4); if (strlen (s) > 2) { strcpy (a, s); a[4] = 0; test (79, "len_gt_3_store_nul_4", 4); } } typedef void dummy_type;


__attribute__ ((noipa)) static void len_1_lt_4_store_nul_0 (void) { const char *s = str (1); if (strlen (s) < 4) { strcpy (a, s); a[0] = 0; test (82, "len_1_lt_4_store_nul_0", 0); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_1_lt_4_store_nul_1 (void) { const char *s = str (1); if (strlen (s) < 4) { strcpy (a, s); a[1] = 0; test (83, "len_1_lt_4_store_nul_1", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_1_lt_4_store_nul_2 (void) { const char *s = str (1); if (strlen (s) < 4) { strcpy (a, s); a[2] = 0; test (84, "len_1_lt_4_store_nul_2", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_1_lt_4_store_nul_3 (void) { const char *s = str (1); if (strlen (s) < 4) { strcpy (a, s); a[3] = 0; test (85, "len_1_lt_4_store_nul_3", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_1_lt_4_store_nul_4 (void) { const char *s = str (1); if (strlen (s) < 4) { strcpy (a, s); a[4] = 0; test (86, "len_1_lt_4_store_nul_4", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_1_lt_4_store_nul_5 (void) { const char *s = str (1); if (strlen (s) < 4) { strcpy (a, s); a[5] = 0; test (87, "len_1_lt_4_store_nul_5", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_1_lt_4_store_nul_6 (void) { const char *s = str (1); if (strlen (s) < 4) { strcpy (a, s); a[6] = 0; test (88, "len_1_lt_4_store_nul_6", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_1_lt_4_store_nul_7 (void) { const char *s = str (1); if (strlen (s) < 4) { strcpy (a, s); a[7] = 0; test (89, "len_1_lt_4_store_nul_7", 1); } } typedef void dummy_type;

__attribute__ ((noipa)) static void len_2_lt_4_store_nul_0 (void) { const char *s = str (2); if (strlen (s) < 4) { strcpy (a, s); a[0] = 0; test (91, "len_2_lt_4_store_nul_0", 0); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_2_lt_4_store_nul_1 (void) { const char *s = str (2); if (strlen (s) < 4) { strcpy (a, s); a[1] = 0; test (92, "len_2_lt_4_store_nul_1", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_2_lt_4_store_nul_2 (void) { const char *s = str (2); if (strlen (s) < 4) { strcpy (a, s); a[2] = 0; test (93, "len_2_lt_4_store_nul_2", 2); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_2_lt_4_store_nul_3 (void) { const char *s = str (2); if (strlen (s) < 4) { strcpy (a, s); a[3] = 0; test (94, "len_2_lt_4_store_nul_3", 2); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_2_lt_4_store_nul_4 (void) { const char *s = str (2); if (strlen (s) < 4) { strcpy (a, s); a[4] = 0; test (95, "len_2_lt_4_store_nul_4", 2); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_2_lt_4_store_nul_5 (void) { const char *s = str (2); if (strlen (s) < 4) { strcpy (a, s); a[5] = 0; test (96, "len_2_lt_4_store_nul_5", 2); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_2_lt_4_store_nul_6 (void) { const char *s = str (2); if (strlen (s) < 4) { strcpy (a, s); a[6] = 0; test (97, "len_2_lt_4_store_nul_6", 2); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_2_lt_4_store_nul_7 (void) { const char *s = str (2); if (strlen (s) < 4) { strcpy (a, s); a[7] = 0; test (98, "len_2_lt_4_store_nul_7", 2); } } typedef void dummy_type;

__attribute__ ((noipa)) static void len_3_lt_4_store_nul_0 (void) { const char *s = str (3); if (strlen (s) < 4) { strcpy (a, s); a[0] = 0; test (100, "len_3_lt_4_store_nul_0", 0); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_3_lt_4_store_nul_1 (void) { const char *s = str (3); if (strlen (s) < 4) { strcpy (a, s); a[1] = 0; test (101, "len_3_lt_4_store_nul_1", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_3_lt_4_store_nul_2 (void) { const char *s = str (3); if (strlen (s) < 4) { strcpy (a, s); a[2] = 0; test (102, "len_3_lt_4_store_nul_2", 2); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_3_lt_4_store_nul_3 (void) { const char *s = str (3); if (strlen (s) < 4) { strcpy (a, s); a[3] = 0; test (103, "len_3_lt_4_store_nul_3", 3); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_3_lt_4_store_nul_4 (void) { const char *s = str (3); if (strlen (s) < 4) { strcpy (a, s); a[4] = 0; test (104, "len_3_lt_4_store_nul_4", 3); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_3_lt_4_store_nul_5 (void) { const char *s = str (3); if (strlen (s) < 4) { strcpy (a, s); a[5] = 0; test (105, "len_3_lt_4_store_nul_5", 3); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_3_lt_4_store_nul_6 (void) { const char *s = str (3); if (strlen (s) < 4) { strcpy (a, s); a[6] = 0; test (106, "len_3_lt_4_store_nul_6", 3); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_3_lt_4_store_nul_7 (void) { const char *s = str (3); if (strlen (s) < 4) { strcpy (a, s); a[7] = 0; test (107, "len_3_lt_4_store_nul_7", 3); } } typedef void dummy_type;

__attribute__ ((noipa)) static void len_7_lt_8_store_nul_0 (void) { const char *s = str (7); if (strlen (s) < 8) { strcpy (a, s); a[0] = 0; test (109, "len_7_lt_8_store_nul_0", 0); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_7_lt_8_store_nul_1 (void) { const char *s = str (7); if (strlen (s) < 8) { strcpy (a, s); a[1] = 0; test (110, "len_7_lt_8_store_nul_1", 1); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_7_lt_8_store_nul_2 (void) { const char *s = str (7); if (strlen (s) < 8) { strcpy (a, s); a[2] = 0; test (111, "len_7_lt_8_store_nul_2", 2); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_7_lt_8_store_nul_3 (void) { const char *s = str (7); if (strlen (s) < 8) { strcpy (a, s); a[3] = 0; test (112, "len_7_lt_8_store_nul_3", 3); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_7_lt_8_store_nul_4 (void) { const char *s = str (7); if (strlen (s) < 8) { strcpy (a, s); a[4] = 0; test (113, "len_7_lt_8_store_nul_4", 4); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_7_lt_8_store_nul_5 (void) { const char *s = str (7); if (strlen (s) < 8) { strcpy (a, s); a[5] = 0; test (114, "len_7_lt_8_store_nul_5", 5); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_7_lt_8_store_nul_6 (void) { const char *s = str (7); if (strlen (s) < 8) { strcpy (a, s); a[6] = 0; test (115, "len_7_lt_8_store_nul_6", 6); } } typedef void dummy_type;
__attribute__ ((noipa)) static void len_7_lt_8_store_nul_7 (void) { const char *s = str (7); if (strlen (s) < 8) { strcpy (a, s); a[7] = 0; test (116, "len_7_lt_8_store_nul_7", 7); } } typedef void dummy_type;


int main (void)
{
  len_eq_1_store_nul_0 ();
  len_eq_1_store_nul_1 ();
  len_eq_1_store_nul_2 ();
  len_eq_1_store_nul_3 ();
  len_eq_1_store_nul_4 ();

  len_eq_2_store_nul_0 ();
  len_eq_2_store_nul_1 ();
  len_eq_2_store_nul_2 ();
  len_eq_2_store_nul_3 ();
  len_eq_2_store_nul_4 ();

  len_eq_3_store_nul_0 ();
  len_eq_3_store_nul_1 ();
  len_eq_3_store_nul_2 ();
  len_eq_3_store_nul_3 ();
  len_eq_3_store_nul_4 ();


  len_gt_1_store_nul_0 ();
  len_gt_1_store_nul_1 ();
  len_gt_1_store_nul_2 ();
  len_gt_1_store_nul_3 ();
  len_gt_1_store_nul_4 ();

  len_gt_2_store_nul_0 ();
  len_gt_2_store_nul_1 ();
  len_gt_2_store_nul_2 ();
  len_gt_2_store_nul_3 ();
  len_gt_2_store_nul_4 ();

  len_gt_3_store_nul_0 ();
  len_gt_3_store_nul_1 ();
  len_gt_3_store_nul_2 ();
  len_gt_3_store_nul_3 ();
  len_gt_3_store_nul_4 ();

  len_1_lt_4_store_nul_0 ();
  len_1_lt_4_store_nul_1 ();
  len_1_lt_4_store_nul_2 ();
  len_1_lt_4_store_nul_3 ();
  len_1_lt_4_store_nul_4 ();
  len_1_lt_4_store_nul_5 ();
  len_1_lt_4_store_nul_6 ();
  len_1_lt_4_store_nul_7 ();

  len_2_lt_4_store_nul_0 ();
  len_2_lt_4_store_nul_1 ();
  len_2_lt_4_store_nul_2 ();
  len_2_lt_4_store_nul_3 ();
  len_2_lt_4_store_nul_4 ();
  len_2_lt_4_store_nul_5 ();
  len_2_lt_4_store_nul_6 ();
  len_2_lt_4_store_nul_7 ();

  len_3_lt_4_store_nul_0 ();
  len_3_lt_4_store_nul_1 ();
  len_3_lt_4_store_nul_2 ();
  len_3_lt_4_store_nul_3 ();
  len_3_lt_4_store_nul_4 ();
  len_3_lt_4_store_nul_5 ();
  len_3_lt_4_store_nul_6 ();
  len_3_lt_4_store_nul_7 ();

  len_7_lt_8_store_nul_0 ();
  len_7_lt_8_store_nul_1 ();
  len_7_lt_8_store_nul_2 ();
  len_7_lt_8_store_nul_3 ();
  len_7_lt_8_store_nul_4 ();
  len_7_lt_8_store_nul_5 ();
  len_7_lt_8_store_nul_6 ();
  len_7_lt_8_store_nul_7 ();

  if (nfails)
    abort ();
}
