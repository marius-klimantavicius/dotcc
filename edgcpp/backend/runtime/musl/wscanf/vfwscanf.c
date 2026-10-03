#include <stdio.h>
#include <stdlib.h>
#include <stdarg.h>
#include <ctype.h>
#include <wchar.h>
#include <wctype.h>
#include <limits.h>
#include <string.h>

#include "wscan_file.h"
#include "../strto/scan_string.h"
#include "../strto/intscan.h"
#include "../strto/floatscan.h"

#define SIZE_hh -2
#define SIZE_h  -1
#define SIZE_def 0
#define SIZE_l   1
#define SIZE_L   2
#define SIZE_ll  3

static void store_int(void *dest, int size, unsigned long long i)
{
	if (!dest) return;
	switch (size) {
	case SIZE_hh:
		*(char *)dest = i;
		break;
	case SIZE_h:
		*(short *)dest = i;
		break;
	case SIZE_def:
		*(int *)dest = i;
		break;
	case SIZE_l:
		*(long *)dest = i;
		break;
	case SIZE_ll:
		*(long long *)dest = i;
		break;
	}
}

static void *arg_n(va_list ap, unsigned int n)
{
	void *p;
	unsigned int i;
	va_list ap2;
	va_copy(ap2, ap);
	for (i=n; i>1; i--) va_arg(ap2, void *);
	p = va_arg(ap2, void *);
	va_end(ap2);
	return p;
}

static int in_set(const wchar_t *set, int c)
{
	int j;
	const wchar_t *p = set;
	if (*p == '-') {
		if (c=='-') return 1;
		p++;
	} else if (*p == ']') {
		if (c==']') return 1;
		p++;
	}
	for (; *p && *p != ']'; p++) {
		if (*p=='-' && p[1] && p[1] != ']')
			for (j=p++[-1]; j<*p; j++)
				if (c==j) return 1;
		if (c==*p) return 1;
	}
	return 0;
}

struct edg_wscan_numeric {struct edg_wscan_input *input;int last;};
static int edg_wscan_ascii(void *context)
{
 struct edg_wscan_numeric *source=(struct edg_wscan_numeric*)context;
 source->last=edg_wget(source->input);
 return source->last<0?-1:source->last<128?source->last:'@';
}
static int edg_wscan_number(struct edg_wscan_input *f,int conversion,int size,int width,void *destination,long *count)
{
 struct edg_wscan_numeric source={f,-1};
 struct edg_scan_input input={0,0,0,0,0,edg_wscan_ascii,&source};
 struct edg_string_scan cursor;sh_frominput(&cursor,&input);shlim(&cursor,width);
 if(conversion=='d'||conversion=='i'||conversion=='o'||conversion=='u'||conversion=='x'||conversion=='X'||conversion=='p'){
  unsigned base=conversion=='i'?0:conversion=='o'?8:(conversion=='x'||conversion=='X'||conversion=='p')?16:10;
  unsigned long long value=__intscan(&cursor,base,0,ULLONG_MAX);
  if(shcnt(&cursor)){if(conversion=='p' && destination)*(void**)destination=(void*)(uintptr_t)value;else store_int(destination,size,value);}
 }else{
  long double value=__floatscan(&cursor,size,0);
  if(shcnt(&cursor)&&destination){if(size==SIZE_def)*(float*)destination=value;else if(size==SIZE_l)*(double*)destination=value;else if(size==SIZE_L)*(long double*)destination=value;}
 }
 *count=shcnt(&cursor);if(input.pending)edg_wunget(source.last,f);
 return *count!=0;
}

