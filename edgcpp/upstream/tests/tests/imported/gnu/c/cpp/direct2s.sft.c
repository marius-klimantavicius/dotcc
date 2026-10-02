//type: fn
//options: 
# 0 "./cpp/direct2s.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp/direct2s.c"
# 15 "./cpp/direct2s.c"
 # include "somerandomfile"

int resync_parser_1;

 #include <somerandomfile>

int resync_parser_2;

void g1 ()
{
 # define X 1
  int resync_parser_3;
}

void g2 ()
{
 #define Y 1
  int resync_parser_4;
}

       


void f ()
{
  int i = X;
  int j = Y;
}
