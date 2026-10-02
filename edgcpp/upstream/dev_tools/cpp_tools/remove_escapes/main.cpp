/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

#include <cstdio>
#include <cctype>

enum states {
	normal,
	one_esc,
	two_esc,
	three_esc,
	one_num,
	two_num,
	end_of_file
};

void output_char(int ch)
{
  static int number;
  static enum states state = normal;
  switch (state) {
    case normal:
      if (ch == '?') state = one_esc;
      break;
    case one_esc:
      if (ch == '?') {
        state = two_esc;
      } else {
	putchar('?');
	state = normal;
      }  /* if */
      break;
    case two_esc:
      if (ch == '?') {
        state = three_esc;
      } else {
	putchar('?');
	putchar('?');
	state = normal;
      }  /* if */
      break;
    case three_esc:
      if (isdigit(ch)) {
        state = one_num;
        number = (ch-'0') * 100;
      } else {
	putchar('?');
	putchar('?');
	putchar('?');
	state = normal;
      }  /* if */
      break;
    case one_num:
      number += (ch-'0') * 10;
      state = two_num;
      break;
    case two_num:
      number += (ch-'0');
      ch = number;
      state = normal;
      break;
    default:
      break;
  };
  if (ch == EOF) state = end_of_file;
  if (state == normal) putchar(ch);
}  /* process_file */


void process_file(FILE *file)
{
  int	ch;

  do {
    ch = getc(file);
    output_char(ch);
  }  while (ch != EOF);
}  /* process_file */
	

int main(int argc, char *argv[])
{
  int  i;
  FILE  *file;

  for (i = 1; i < argc; ++i) {
    char *file_name = argv[i];
    file = fopen(file_name, "r");
    if (file == NULL) {
      fprintf(stderr, "remove_escapes: cannot open %s\n", file_name);
      continue;
    }  /* if */
    process_file(file);
    fclose(file);
  }  /* for */
  if (argc == 1) process_file(stdin);
}  /* main */
