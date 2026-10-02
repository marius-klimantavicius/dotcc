/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Declarations for EDG template prelink utility.

*/


/* Type code output by "nm" for externally visible function definitions. */
#define EXTERN_TYPE 'T'

/* Suffix to be used for the instantiation request file. */
#define INSTANTIATION_REQUEST_SUFFIX ".ii"

/* Suffix to be used for the instantiation request file. */
#define TEMPLATE_INFO_SUFFIX ".ti"

/* Maximum length of a suffix. */
#define MAX_SUFFIX_LEN 10

/* Suffix to be used for instantiation object files created in one
   instantiation per object mode. */
#if BACK_END_IS_C_GEN_BE || BACK_END_IS_CP_GEN_BE
/* When the C or C++ generating back end is used, the suffix will
   include ".int" and the object file suffix. */
#if __MICROSOFT_OS__
#define INSTANTIATION_OBJECT_SUFFIX ".int.obj"
#else /* !__MICROSOFT_OS__ */
#define INSTANTIATION_OBJECT_SUFFIX ".int.o"
#endif /* __MICROSOFT_OS__ */
#else /* !(BACK_END_IS_C_GEN_BE || BACK_END_IS_CP_GEN_BE) */
/* When a "real" back end is used, the instantiation suffix is just the
   object file suffix. */
#if __MICROSOFT_OS__
#define INSTANTIATION_OBJECT_SUFFIX ".obj"
#else /* !__MICROSOFT_OS__ */
#define INSTANTIATION_OBJECT_SUFFIX ".o"
#endif /* __MICROSOFT_OS__ */
#endif /* BACK_END_IS_C_GEN_BE || BACK_END_IS_CP_GEN_BE */

/* Special mangled name prefixes used by the prelinker. */
#define PL_CAN_BE_INSTANTIATED_PREFIX		"__CBI__"
#define PL_CAN_BE_INSTANTIATED_PREFIX_LEN	7
#define PL_DO_NOT_INSTANTIATE_PREFIX		"__DNI__"
#define PL_DO_NOT_INSTANTIATE_PREFIX_LEN	7
#define PL_INSTANCE_REQUIRED_PREFIX		"__TIR__"
#define PL_INSTANCE_REQUIRED_PREFIX_LEN		7

/* The maximum number of iterations after which we give up under the
   assumption that we've encountered an instantiation loop. */
#ifndef PL_MAX_ITERATIONS
#define PL_MAX_ITERATIONS	300
#endif /* ifndef PL_MAX_ITERATIONS */

/* Indicates that the object file should be removed when the .ii file
   is updated.  This can be useful if the compilation is terminated
   before the new object file has been written since it will prevent the
   .ii file and .o file from getting out of sync. */
#ifndef PL_REMOVE_OBJECT_FILE_BEFORE_RECOMPILATION
#define PL_REMOVE_OBJECT_FILE_BEFORE_RECOMPILATION TRUE
#endif /* ifndef PL_REMOVE_OBJECT_FILE_BEFORE_RECOMPILATION */

/* Indicates whether the prelinker should produce verbose output by
   default. */
#ifndef PL_DEFAULT_VERBOSE_MODE
#define PL_DEFAULT_VERBOSE_MODE TRUE
#endif /* ifndef PL_DEFAULT_VERBOSE_MODE */

/* Indicates whether the prelinker should check for a specialization with
   the same name as a generated instance by default.  The mechanism
   used to check this has proven to be unreliable, so use of this
   feature is no longer recommended. */
#ifndef PL_DEFAULT_CHECK_SPECIALIZATION_ERRORS
#define PL_DEFAULT_CHECK_SPECIALIZATION_ERRORS FALSE
#endif /* ifndef PL_DEFAULT_CHECK_SPECIALIZATION_ERRORS */

/* Indicates whether the prelinker should, by default, create a definition
   list file when invoking the front end.  The definition list file contains
   a list of all of the entities defined in the objects and libraries with
   which a given file is linked.  It permits the front end to determine
   whether or not a new instantiation that is referenced can be
   instantiated. */
#ifndef PL_DEFAULT_USE_DEFINITION_LIST
#define PL_DEFAULT_USE_DEFINITION_LIST TRUE
#endif /* ifndef PL_DEFAULT_USE_DEFINITION_LIST */


/* Command to be used to produce a namelist of an object file. */
static char		default_nm_command[] = "/bin/nm -og";
static char		gnu_nm_command[] = "nm -og --no-cplus";
static char		solaris_nm_command[] = "nm -pxR";
static char		SGI_nm_command[] = "/bin/nm -Bop";
static char		CLIX_nm_command[] = "/bin/nm -pxre";
static char		alternate_nm_command[] = "/bin/nm -pxr";
static char		nm_command_suffix[] = "";

static char		*pl_predefined_names[] = {
#ifdef sparc
				"etext",
				"edata",
				"end",
				"_DYNAMIC",
				"_GLOBAL_OFFSET_TABLE",
#endif /* sparc */
				NULL
};


