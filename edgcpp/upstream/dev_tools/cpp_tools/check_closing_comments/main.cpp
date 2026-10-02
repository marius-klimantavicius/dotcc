/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

#include <cstdio>
#include <cstdlib>
#include <cstring>

/*
The value passed to exit at the end of processing; non-zero indicates a
failure.
*/
int result = 0;

/*
Definition of a class representing a single source line.  The objects are
kept in a singly-linked list rooted in last_line.
*/
struct a_line {
  a_line	*prev;	/* Points to the preceding line object, null for
			   the first line of the file. */
  unsigned	line_number;
			/* The line number from the source file at which
			   this line is found. */
  char		*str;	/* Points to a null-terminated copy of the line
			   from the file. */
  a_line(const char *buffer, a_line *prev_line);
  static unsigned
		last_line_number;
			/* The line number of the most recently read line
			   from the file, incremented as each line is
			   read. */
};

unsigned a_line::last_line_number = 0;

a_line::a_line(const char* buffer, a_line *prev_line)
  : prev(prev_line), line_number(++last_line_number) {
  unsigned len = strlen(buffer) + 1;
  str = new char[len];
  strcpy(str, buffer);
}  /* a_line::a_line */

a_line *last_line = nullptr;

void scan_file_for_closing_comments(FILE *source_file)
/*
Read the lines of source_file into a linked list of a_line structures, then
traverse them in reverse order looking for function closing comments and
the corresponding function declarator, reporting an error for each
malformed comment or comment that does not match the function
declarator-id.  A closing comment is identified by the first six characters
of the line being "}  /" followed by "* " (separated to avoid compiler
warnings about embedding a comment delimiter inside a comment); the region
to search for a matching declarator-id starts at the nearest preceding line
beginning with "{" and extending backward to the next line beginning with
'}' (in the first column) or the first line of the source file, whichever
comes first.  Whether the closing comment has a matching declarator-id is
based simply on the occurrence of the name in the search region, not a full
parse of the declarator syntax, and is thus subject to false positives
resulting from matching a string in the header comment or parameter list.
*/
{
  char              buffer[1024];
  a_line            *curr_closing_comment = nullptr;
  char              *fcn_name = nullptr;
  unsigned          name_len;
  bool              matched_name = false;
  bool              scanning_for_fcn_name = false;
  char              nulled_char = ' ';
  static const char id_chars[] =
             "0123456789ABCEDFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";

  /* Read the lines of the file, populating the list of a_line objects. */
  while (fgets(buffer, sizeof(buffer), source_file) != NULL) {
    last_line = new a_line(buffer, last_line);
  }  /* while */
  fclose(source_file);
  /* Scan through the lines of the file in reverse order, checking for
     errors. */
  for (a_line *lp = last_line; lp != nullptr; lp = lp->prev) {
    if (lp->str[0] == '}' || lp->prev == nullptr) {
      /* This is the end of the preceding function/entity or the top of the
         file.  Check to see if we have an unmatched closing comment. */
      if (fcn_name != nullptr && !matched_name) {
        /* Report an unmatched function name.  Restore the character that
           was overwritten by a null following the function name so we can
           print the entire closing comment line. */
        fcn_name[name_len] = nulled_char;
        printf("No matching function name (line %u):\n%s\n",
               curr_closing_comment->line_number, curr_closing_comment->str);
        result = 1;
      }  /* if */
      fcn_name = nullptr;
      scanning_for_fcn_name = false;
      if (lp->prev != nullptr) {
        /* This is a line beginning with '}', so we assume it marks the end
           of the preceding function/entity, but it is not necessarily a
           well-formed closing comment line. */
        if (strncmp(lp->str, "}  /* ", 6) == 0) {
          /* This is a well-formed closing comment line.  Remember the line
             and the start of the function name, and overwrite the space
             character or parenthesis following the function name with a
             null character so it can be easily used in the search for a
             matching declarator-id. */
          curr_closing_comment = lp;
          fcn_name = lp->str + 6;
          char *name_end = fcn_name + 1;
          while (*name_end != ' ' && *name_end != '(' && *name_end != '\0') {
            if (name_end[0] == ':' && name_end[1] == ':') {
              /* Skip over the "::" so we will match only on the function
                 name.  This is necessary because the closing comments of
                 member functions of class templates generally use only the
                 template name as the qualifier, while the declarator-id
                 uses the template name with template parameters or
                 arguments, e.g., "template<> void foo<int>::bar()" may be
                 closed with a comment whose name is "foo::bar".
                 Attempting to match on the entire qualified name would
                 spuriously fail. */
              fcn_name = name_end + 2;
              name_end = fcn_name + 1;
            } else {
              ++name_end;
            }  /* if */
          }  /* while */
          if (*name_end == '\0') {
            /* A missing space before the closing comment delimiter.
               Report the problem and do not try to match the name. */
            printf("Missing space at end of name (line %u):\n%s\n",
                   lp->line_number, lp->str);
            result = 1;
            fcn_name = nullptr;
            curr_closing_comment = nullptr;
          } else if (strncmp(fcn_name, "namespace ", 10) == 0 ||
                     strncmp(fcn_name, "extern ", 7) == 0) {
            /* Exclude namespace and extern "C" closing comments, as the
               beginning of the construct cannot consistently be
               detected. */
            fcn_name = nullptr;
          } else {
            name_len = name_end - fcn_name;
            nulled_char = fcn_name[name_len];
            fcn_name[name_len] = '\0';
          }  /* if */
        } else if (lp->str[1] != ';' && lp->str[1] != ',' &&
                   strchr(id_chars, lp->str[3]) == 0) {
          /* Report a malformed closing comment.  (The various character
             checks avoid closing comments for class definitions and such,
             which follow different formatting rules at the beginning of
             the declaration and thus would cause spurious error
             reports.) */
          printf("Incorrect closing line formatting (line %u):\n%s\n",
                 lp->line_number, lp->str);
          result = 1;
        }  /* if */
      }  /* if */
    } else if (lp->str[0] == '{' && fcn_name != nullptr) {
      /* This is presumably the start of the function body.  Start looking
         for a matching declarator-id in the preceding lines. */
      scanning_for_fcn_name = true;
      matched_name = false;
    } else if (scanning_for_fcn_name && !matched_name) {
      /* Check to see if the function name appears in this line as an
         identifier, i.e., that a substring matching the function name is
         not just part of a larger identifier. */
      const char *p = strstr(lp->str, fcn_name);
      while (p != nullptr) {
        if ((p == lp->str || strchr(id_chars, p[-1]) == 0) &&
            strchr(id_chars, p[name_len]) == 0) {
          /* We found a matching identifier. */
          scanning_for_fcn_name = false;
          matched_name = true;
          fcn_name = nullptr;
          p = nullptr;
        } else {
          /* Handle a case like "foo_ptr foo()", where the first occurrence
             of the function name is not a distinct identifier. */
          p = strstr(p + name_len, fcn_name);
        }  /* if */
      }  /* while */
    }  /* if */
  }  /* for */
}  /* scan_file_for_closing_comments */

int main(int argc, char *argv[]) {
  FILE *source_file = NULL;

  if (argc != 2) {
    fprintf(stderr, "Usage: %s source-file\n", argv[0]);
    exit(2);
  }  /* if */
  if ((source_file = fopen(argv[1], "r")) == NULL) {
    fprintf(stderr, "Cannot open %s for reading.\n", argv[1]);
    exit(3);
  }  /* if */
  scan_file_for_closing_comments(source_file);
  exit(result);
}  /* main */
