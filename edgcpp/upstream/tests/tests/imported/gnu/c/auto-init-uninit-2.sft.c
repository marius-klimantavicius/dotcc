//type: fp
//options: 
# 0 "./auto-init-uninit-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./auto-init-uninit-2.c"




# 1 "./uninit-2.c" 1





struct definition
{
  int nargs;
  int rest_args;
};

struct cpp_reader;

enum cpp_token
{
  CPP_EOF, CPP_POP, CPP_COMMA, CPP_RPAREN
};

extern enum cpp_token macarg (struct cpp_reader *, int);

void
macroexpand (struct cpp_reader *pfile, struct definition *defn)
{
  int nargs = defn->nargs;

  if (nargs >= 0)
    {
      enum cpp_token token;
      int i, rest_args;
      i = 0;
      rest_args = 0;
      do
 {
   if (rest_args)
     continue;
   if (i < nargs || (nargs == 0 && i == 0))
     {

       if (i == nargs - 1 && defn->rest_args)
  rest_args = 1;
       token = macarg (pfile, rest_args);
     }
   else
     token = macarg (pfile, 0);
   if (token == CPP_EOF || token == CPP_POP)
     return;

   i++;
 }
      while (token == CPP_COMMA);
    }
}
# 6 "./auto-init-uninit-2.c" 2
