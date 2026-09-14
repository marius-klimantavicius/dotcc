#ifndef _SETJMP_H
#define _SETJMP_H

/* dotcc's <setjmp.h> — C99 7.13. Implemented via .NET exceptions:
   `longjmp(env, value)` throws a tagged exception and the emitter
   recognises specific setjmp shapes at the use site to wrap the
   surrounding code in a `try / catch when` block.

   Supported syntactic patterns (emitter rewrites these; other shapes
   raise CompileException):

     -- zero-vs-nonzero `if` guard (try/catch):
     if (setjmp(env))         { recovery } else { normal }
     if (setjmp(env) == 0)    { normal }   else { recovery }

     -- VALUE-CAPTURE (goto-restart: the region re-runs with r holding
        the longjmp value, so `switch`/`if` on r reaches the right arm —
        real C's "setjmp returns twice"):
     switch (setjmp(env)) { case 0: ...; case N: ...; }
     int r = setjmp(env);  switch (r) { ... }   (or: if (r == N) ...)
     r = setjmp(env);      (r = a pre-declared simple variable)

   Bonus over real C: `finally` blocks DO run during the unwind
   (.NET exception semantics) — strictly better than real longjmp's
   silent-skip-through-cleanup behaviour.

   NOT supported (rejected loudly, never lowered to the always-0 stub):
   setjmp in a loop/ternary condition, a nested sub-expression
   (`x = setjmp(env) + 1`), or a bare discarded call. */

/* An opaque numeric identity slot, not a CLR object reference. The array form
   preserves C parameter decay and can safely live inside malloc'd aggregates.
   This is the dotcc C ABI, not the native host's register-save record. */
#include <stdint.h>
typedef uint64_t jmp_buf[1];

int setjmp(jmp_buf env);
void longjmp(jmp_buf env, int value);

#endif
