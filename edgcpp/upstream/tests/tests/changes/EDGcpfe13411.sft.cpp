//type:fn
//remark:[4.6] Infinite loop when redeclaring an extern "C" function as static
// 11/20/12 [EDGcpfe/13411]
//
// Infinite loop when redeclaring an extern "C" function as static
//
// In some modes, the front end allows a function to first be declared with
// extern "C" linkage, and later defined with internal linkage (i.e., with the
// "static" specifier).  When this occurred in a namespace scope, the front end
// was likely to end up in an infinite loop attempting to move the routine entry
// in function perform_scheduled_routine_moves (il.c).
//
// This is now fixed.
extern "C" namespace N { void f(), g(); }
namespace N {
  static void f() {}  // Previously caused an infinite loop.  The 4.6 change
}                     // made this compile; later versions diagnose a
                      // linkage conflict.
