/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*
Remove code from C source, keep comments.  Input is from stdin, output to
stdout.  Line and column numbers are preserved.  Comments are surrounded
by brackets so that one will not run into the next if subjected to
grammatical analysis.

The -c option turns all things that look like identifiers (i.e., they contain
underscores or "::") into dashes.
*/
#include <cstdio>
#include <cstdlib>
#include <cctype>
#include <cstring>

#define FALSE 0
#define TRUE 1

int 		curr_ch;	/* Current character of input. */
int		c_option = FALSE;
				/* If TRUE, turn identifiers into dashes. */
/*
Fetch the next character of input and put it in curr_ch.
*/
#define get_char() {curr_ch = getchar();}


/*
Output curr_ch.
*/
#define put_char() {putchar(curr_ch);}

/*
Output a noncomment character.  That means to put out a blank unless the
character is a newline.
*/
#define put_blank_or_newline() \
{ if (curr_ch == '\n') { \
    put_char(); \
  } else { \
    putchar(' '); \
  }  /* if */ \
}  /* put_blank_or_newline */


static void scan_comment()
/*
Scan a comment.  The current character of input is the "*" of the
opening "/"+"*" sequence.  On return, the current character will be the
character after the closing "/".
*/
{
#define MAX_ID_LEN 1000
  char id[MAX_ID_LEN];
  int  id_len, i;
  int  is_identifier;

  /* Output a "[" for the "*". */
  putchar('[');
  for (;;) {
    get_char();
top_of_loop:
    if (c_option && isalpha(curr_ch)) {
      /* Accumulate a word, possibly an identifier. */
      id_len = 0;
      is_identifier = FALSE;
      do {
        if (id_len >= MAX_ID_LEN) {
          fprintf(stderr, "Identifier is too long.\n");
          exit(1);
        }  /* if */
        id[id_len] = curr_ch;
        id_len++;
        /* If the "word" has an underscore or "::" in it, it's an
           identifier. */
        if (curr_ch == '_' ||
            (curr_ch == ':' && id_len >= 2 && id[id_len-2] == ':')) {
          is_identifier = TRUE;
        }  /* if */
        get_char();
      } while (isalpha(curr_ch) || isdigit(curr_ch) || curr_ch == '_' ||
               curr_ch == ':');
      for (i = 0; i < id_len; i++) {
        if (is_identifier) {
          putchar('-');
        } else {
          putchar(id[i]);
        }  /* if */
      }  /* for */
    }  /* if */
    if (curr_ch == '*') {
      get_char();
      if (curr_ch == '/') goto end_comment;
      /* Put out the "*". */
      putchar('*');
      goto top_of_loop;
    }  /* if */
    if (curr_ch == EOF) {
      fprintf(stderr, "Unclosed comment.\n");
      exit(1);
    }  /* if */
    put_char();
  }  /* for */
end_comment:;
  /* Put out a "]" for the "*". */
  putchar(']');
  /* Pass over the final "/". */
  putchar(' ');
  get_char();
}  /* scan_comment */


static void scan_string()
/*
Scan a string literal or char constant.  The current character of input is
the opening quoting character.  On return, the current character will be
the character after the closing quoting character.
*/
{
  int opening_char = curr_ch;

  /* The quoting characters and the characters within the string are
     passed to output as blanks (except for any embedded newlines). */
  for (;;) {
    put_blank_or_newline();
    get_char();
    if (curr_ch == opening_char) break;
    if (curr_ch == EOF) {
      fprintf(stderr, "Unclosed string.\n");
      exit(1);
    }  /* if */
    if (curr_ch == '\\') {
      /* Escape character.  Pass next character without checking for
         closing quote. */
      put_blank_or_newline();
      get_char();
      if (curr_ch == EOF) {
        fprintf(stderr, "Unclosed string.\n");
        exit(1);
      }  /* if */
    }  /* if */
  }  /* for */
  put_blank_or_newline();
  get_char();
}  /* scan_string */


int main(int argc, char *argv[])
{
  int narg = 0;

  if (argc == 2 && strcmp((const char *)argv[1] , "-c") == 0) {
    c_option = TRUE;
    narg++;
  }  /* if */
  
  if (narg != argc-1) {
    fprintf(stderr, "Usage: get_comments [-c]\n");
    exit(1);
  }  /* if */
  
  /* Main loop.  Read a character, write a character.  Some characters
     require special handling. */
  get_char();
  while (curr_ch != EOF) {
    if (curr_ch == '/') {
      /* Check for comment. */
      put_blank_or_newline();
      get_char();
      if (curr_ch == '*') {
        scan_comment();
      }  /* if */
    } else if (curr_ch == '"' || curr_ch == '\'') {
      /* Scan string or character constant. */
      scan_string();
    } else {
      /* Normal character. */
      put_blank_or_newline();
      get_char();
    }  /* if */
  }  /* while */
  exit(0);
}  /* main */
