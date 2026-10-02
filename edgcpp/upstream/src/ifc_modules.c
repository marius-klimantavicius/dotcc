/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

ifc_modules.c -- IFC reading & writing common code.

*/

/* Header files common to all files. */
#include "fe_common.h"

/* Additional header files. */
#include "ifc_modules.h"
#include "ifc_map_functions.h"
#include "ifc_modules_internal.h"
#include "pch.h"

#if !STANDALONE_UTILITY_PROGRAM

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

an_ifc_module_file::an_ifc_module_file(a_module_file_kind mk,
                     /* Defaulted: */  a_boolean          for_read_val)
/*
Construct a new IFC module file with the given module file kind.  for_read_val
should be TRUE if this IFC module file is being constructed for a read
operation.  for_read_val should be FALSE if this IFC module file is being
constructed for a write operation.
*/
  : module_kind(mk), for_read(for_read_val)
{
  if (this->for_read) {
    new (&this->read_state) an_ifc_module_file_read_state();
  } else {
    new (&this->write_state) an_ifc_module_file_write_state();
  }  /* if */
}  /* an_ifc_module_file::an_ifc_module_file */


an_ifc_module_file::an_ifc_module_file(an_ifc_module_file &&old)
/*
Move-construct from the given IFC module file.
*/
  : an_ifc_module_file(old.module_kind)
{
  this->version_major = old.version_major;
  this->version_minor = old.version_minor;
#if !ASSUME_LITTLE_ENDIAN_IFC_MODULES
  swap_at(&old.endianness, &this->endianness);
#endif /* !ASSUME_LITTLE_ENDIAN_IFC_MODULES */
  swap_at(&old.f_module, &this->f_module);
  if (old.for_read) {
    this->for_read = TRUE;
    construct(&this->read_state);
    swap_at(&old.read_state, &this->read_state);
  } else {
    this->for_read = FALSE;
    construct(&this->write_state);
    swap_at(&old.write_state, &this->write_state);
  }  /* if */
}  /* an_ifc_module_file::an_ifc_module_file */


an_ifc_module_file::~an_ifc_module_file()
/*
Destroy the IFC module file.
*/
{
  this->close();
  if (this->for_read) {
    this->read_state.~an_ifc_module_file_read_state();
  } else {
    this->write_state.~an_ifc_module_file_write_state();
  }  /* if */
}  /* an_ifc_module_file::~an_ifc_module_file */


an_ifc_module_file &an_ifc_module_file::operator=(an_ifc_module_file &&old)
/*
Move from the given IFC module file, returning self.
*/
{
  if (this != &old) {
    destroy(this);
    construct(this, move_from(&old));
  }  /* if */
  return *this;
}  /* an_ifc_module_file::operator= */


void an_ifc_module_file::close()
/*
Close the module file.
*/
{
  if (this->f_module != NULL) {
    if (this->for_read) {
      (void)fclose(this->f_module);
    } else {
      /* FIXME: Eventually we should figure out the correct error code from
         an_ifc_module_file::module_kind. */
      (void)close_output_file_with_error_handling(&this->f_module,
                                                  ec_edg_ifc_file);
    }  /* if */
    this->f_module = NULL;
#if USE_MMAP_FOR_MEMORY_REGIONS
    if (this->for_read) {
      an_ifc_module_file_read_state &rs_ref = this->read_state;

      unmap_memory(rs_ref.mmap_addr, rs_ref.mmap_size);
      rs_ref.mmap_addr = NULL;
      rs_ref.mmap_size = 0;
      close_mapped_input_file(rs_ref.mapped_file);
    }  /* if */
#endif /* USE_MMAP_FOR_MEMORY_REGIONS */
  }  /* if */
}  /* an_ifc_module_file::close */


a_boolean is_for_read(an_ifc_module_file *file)
/*
Return TRUE if the given module file is for a module read operation.  Return
FALSE if the given module file is for a module write operation.

This function provides a definition for the declaration in ifc_map.h.  This
allows ifc_map.h to function without a complete definition of
an_ifc_module_file.
*/
{
  return file->is_for_read();
}  /* is_for_read */


a_boolean is_at_least(an_ifc_module_file     *file,
                      an_ifc_version_storage minimum_version_major,
                      an_ifc_version_storage minimum_version_minor)
/*
Check to see if the given module's version has at least the minimum version
"major.minor".
*/
{
  a_boolean result = FALSE;

  if (file->version_major > minimum_version_major) {
    result = TRUE;
  } else if (file->version_major == minimum_version_major &&
             file->version_minor >= minimum_version_minor) {
    result = TRUE;
  } else if (skip_module_version_check) {
    /* The version check is being skipped.  Check to see if this is a query for
       the minimum supported IFC version or something before that.

       Local variables are used here to suppress spurious diagnostics about
       pointless comparisons against 0 if IFC_MIN_VER_MAJOR or
       IFC_MIN_VER_MINOR is 0.  These local variables while "constant" are not
       sufficiently constant for front ends to analyze; the optimizer is
       unaffected. */
    an_ifc_version_storage min_supported_major = IFC_MIN_VER_MAJOR;
    an_ifc_version_storage min_supported_minor = IFC_MIN_VER_MINOR;

    if (minimum_version_major < min_supported_major) {
      result = TRUE;
    } else if (minimum_version_major == min_supported_major &&
               minimum_version_minor <= min_supported_minor) {
      result = TRUE;
    }  /* if */
  }  /* if */
  return result;
}  /* is_at_least */


