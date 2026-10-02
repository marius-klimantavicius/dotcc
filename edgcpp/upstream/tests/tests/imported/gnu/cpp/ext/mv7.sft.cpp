//type: fn
//options: 
//options_all: --gnu_version=80200 -tused -e 200 --no_wrap
// { dg-do compile { target i?86-*-* x86_64-*-* } }
// { dg-options "" }

__attribute__((target ("default")))
void foo (void)	// { dg-message "previously defined here" }
{
}

__attribute__((target (128)))
void foo (void) // { dg-error "(not a string|redefinition)" }
{
}
