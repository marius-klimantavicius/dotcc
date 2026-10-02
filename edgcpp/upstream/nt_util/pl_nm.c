/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Program that filters Windows link -dump output so that it can be read
by edg_prelink.

*/

#include <basics.h>
#include <stdio.h>
#include <assert.h>

#ifdef DEBUG
#undef DEBUG
#endif /* DEBUG */
#define DEBUG 0

/*
Lines from standard input are read into this buffer for analysis.
*/
#define INPUT_LINE_SIZE 32767
static char   input_line_buffer[INPUT_LINE_SIZE];
static int    line_size;
                /* Number of characters in the input line not including
                   the final null. */

FILE		*input_file;
		/* File from which the link command output will be read. */



static void error_exit(void)
/*
Exit with an error status.
*/
{
  exit(1);
}


static void *alloc_general(sizeof_t size)
{
  void*		ptr;

  ptr = (void*)malloc(size);
  if (ptr == NULL) {
    fprintf(stderr, "prelink_nm: out of memory.\n");
    error_exit();
  }  /* if */
  return ptr;
}  /* alloc_general */


static char *copy_of_string(char *source)
/*
Allocate space for a copy of the string and make a copy.  Return a pointer
to the copy.
*/
{
  char	*dest;
  dest = (char *)alloc_general(strlen(source) + 1);
  strcpy(dest, source);
  return dest;
}  /* copy_of_string */


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

  while (ch = getc(input_file), ch != EOF && ch != '\n') {
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
#if DEBUG
  fprintf(stderr, "input:%s\n", input_line_buffer);
#endif /* DEBUG */
  return (result);
}  /* read_input_line */


static a_boolean find_line(a_const_char *starting_string)
/*
Read lines until one is found that begins with the specified string.
*/
{
  int		len = (int)strlen(starting_string);
  a_boolean	found = FALSE;

  while (read_input_line()) {
    if (strncmp(input_line_buffer, starting_string, len) == 0) {
      found = TRUE;
      break;
    }  /* if */
  }  /* while */
  return found;
}  /* find_line */


/*
See if the first N characters of str1 are the same as substring.
*/
#define substring_cmp(str1, substring)					\
  (strncmp(str1, substring, strlen(substring)))


void process_file(char *file_to_process)
{
  char		*ptr;
  char		*file_name;
  a_const_char	*find_string;
  int		file_number = 0;
  char		*name;
  char		*status;
  char		*type;
  char		*func;
  char		*linkage;
  a_boolean	in_text_section;
  a_boolean	is_archive;
  a_boolean	undefined;
  a_boolean	is_function;
  char		new_type;

  /* Execute the link -dump -symbols command and direct its output to
     the file "input_file". */
  sprintf(input_line_buffer, "link -dump -symbols %s", file_to_process);
  input_file = _popen(input_line_buffer, "r");

  /* Look for a line of the format:

	Dump of file \xxx\yyy\abc.obj
	Dump of file \xxx\yyy\abc.lib
  */
  find_string = "Dump of file ";
  if (!find_line(find_string)) goto done;
  /* Save the file name. */
  ptr = &input_line_buffer[strlen(find_string)];
  file_name = copy_of_string(ptr);

  /* Look for "File Type: LIBRARY" or "File Type: COFF OBJECT". */
  find_string = "File Type: ";
  if (!find_line(find_string)) goto done;
  ptr = &input_line_buffer[strlen(find_string)];
  is_archive = strcmp(ptr, "LIBRARY") == 0;

  for (;; file_number++) {
    /* Look for "SYMBOL TABLE". */
    if (!find_line("COFF SYMBOL TABLE")) break;

    in_text_section = FALSE;
    while (read_input_line()) {
      int	line_offset = 0;
      /* Each object file is terminated by a "STRING TABLE" entry. */
      if (strncmp(input_line_buffer, "STRING TABLE", 12) == 0) break;
      /* Ignore lines that begin with a blank.  These are continution lines. */
      if (input_line_buffer[0] == ' ') continue;
      /* Ignore NULL lines. */
      if (input_line_buffer[0] == '\0') continue;
      /* Ignore lines short lines that contain header information. */
      if (line_size < 49) continue;
      /* Process lines of the format:


0---------1---------2---------3---------4---------5---------6---------7--------
0123456789012345678901234567890123456789012345678901234567890123456789012345678
00C 00000000 UNDEF  notype ()    External     | _f__10A__pt__2_iFv
00D 00000000 UNDEF  notype ()    External     | __main
00B 00000000 ___curr_eh_stack_entry           UNDEF notype       External
00C 0000006E ___vec_new_eh                    SECT3 notype ()    External
1000 0000006E ___vec_new_eh                    SECT3 notype ()    External

      */
      /* Get the identifier name from the line. */
      /* The name is separated from the rest of the line by a '|'. */
      name = strchr(&input_line_buffer[45], '|');
      if (name != NULL) name += 2;
      /* Look for a directive that marks the start of a section. */
      if (name != NULL && strncmp(name, ".text", 5) == 0) {
        in_text_section = TRUE;
        continue;
      } else if (name == NULL || name[0] == '.') {
        /* Some other directive -- skip this line. */
        continue;
      }  /* if */
      /* Find the first nonblank after the end of the name. */
      ptr = &input_line_buffer[0];
      /* Find the first blank after the sequence number.  Use that to adjust
         the line pointer to the position expected for the other fields. */
      { char	*blank_pos;
        blank_pos = strchr(ptr, ' ');
        if (blank_pos != NULL) {
          line_offset = (int)(blank_pos - ptr - 3);
          assert(line_offset >= 0);
          ptr += line_offset;
        }  /* if */
      }
      status = &ptr[13];   /* UNDEF or SECT3 in the example. */
      type = &ptr[20];     /* notype in the example. */
      func = &ptr[27];    /* () or "  " in the example. */
      linkage = &ptr[33]; /* External in the exampe. */
      /* Skip static symbols. */
      if (substring_cmp(linkage, "External") != 0) continue;
      /* Is this a definition or a reference? */
      undefined = substring_cmp(status, "UNDEF") == 0;
      is_function = substring_cmp(func, "()") == 0;
      /* Figure out the UNIX style type code. */
      if (undefined && !in_text_section) {
        /* An undefined data symbol is actually a tentative definition. */
        new_type = 'C';
      } else if (undefined) {
        /* All other undefined symbols are just plain undefined. */
        new_type = 'U';
      } else if (in_text_section) {
        /* A definition in the text section. */
        new_type = 'T';
      } else {
        /* A definition in some other section. */
        new_type = 'D';
      }  /* if */
      /* Generate the output line. */
      printf("%s:", file_name);
      if (is_archive) {
        printf("%03d.obj:", file_number);
      }  /* if */
      printf("00000000 %c %s\n", new_type, name);
#if DEBUG
      fprintf(stderr, "output:00000000 %c %s\n", new_type, name);
#endif /* DEBUG */
    }  /* while */
  }  /* for */
done:
  if (input_file != NULL) fclose(input_file);
}  /* process_file */

int main(int argc, char *argv[])
{
  int	i;
  for (i = 1; i < argc; ++i) {
    process_file(argv[i]);
  }  /* for */
  exit(0);
}



