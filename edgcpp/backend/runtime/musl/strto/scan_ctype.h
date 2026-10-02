/* Exact ASCII whitespace/digit classification for C/POSIX/C.UTF-8 numeric input. */
#ifndef EDG_SCAN_CTYPE_H
#define EDG_SCAN_CTYPE_H
static int __edg_scan_isspace(int c) {return c==' ' || (unsigned)(c-'\t')<5;}
static int __edg_scan_isdigit(int c) {return (unsigned)(c-'0')<10;}
#define isspace(c) __edg_scan_isspace(c)
#define isdigit(c) __edg_scan_isdigit(c)
#endif
