/*
  print-gcc-builtins: a GCC plugin to print builtin declarations
  Copyright (C) 2016-2023 Edison Design Group, Inc.

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

#include <stdlib.h>
#include <gmp.h>
#include <cstdlib>

/*
The GCC architecture has changed somewhat between 4.5.0 (when plugins were
first introduced) and the latest release (6.1.0 at present); the conditional
code below enables this plugin to run in all of those environments.
*/

#if __GNUC__ >= 5 || (__GNUC__ == 4 &&  __GNUC_MINOR__ >= 9)

#include "gcc-plugin.h"
#include "cp/cp-tree.h"
#include "print-tree.h"
#include "langhooks.h"

#else

#if __GNUC_MINOR__ < 7
/* Prior to 4.7.0, need extern "C" block. */
extern "C"
{
#endif

#include "gcc-plugin.h"
#include "tree.h"
#include "tm.h"
#include "cp/cp-tree.h"   /* Note that in 4.6.x, this file fails to find
                             "c-family/c-common.h" in the plugin include
                             directory (it's one level up), so a symbolic
                             link was added to enable this to work. */
#include "langhooks.h"

#if __GNUC_MINOR__ < 7
}
#endif
#endif

int plugin_is_GPL_compatible;

extern "C" void
gate_callback (void*, void*)
{
  /* If there were errors during compilation, let GCC handle the exit. */
  if (errorcount || sorrycount)
    return;

  for ( tree node = lang_hooks.decls.getdecls();
        node != NULL;
        node = TREE_CHAIN (node)) {
    if (TREE_CODE (node) == FUNCTION_DECL) {
#if 0
      debug_tree(node);  // Prints the tree to stderr.
#endif
#if 1
      /* decl_as_string is available only in C++ mode. */
      if (
#ifdef DECL_IS_UNDECLARED_BUILTIN
          DECL_IS_UNDECLARED_BUILTIN (node)
#else /* !defined(DECL_IS_UNDECLARED_BUILTIN) */
          DECL_IS_BUILTIN (node)
#endif /* defined(DECL_IS_UNDECLARED_BUILTIN) */
          ) {
        printf("/* %s */ %s", get_name(node), decl_as_string(node,
                                                 TFF_DECL_SPECIFIERS |
                                                 TFF_RETURN_TYPE |
                                                 TFF_CHASE_TYPEDEF |
                                                 TFF_EXCEPTION_SPECIFICATION));
      } else {
        printf("/* %s:%s */ %s", DECL_SOURCE_FILE(node), get_name(node),
                                 decl_as_string(node,
                                                 TFF_DECL_SPECIFIERS |
                                                 TFF_RETURN_TYPE |
                                                 TFF_CHASE_TYPEDEF |
                                                 TFF_EXCEPTION_SPECIFICATION));
      }
      // Emit a noreturn attribute if appropriate:
      if (TREE_THIS_VOLATILE(node)) {
        printf(" __attribute__((noreturn))");
      }
      printf(";\n");
#else
      /* print_decl is available in C mode, but isn't as flexible as
         decl_as_string.  See https://gcc.gnu.org/ml/gcc/2011-11/msg00504.html.
         Also, don't seem to be able to get builtin declarations in C mode. */
      printf("/* %s */", get_name(node));
      lang_hooks.print_decl(stdout, node, 0);
#endif
    }
  }

  /* Nothing else to do. */
  exit (0);
}

extern "C" int
plugin_init (plugin_name_args* info,
             plugin_gcc_version* ver)
{
  /* Register the callback. */
  register_callback (info->base_name, PLUGIN_OVERRIDE_GATE, &gate_callback, 0);
  return 0;
}
