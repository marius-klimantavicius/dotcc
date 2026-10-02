/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*
It must be possible to include this file more than once, so it intentionally
does not have an include guard.
*/

/*

target_cfg.h -- Generate target-specific routines

*/

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

/*
Define a set_target_config_* function to initialize target-specific global
variables to a particular set of target-specific values.
*/
/* Routine name: set_target_config_X (where X is the configuration name). */
#define TARGET_MAP_ROUTINE_NAME(config) \
  EDG_CONCAT(set_target_config ## _, config)(void)
/* Assign the target-specific macro value to the associated global variable. */
/*lint -estring(823,TARGET_MAP_MACRO)*/
/*lint -emacro(506,TARGET_MAP_MACRO)*/
#define TARGET_MAP_MACRO(config_macro, global_var, config) \
  (global_var) = EDG_CONCAT(config_macro ## _, config);
END_EDG_NAMESPACE  /* Conditionally close the "edg" namespace. */
#include "target_map.h"
BEGIN_EDG_NAMESPACE  /* Conditionally open the "edg" namespace. */

#if DUMP_CONFIG_ENABLED

/*
Define a dump_target_config_* function to dump the values of target-specific
configuration macros.
*/
/* Routine name: dump_target_config_X (where X is the configuration name). */
#define TARGET_MAP_ROUTINE_NAME(config) \
  EDG_CONCAT(dump_target_config ## _, config)(void)
#define STRINGIZE_HELPER(X) stringize(X)
/* Write the target-specific macro and its value to stderr in #define format.*/
/*lint -estring(823,TARGET_MAP_MACRO)*/
#define TARGET_MAP_MACRO(config_macro, global_var, config) \
  fprintf(f_error, "#define %s %s\n", \
          #config_macro "_" stringize(config), \
          STRINGIZE_HELPER(EDG_CONCAT(config_macro ## _, config)));
END_EDG_NAMESPACE  /* Conditionally close the "edg" namespace. */
#include "target_map.h"
BEGIN_EDG_NAMESPACE  /* Conditionally open the "edg" namespace. */
#undef STRINGIZE_HELPER

#endif /* DUMP_CONFIG_ENABLED */

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#undef TARGET_CONFIGURATION


