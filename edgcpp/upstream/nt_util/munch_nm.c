/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Program that filters Windows link -dump output so that it can be read
by edg_munch.

*/

#include <basics.h>
#include <stdio.h>

/*
Lines from standard input are read into this buffer for analysis.
*/
#define INPUT_LINE_SIZE 32767
static char   input_line_buffer[INPUT_LINE_SIZE];
static int    line_size;
                /* Number of characters in the input line not including
                   the final null. */

static int read_input_line(void)
/*
Reads a line of input from standard input.  Returns TRUE if a line of
input is being returned.  Returns FALSE at end-of-file.  Sets "line_size"
to the number of characters read not including the trailing null character.
*/
{
  register char*    buffer_pos = &input_line_buffer[0];
  register int      size = 0;
  register int      ch;
  int               result;

  while (ch = getchar(), ch != EOF && ch != '\n') {
    if (++size > INPUT_LINE_SIZE) {
      fprintf(stderr, "munch_nm: read_input_line: input line too long.");
    }  /* if */
    *buffer_pos++ = ch;
  }  /* while */
  
  /* Terminate string with a null character. */
  *buffer_pos++ = '\0';

  /* Determine whether to return end-of-file (FALSE). */
  result = TRUE;
  if (ch == EOF && size == 0) result = FALSE;

  /* Save the number of characters read. */
  line_size = size;
  return (result);
}  /* read_input_line */


int main()
{
  char	*ptr;
  /* Look for a line that begins with "SYMBOL TABLE". */
  while (read_input_line()) {
    if (strncmp(input_line_buffer, "COFF SYMBOL TABLE", 17) == 0) break;
  }  /* while */
  /* Process lines until we find a line that says "STRING TABLE". */
  while (read_input_line()) {
    if (strncmp(input_line_buffer, "STRING TABLE", 12) == 0) break;
    /* Ignore lines that begin with a blank.  These are continution lines. */
    if (input_line_buffer[0] == ' ') continue;
    /* The name is separated from the rest of the line by a "|". */
    ptr = strchr(input_line_buffer, '|');
    if (ptr != NULL) ptr += 2;
    if (ptr != NULL) {
      if (strncmp(ptr, "___sti__", 8) == 0 ||
          strncmp(ptr, "___std__", 8) == 0) {
        /* Find the blank after the end of the symbol and replace it with
           a null terminator character. */
        char	*end = strchr(ptr, ' ');
        if (end != NULL) *end = '\0';
        printf("00000000 T %s\n", ptr);
      }  /* if */
    }  /* if */
  }  /* while */
  exit(0);
}



