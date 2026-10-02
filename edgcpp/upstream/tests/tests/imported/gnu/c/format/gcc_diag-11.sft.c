//type: fp
//options: 
# 0 "./format/gcc_diag-11.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./format/gcc_diag-11.c"







typedef long long __gcc_host_wide_int__;

typedef struct location_s
{
  const char *file;
  int line;
} location_t;

union tree_node;
typedef union tree_node *tree;



typedef struct gimple gimple;


typedef struct cgraph_node cgraph_node;





void diag_raw (const char*, ...) __attribute__ ((format (__gcc_diag_raw__, 1, 2)));
void cdiag_raw (const char*, ...) __attribute__ ((format (__gcc_cdiag_raw__, 1, 2)));
void tdiag_raw (const char*, ...) __attribute__ ((format (gcc_tdiag_raw, 1, 2)));
void cxxdiag_raw (const char*, ...) __attribute__ ((format (gcc_cxxdiag_raw, 1, 2)));


void diag (const char*, ...) __attribute__ ((format (__gcc_diag__, 1, 2)));



void cdiag (const char*, ...) __attribute__ ((format (__gcc_cdiag__, 1, 2)));



void tdiag (const char*, ...) __attribute__ ((format (__gcc_tdiag__, 1, 2)));



void cxxdiag (const char*, ...) __attribute__ ((format (__gcc_cxxdiag__, 1, 2)));





void test_diag_raw (tree t, gimple *gc)
{
  diag_raw ("a  b");
  diag_raw ("newline\n");
  diag_raw ("lone period.");
  diag_raw ("multiple punctuators: !!!");
  diag_raw ("unbalanced paren (");
  diag_raw ("keyword alignas and identifier_with_underscores");
  diag_raw ("disable __builtin_abs with the -fno-builtin-abs option");
  diag_raw ("who says I can't have no stinkin' contractions? ");

  cdiag_raw ("__atomic_sync (%qE) == 7???", t);
  tdiag_raw ("__builtin_abs (%E) < 0!?!", t);
  cxxdiag_raw ("template <> int f (%E", t);
}




void test_cdiag_whitespace (tree t, gimple *gc)
{
  (void)&t; (void)&gc;


  cdiag (" a");
  cdiag ("  b");
  cdiag ("   c");
  cdiag ("%< %>a");
  cdiag ("%<  %>a");
  cdiag ("a b");
  cdiag ("a  b");
  cdiag ("a ");
  cdiag ("a  ");
  cdiag ("a%< %>");
  cdiag ("a%< %>%< %>");
  cdiag ("a%< %> ");
  cdiag ("a%< %>  %< %>");



  cdiag ("a %< %>");
  cdiag ("a%< %> %< %>");


  cdiag ("a\fb");
  cdiag ("a\nb");
  cdiag ("a\rb");
  cdiag ("a\vb");

  cdiag ("First sentence.  And a next.");
  cdiag ("First sentence.  not capitalized sentence");

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wformat-diag"


  cdiag ("\ta\b    c\vb\n");

#pragma GCC diagnostic pop
}


void test_cdiag_control (tree t, gimple *gc)
{
  (void)&t; (void)&gc;

  cdiag ("\1");
  cdiag ("a\ab");
  cdiag ("a\bb");
}


