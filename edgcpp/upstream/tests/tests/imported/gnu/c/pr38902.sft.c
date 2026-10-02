//type: rp
//options: 
# 0 "./pr38902.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr38902.c"
# 10 "./pr38902.c"
extern int sprintf (char *, const char *, ...);



extern void abort (void);
# 45 "./pr38902.c"
char *heap = "1234567890abcdefghijklmnopqrstuvwxyz";

int failed = 0;
# 65 "./pr38902.c"
void test0 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; __builtin_memcpy (buffer, "1234567890abcdefghijklmnopqrstuvwxyz", __builtin_strlen("1234567890abcdefghijklmnopqrstuvwxyz")+1);; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };
void test1 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; __builtin_memcpy (buffer, local, __builtin_strlen(local)+1);; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };
void test2 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; __builtin_memcpy (buffer, heap, __builtin_strlen(heap)+1);; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };

void test3 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; __builtin_memmove (buffer, "1234567890abcdefghijklmnopqrstuvwxyz", __builtin_strlen("1234567890abcdefghijklmnopqrstuvwxyz")+1);; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };
void test4 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; __builtin_memmove (buffer, local, __builtin_strlen(local)+1);; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };
void test5 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; __builtin_memmove (buffer, heap, __builtin_strlen(heap)+1);; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };

void test6 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; __builtin_strcpy (buffer, "1234567890abcdefghijklmnopqrstuvwxyz");; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };
void test7 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; __builtin_strcpy (buffer, local);; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };
void test8 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; __builtin_strcpy (buffer, heap);; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };

void test9 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; sprintf (buffer, "1234567890abcdefghijklmnopqrstuvwxyz");; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };
void test10 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; sprintf (buffer, local);; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };
void test11 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; sprintf (buffer, heap);; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };

void test12 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; sprintf (buffer, "%s", "1234567890abcdefghijklmnopqrstuvwxyz");; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };
void test13 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; sprintf (buffer, "%s", local);; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };
void test14 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; sprintf (buffer, "%s", heap);; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };

void test15 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; __builtin_strcat (buffer, "1234567890abcdefghijklmnopqrstuvwxyz");; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };
void test16 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; __builtin_strcat (buffer, local);; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };
void test17 (void) { char *local = "1234567890abcdefghijklmnopqrstuvwxyz"; char buffer[1024]=""; __builtin_strcat (buffer, heap);; if (__builtin_strcmp(buffer, heap) == 0) { ; } else { ; failed++; } };

void mongoose(void)
{
  char buffer[1024]="";
  sprintf (buffer, "%s", "1234567890abcdefghijklmnopqrstuvwxyz");;
    if (__builtin_strcmp(buffer, heap) == 0) {
        ;
    }
    else {
        ;
 failed++;
    }
}

int main (int argc, char *argv[])
{
  test0();
  test1();
  test2();
  test3();
  test4();
  test5();
  test6();
  test7();
  test8();
  test9();
  test10();
  test11();


  test12();
  mongoose();

  test13();
  test14();
  test15();
  test16();
  test17();

  if (failed)
    abort ();

  return 0;
}