int __edg_musl_vwscan(struct edg_wscan_input *restrict f, const wchar_t *restrict fmt, va_list ap)
{
	int width;
	int size;
	int alloc;
	const wchar_t *p;
	int c, t;
	char *s;
	wchar_t *wcs;
	void *dest=NULL;
	int invert;
	int matches=0;
	long pos = 0, cnt;
	char tmp[3*sizeof(int)+10];
	const wchar_t *set;
	size_t i, k;


	for (p=fmt; *p; p++) {

		alloc = 0;

		if (iswspace(*p)) {
			while (iswspace(p[1])) p++;
			while (iswspace((c=edg_wget(f)))) pos++;
			edg_wunget(c, f);
			continue;
		}
		if (*p != '%' || p[1] == '%') {
			if (*p == '%') {
				p++;
				while (iswspace((c=edg_wget(f)))) pos++;
			} else {
				c = edg_wget(f);
			}
			if (c!=*p) {
				edg_wunget(c, f);
				if (c<0) goto input_fail;
				goto match_fail;
			}
			pos++;
			continue;
		}

		p++;
		if (*p=='*') {
			dest = 0; p++;
		} else if (iswdigit(*p) && p[1]=='$') {
			dest = arg_n(ap, *p-'0'); p+=2;
		} else {
			dest = va_arg(ap, void *);
		}

		for (width=0; iswdigit(*p); p++) {
			width = 10*width + (*p - '0');
		}

		if (*p=='m') {
			wcs = 0;
			s = 0;
			alloc = !!dest;
			p++;
		} else {
			alloc = 0;
		}

		size = SIZE_def;
		switch (*p++) {
		case 'h':
			if (*p == 'h') p++, size = SIZE_hh;
			else size = SIZE_h;
			break;
		case 'l':
			if (*p == 'l') p++, size = SIZE_ll;
			else size = SIZE_l;
			break;
		case 'j':
			size = SIZE_ll;
			break;
		case 'z':
		case 't':
			size = SIZE_l;
			break;
		case 'L':
			size = SIZE_L;
			break;
		case 'd': case 'i': case 'o': case 'u': case 'x':
		case 'a': case 'e': case 'f': case 'g':
		case 'A': case 'E': case 'F': case 'G': case 'X':
		case 's': case 'c': case '[':
		case 'S': case 'C':
		case 'p': case 'n':
			p--;
			break;
		default:
			goto fmt_fail;
		}

		t = *p;

		/* Transform S,C -> ls,lc */
		if ((t&0x2f)==3) {
			size = SIZE_l;
			t |= 32;
		}

		if (t != 'n') {
			if (t != '[' && (t|32) != 'c')
				while (iswspace((c=edg_wget(f)))) pos++;
			else
				c=edg_wget(f);
			if (c < 0) goto input_fail;
			edg_wunget(c, f);
		}

		switch (t) {
		case 'n':
			store_int(dest, size, pos);
			/* do not increment match count, etc! */
			continue;

		case 's':
		case 'c':
		case '[': {
			if (t == 'c') {
				if (width<1) width = 1;
				invert = 1;
				set = L"";
			} else if (t == 's') {
				invert = 1;
				static const wchar_t spaces[] = {
					' ', '\t', '\n', '\r', 11, 12,  0x0085,
					0x2000, 0x2001, 0x2002, 0x2003, 0x2004, 0x2005,
					0x2006, 0x2008, 0x2009, 0x200a,
					0x2028, 0x2029, 0x205f, 0x3000, 0 };
				set = spaces;
			} else {
				if (*++p == '^') p++, invert = 1;
				else invert = 0;
				set = p;
				if (*p==']') p++;
				while (*p!=']') {
					if (!*p) goto fmt_fail;
					p++;
				}
			}

			s = (size == SIZE_def) ? (char *)dest : 0;
			wcs = (size == SIZE_l) ? (wchar_t *)dest : 0;

			int gotmatch = 0;

			if (width < 1) width = -1;

			i = 0;
			if (alloc) {
				k = t=='c' ? width+1U : 31;
				/* C/UTF-8: reserve a complete scalar before the first wctomb. */
				if (size != SIZE_l && k < 4) k = 4;
				if (size == SIZE_l) {
					wcs = (wchar_t *)malloc(k*sizeof(wchar_t));
					if (!wcs) goto alloc_fail;
				} else {
					s = (char *)malloc(k);
					if (!s) goto alloc_fail;
				}
			}
			while (width) {
				if ((c=edg_wget(f))<0) break;
				if (in_set(set, c) == invert)
					break;
				if (wcs) {
					wcs[i++] = c;
					if (alloc && i==k) {
						k += k+1;
						wchar_t *tmp = (wchar_t *)realloc(wcs, k*sizeof(wchar_t));
						if (!tmp) goto alloc_fail;
						wcs = tmp;
					}
				} else if (size != SIZE_l) {
					int l = wctomb(s?s+i:tmp, c);
					if (l<0) goto input_fail;
					i += l;
					if (alloc && i > k-4) {
						k += k+1;
						char *tmp = (char *)realloc(s, k);
						if (!tmp) goto alloc_fail;
						s = tmp;
					}
				}
				pos++;
				width-=(width>0);
				gotmatch=1;
			}
			if (width) {
				edg_wunget(c, f);
				if (t == 'c' || !gotmatch) goto match_fail;
			}

			if (alloc) {
				if (size == SIZE_l) *(wchar_t **)dest = wcs;
				else *(char **)dest = s;
			}
			if (t != 'c') {
				if (wcs) wcs[i] = 0;
				if (s) s[i] = 0;
			}
			break;

		}
		case 'd': case 'i': case 'o': case 'u': case 'x':
		case 'a': case 'e': case 'f': case 'g':
		case 'A': case 'E': case 'F': case 'G': case 'X':
		case 'p':
			if (width < 1) width = 0;
			if (!edg_wscan_number(f,t,size,width,dest,&cnt)) goto match_fail;
			pos += cnt;
			break;
		default:
			goto fmt_fail;
		}

		if (dest) matches++;
	}
	if (0) {
fmt_fail:
alloc_fail:
input_fail:
		if (!matches) matches--;
match_fail:
		if (alloc) {
			free(s);
			free(wcs);
		}
	}
	return matches;
}