void test_cdiag_punct (tree t, gimple *gc, int i)
{
  (void)&t; (void)&gc;


  cdiag (".abc");
  cdiag ("abc;");


  cdiag ("This is a full sentence.");
  cdiag ("Capitalized sentence (with a parethetical note).");
  cdiag ("Not a full sentence;");
  cdiag ("Neither is this one,");


  cdiag ("this message...");
  cdiag ("...continues here");
  cdiag ("but...not here");



  cdiag ("null argument where non-null required (argument %i)", i);
  cdiag ("null (argument %i) where non-null required", i);
  cdiag ("(see what comes next)");


  cdiag ("candidates are:");
  cdiag ("candidates are::");


  cdiag ("C++ is cool");
  cdiag ("this is c++");
  cdiag ("you can do this in C++ but not in C");


  cdiag ("G++ rocks");
  cdiag ("this is accepted by g++");
  cdiag ("valid in G++ (or g++) but not in gcc");



  cdiag ("found a bug (here):");
  cdiag ("because of another bug (over there); fix it");

  cdiag ("found foo (123): go look at it");
  cdiag ("missed bar (abc); will try harder next time");

  cdiag ("expected this (or that), got something else (or who knows what)");


  cdiag ("hmmm (did you really mean that?)");
  cdiag ("error (did you mean %<foo()%>?)");

  cdiag ("did you mean this (or that)?");


  cdiag ("or this or the other)?");

  cdiag ("## Heading");
  cdiag ("## %s ##", "1");

  cdiag ("#1 priority");
  cdiag ("priority #2");


  cdiag ("\"quoted\"");
  cdiag ("\"quoted\" string");
  cdiag ("this is a \"string in quotes\"");
  cdiag ("\"missing closing quote");


  cdiag ("bad version : 1");
  cdiag ("problem ; fix it");
  cdiag ("End . not.");
  cdiag ("it is bad , very bad");
  cdiag ("say what ?");


  cdiag ("1 / 2");
  cdiag ("2 + 3");
  cdiag ("2 - 3");
}

void test_cdiag_punct_balance (tree t, gimple *gc)
{
  (void)&t; (void)&gc;


  cdiag ("a < b");
  cdiag ("must be > 0");

  cdiag ("f()");
  cdiag ("g(1)");
  cdiag ("(");
  cdiag ("()");
  cdiag (")");
  cdiag ("f()g");
  cdiag ("illegal operand (1)");
}


void test_cdiag_nongraph (tree t, gimple *gc)
{
  (void)&t; (void)&gc;

  cdiag ("a\376b");
  cdiag ("a\377b");
}


void test_cdiag_attribute (tree t, gimple *gc)
{
  (void)&t; (void)&gc;

  cdiag ("attribute foo");
  cdiag ("this is attribute bar");
  cdiag ("bad __attribute bar");
  cdiag ("__attribute__ (foobar) bad");
  cdiag ("__attribute__ ((foobar))");
  cdiag ("__attribute__ (xxx))");

  cdiag ("__attribute__ ((yyy)))");

  cdiag ("__attribute__ ((zzz)");


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wformat-diag"


  cdiag ("__attribute__ (((");

#pragma GCC diagnostic pop
}

void test_cdiag_builtin (tree t, gimple *gc)
{
  (void)&t; (void)&gc;

  cdiag ("__builtin_abort");
  cdiag ("in __builtin_trap");
  cdiag ("__builtin_xyz bites");

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wformat-diag"


  cdiag ("__builtin____with____lots__of__underscores");

#pragma GCC diagnostic pop
}


void test_cdiag_option (tree t, gimple *gc)
{
  (void)&t; (void)&gc;

  cdiag ("%<-Wall%>");
  cdiag ("use option %<-Wextra%> to enable additinal warnings");

  cdiag ("-O2 is fast");
  cdiag ("but -O3 is faster");

  cdiag ("get --help");
  cdiag ("enable -m32");
  cdiag ("value is -12");
  cdiag ("foo-O2");
  cdiag ("a-W");
}


