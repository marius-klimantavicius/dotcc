/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

#ifndef MS_METADATA
#define MS_METADATA 1

#if MICROSOFT_EXTENSIONS_ALLOWED

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

/*
C++/CLI metadata import flags.
*/
enum a_cpp_cli_import_flag {
  cpp_cli_none                   = 0x0000,
                        /* Default import behavior. */
  cpp_cli_as_friend_assembly     = 0x0001,
                        /* Treats the imported assembly as a friend
                           assembly. */
  cpp_cli_declspec_assembly_info  = 0x0002,
                        /* Adds a __declspec(assembly_info(...)) specifier to
                           every imported type indicating its metadata token.
                           This is used to uniquely identify the type in the
                           assembly and to obtain the definition of the type
                           on demand. */
  cpp_cli_declspec_member_info   = 0x0004,
                        /* Adds a __declspec(member_info(...)) specifier to
                           every imported field or method indicating its
                           metadata token. */
  cpp_cli_define_all_types       = 0x0008,
                        /* Imports all types in the assembly as opposed to only
                           the top-level declarations. */
  cpp_cli_wchar_t_is_keyword     = 0x0010,
                        /* Imports wide character types as "wchar_t" instead
                           of "unsigned short". */
  cpp_cli_ide_custom_attributes  = 0x0020,
                        /* Imports custom attributes that pertain to the
                           design-time experience in the IDE. */
  cpp_cli_all_custom_attributes  = 0x0040
                        /* Imports all custom attributes. */
};

typedef unsigned int a_cpp_cli_import_flag_set;

EXTERN_THREAD a_cpp_cli_import_flag_set
                default_cpp_cli_import_flags
#if VAR_INITIALIZERS
			= (a_cpp_cli_import_flag_set)
			                       cpp_cli_declspec_assembly_info
#endif /* VAR_INITIALIZERS */
			                                                     ;
                        /* Flags used to control the behavior of metadata
                           import. */

extern an_assembly_index import_metadata_file(
                                a_const_char              *assembly_full_name,
                                a_cpp_cli_import_flag_set import_flags,
                                a_boolean                 *is_duplicate);
extern void import_all_types(an_assembly_index assembly_index,
                             char              *buffer,
                             size_t            *buffer_size);
extern void import_class_definition(
                                 an_assembly_scope_index assembly_scope_index,
                                 a_cpp_cli_token         typedef_token,
                                 char                    *buffer,
                                 size_t                  *buffer_size,
                                 a_boolean               *is_delegate);
extern void ms_metadata_trans_unit_init(a_const_char *trans_unit_file_name);
extern void ms_metadata_trans_unit_wrapup(void);
extern void ms_metadata_cleanup(void);

#if CPPCLI_ENABLING_POSSIBLE
#if READ_CPPCLI_PORTABLE_ASSEMBLIES || WRITE_CPPCLI_PORTABLE_ASSEMBLIES

typedef struct a_portable_assembly_header {
  /* This structure defines the data found at the beginning of a portable
     assembly file.  These fields are each converted to ASCII and formatted as
     %08x in the file.  The header is terminated with a newline. */
#define PORTABLE_ASSEMBLY_HEADER_FORMAT "%08x %08x %08x\n"
#define PORTABLE_ASSEMBLY_MAGIC_NUMBER  0x11223344
  uint32_t      magic;  /* Identifying "magic" number for portable assembly
                           files. */
  uint32_t      num_entries;
                        /* The number of a_portable_assembly_table_entrys
                           this file contains. */
  uint32_t      table_offset;
                        /* An offset (from the beginning of the file) to the
                           a_portable_assembly_table_entry table. */
  /* This header is followed by the string data, then the table. */
} a_portable_assembly_header;

typedef struct a_portable_assembly_table_entry {
  /* Each entry in this table represents the metadata associated with a
     particular C++/CLI metadata token in a particular import scope.  These
     fields are each converted to ASCII and formatted as %04hx (for 16-bit
     fields) or %08x (for 32-bit fields) in the file.  Each entry is
     terminated with a newline. */
#define PORTABLE_ASSEMBLY_TABLE_FORMAT "%04hx %08x %08x %08x\n"
  uint16_t      scope_index;
                        /* The index of the import scope in the assembly
                           containing the definition of the type associated
                           with this entry. */
  uint32_t      token;  /* The C++/CLI metadata type_def token associated with
                           this entry.  The first entry in this table
                           (token == 0) refers to the string returned by
                           import_all_types. */
  uint32_t      offset; /* Offset (from the beginning of the file) to the
                           metadata string associated with token. */
  uint32_t      size;   /* Size (in bytes) of the associated metadata. */
} a_portable_assembly_table_entry;

/*
A string prepended to delegate definitions in portable assembly files.
(This is needed to reliably return the "is_delegate" flag in the version
of import_class_definition for portable assemblies.)
*/
#define PORTABLE_ASSEMBLY_DELEGATE_PREFIX "delegate "

#endif /* READ_CPPCLI_PORTABLE_ASSEMBLIES || WRITE_CPPCLI_PORTABLE_ASSEMBLIES*/
#endif /* CPPCLI_ENABLING_POSSIBLE */

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* MICROSOFT_EXTENSIONS_ALLOWED */
#endif /* MS_METADATA */