a_const_char* ifc_token_name_of(a_token_kind token_kind)
/*
Return a unique name for the given token for identification purposes in an EDG
IFC token cache.

This function primarily uses the EDG token_names, but replaces some token names
to allow every token to have a unique name.
*/
{
  a_const_char *result;

  switch (token_kind) {
#if MICROSOFT_EXTENSIONS_ALLOWED
    case tok_prefix_for:
      result = "for[[prefix]]";
      break;
    case tok_prefix_enum:
      result = "enum[[prefix]]";
      break;
#endif /* MICROSOFT_EXTENSIONS_ALLOWED */
    case tok_cpp98_export:
      result = "export[[C++98]]";
      break;
    case tok_export_keyword:
      result = "export[[keyword]]";
      break;
    default:
      result = token_names[token_kind];
      break;
  }  /* switch */
  return result;
}  /* ifc_token_name_of */


a_string_view get_partition_name_from_kind(an_ifc_partition_kind part_kind)
/*
Given a partition kind that corresponds to a real partition, return the
corresponding name.
*/
{
  /* Subtract 1 to ignore pk_none. */
  static_assert(ifc_pk_none == 0,
                "pk_none does not hold the expected value");
  check_assertion(part_kind != ifc_pk_none);

  an_ifc_partition_map *map_entry = &ifc_partition_map[part_kind - 1];
  /* Make sure the right map entry is going to be returned. */
  check_assertion(map_entry->kind == part_kind);
  return a_string_view(map_entry->name);
}  /* get_partition_name_from_kind */

#if !ASSUME_LITTLE_ENDIAN_IFC_MODULES

a_boolean has_matching_endianness(an_ifc_module_file *file)
/*
Check to see if the given module file's endianness matches the endianness the
front end was compiled under.
*/
{
  a_boolean result;

  switch (file->endianness) {
    case ifc_mpe_little:
      result = host_little_endian;
      break;
    case ifc_mpe_big:
      result = !host_little_endian;
      break;
    case ifc_mpe_unknown:
      /* Make a best guess based on the compiler's target. */
      result = targ_little_endian == host_little_endian;
      break;
    default_is_unexpected();
  }  /* switch */
  return result;
}  /* has_matching_endianness */

#endif /* !ASSUME_LITTLE_ENDIAN_IFC_MODULES */

STATIC_THREAD a_boolean
        ifc_modules_initialized_for_curr_tu;
                /* TRUE if IFC modules-related variables have been fully
                   initialized for the current translation unit. */


void ifc_modules_one_time_init()
/*
Do one-time initialization of static variables defined in this file.
*/
{
  /* Save variables from ifc_modules.h and ifc_modules.c that are needed for
     precompiled headers */
  if (precompiled_header_processing_required) {
    STATIC_THREAD a_pch_saved_variable saved_vars[] = {
      pch_saved_var_array_elem(ifc_modules_initialized_for_curr_tu),
      pch_saved_var_array_terminating_elem()
    };
    register_pch_saved_variables(saved_vars);
  }  /* if */
  /* Register variables that have distinct copies for distinct translation
     units. */
  register_trans_unit_variable(ifc_modules_initialized_for_curr_tu);
  ifc_modules_read_one_time_init();
  ifc_modules_write_one_time_init();
}  /* ifc_modules_one_time_init */


void require_ifc_modules()
/*
IFC module use has been detected in the current translation unit.  Initialize
the corresponding front end structures (if not already initialized).
*/
{
  if (!ifc_modules_initialized_for_curr_tu) {
    ifc_modules_read_trans_unit_delayed_init();
    ifc_modules_initialized_for_curr_tu = TRUE;
  }  /* if */
}  /* require_ifc_modules */


void ifc_modules_trans_unit_init()
/*
Initialize the variables necessary for using IFC modules in the current
translation unit.
*/
{
  ifc_modules_initialized_for_curr_tu = FALSE;
  ifc_modules_read_trans_unit_init();
}  /* ifc_modules_trans_unit_init */


void ifc_modules_trans_unit_wrapup()
/*
Perform any wrapup operations needed for the translation unit.  This is
called after all processing for the translation unit (including template
instantiations, etc.) has been done.
*/
{
  ifc_modules_read_trans_unit_wrapup();
}  /* ifc_modules_trans_unit_wrapup */

#if MAKE_FRONT_END_CALLABLE

void ifc_modules_cleanup()
/*
This routine is called at the end of compilation, or if compilation is
terminated prematurely for some reason.
*/
{
  ifc_modules_read_cleanup();
  ifc_modules_write_cleanup();
}  /* ifc_modules_cleanup */

#endif /* MAKE_FRONT_END_CALLABLE */

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* !STANDALONE_UTILITY_PROGRAM */

