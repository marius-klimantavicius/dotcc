/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

ifc_modules_spec.h -- Explicit template specializations needed at link time by
                      ifc_modules.c and ifc_map_functions.c.

** NOTICE: This file is produced by an external script. **

While EDG staff should update the generation script rather than manually
editing this file, customers are welcome to modify this file and create patches
as they see fit.

Please contact EDG Support if you would be interested in using, or learning
more about, the tool that generated this file.
*/

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

/* Disable spurious GCC warnings in generated code. */
BEGIN_DISABLE_GCC_WARNING_MAYBE_UNITIALIZED


/*
Explicit specializations of functions for EdgConstantIntegerWordOffset.
*/
SPEC_OFFSET_PARTITION_KIND(an_ifc_edg_constant_integer_word_offset)
SPEC_OFFSET_PARTITION_INDEX(an_ifc_edg_constant_integer_word_offset)


/*
Explicit specializations of functions for EdgExtensionExprOffset.
*/
SPEC_OFFSET_PARTITION_KIND(an_ifc_edg_extension_expr_offset)
SPEC_OFFSET_PARTITION_INDEX(an_ifc_edg_extension_expr_offset)


/*
Explicit specializations of functions for EdgExtensionTypeOffset.
*/
SPEC_OFFSET_PARTITION_KIND(an_ifc_edg_extension_type_offset)
SPEC_OFFSET_PARTITION_INDEX(an_ifc_edg_extension_type_offset)


/*
Explicit specializations of functions for EdgHeapComplexTokenOffset.
*/
SPEC_OFFSET_PARTITION_KIND(an_ifc_edg_heap_complex_token_offset)
SPEC_OFFSET_PARTITION_INDEX(an_ifc_edg_heap_complex_token_offset)


/*
Explicit specializations of functions for EdgHeapTemplateArgumentOffset.
*/
SPEC_OFFSET_PARTITION_KIND(an_ifc_edg_heap_template_argument_offset)
SPEC_OFFSET_PARTITION_INDEX(an_ifc_edg_heap_template_argument_offset)


/*
Explicit specializations of functions for EdgTokenBasicOffset.
*/
SPEC_OFFSET_PARTITION_KIND(an_ifc_edg_token_basic_offset)
SPEC_OFFSET_PARTITION_INDEX(an_ifc_edg_token_basic_offset)


/*
Explicit specializations of functions for EdgTokenCacheOffset.
*/
SPEC_OFFSET_PARTITION_KIND(an_ifc_edg_token_cache_offset)
SPEC_OFFSET_PARTITION_INDEX(an_ifc_edg_token_cache_offset)


/*
Explicit specializations of functions for ExprNamedDeclOffset.
*/
SPEC_OFFSET_PARTITION_KIND(an_ifc_expr_named_decl_offset)
SPEC_OFFSET_PARTITION_INDEX(an_ifc_expr_named_decl_offset)


/*
Explicit specializations of functions for FormSpecOffset.
*/
SPEC_OFFSET_PARTITION_KIND(an_ifc_form_spec_offset)
SPEC_OFFSET_PARTITION_INDEX(an_ifc_form_spec_offset)


/*
Explicit specializations of functions for LineOffset.
*/
SPEC_OFFSET_PARTITION_KIND(an_ifc_line_offset)
SPEC_OFFSET_PARTITION_INDEX(an_ifc_line_offset)


/*
Explicit specializations of functions for ScopeOffset.
*/
SPEC_OFFSET_PARTITION_KIND(an_ifc_scope_offset)
SPEC_OFFSET_PARTITION_INDEX(an_ifc_scope_offset)


/* End the suppression of GCC warnings. */
END_DISABLE_GCC_WARNING_MAYBE_UNITIALIZED

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