void test_cdiag_keyword (tree t, gimple *gc)
{
  cdiag ("alignasi");
  cdiag ("malignofer or alignofus");
  cdiag ("use alignof");
  cdiag ("or _Alignof");
  cdiag ("_Pragma too");

  cdiag ("a #error directive");
  cdiag ("#include file");
  cdiag ("but #pragma foobar");
  cdiag ("pragma foobar is okay");
  cdiag ("or even # pragma is fine");


  cdiag ("const function");
  cdiag ("const-qualified variable");

  cdiag ("a const %qD", t);
  cdiag ("restrict %qE", t);
  cdiag ("volatile %qT", t);
  cdiag ("const %qD and restrict %qE or volatile %qT", t, t, t);




  cdiag ("an offsetof here");
  cdiag ("sizeof x");
  cdiag ("have typeof");


  cdiag ("break rules");
  cdiag ("if we continue by default for a short while else do nothing");
  cdiag ("register a function for unsigned extern to void const reads");
  cdiag ("or volatile access");
}


void test_cdiag_operator (tree t, gimple *gc)
{
  cdiag ("x != 0");
  cdiag ("logical &&");
  cdiag ("+= operator");
  cdiag ("a == b");
  cdiag ("++a");
  cdiag ("b--");
  cdiag ("1 << 2");
  cdiag (">> here <<");
}


void test_cdiag_type_name (tree t, gimple *gc)
{
  cdiag ("the word character should not be quoted");
  cdiag ("but char should be");

  cdiag ("unsigned char should be quoted");
  cdiag ("but unsigned character is fine");

  cdiag ("as should int");
  cdiag ("and signed int");
  cdiag ("and also unsigned int");
  cdiag ("very long thing");
  cdiag ("use long long here");

  cdiag ("have a floating type");
  cdiag ("found float type");

  cdiag ("wchar_t is wide");
}


void test_cdiag_identifier (tree t, gimple *gc)
{
  (void)&t; (void)&gc;

  cdiag ("private _x ident");
  cdiag ("and another __y");
  cdiag ("ident z_ with trailing underscore");
  cdiag ("v_ variable");
  cdiag ("call foo_bar");
  cdiag ("unquoted x_y ident");

  cdiag ("size_t type");
  cdiag ("bigger than INT_MAX");

  cdiag ("quoted ident %<a_b%>");
  cdiag ("another quoted identifier %<x_%> here");
}


void test_cdiag_bad_words (tree t, gimple *gc)
{
  (void)&t; (void)&gc;

  cdiag ("aren't you dumb?");
  cdiag ("bitfields suck");
  cdiag ("invalid bitfield");
  cdiag ("bad builtin function");
  cdiag ("bad builtin function");
  cdiag ("builtin function x");
  cdiag ("builtin functions disabled");
  cdiag ("enable builtin functions");
  cdiag ("you can't do that");
  cdiag ("you can%'t do that");
  cdiag ("Can%'t touch this.");
  cdiag ("can%'");
  cdiag ("can%' whatever");
  cdiag ("on the commandline");
  cdiag ("command line option");
  cdiag ("it mustn't be");
  cdiag ("isn't that silly?");

  cdiag ("can not do this");
  cdiag ("you can not");


  cdiag ("Mising arg.");
  cdiag ("2 args: a and b");
  cdiag ("arg 1");
  cdiag ("Args are wrong.");
  cdiag ("bad arg");
  cdiag ("two args");
  cdiag ("args 1 and 2");

  cdiag ("Reg A");
  cdiag ("regs A and B");
  cdiag ("no regs");



  cdiag ("gulmarg and balfarg");
  cdiag ("ademargs or toshargs");
  cdiag ("talk to Greg");
  cdiag ("prepreg is a fabric");
  cdiag ("there are dregs in my wine");
}


void test_cdiag_directive (tree t, gimple *gc)
{
  (void)&t; (void)&gc;

  cxxdiag ("%<%s%>", "");



  cdiag ("\"%s\"", "");


  cdiag ("%<'%>");
  cdiag ("%<\"%>");
  cdiag ("%<<%>");
  cdiag ("%<>%>");
  cdiag ("%<(%>");
  cdiag ("%<)%>");
  cdiag ("%<[%>");
  cdiag ("%<]%>");

  cdiag ("%<'%> %<\"%> %<>%> %<<%> %<)%> %<(%> %<]%> %<[%>");
}
