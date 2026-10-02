# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

# List files for the front end
set(ALL_C_FILES const_ints.c debug.c error.c fe_init.c fixed_pt.c float_pt.c
                floating.c host_envir.c il.c il_display.c il_read.c
                il_to_str.c il_walk.c mem_manage.c src_seq.c target.c types.c)
list(TRANSFORM ALL_C_FILES PREPEND ${PROJECT_SOURCE_DIR}/)

# Load the specified macro configuration for cdisp.
macro_configuration_to_compile_definitions("cdisp")

# compile .c files as C++
set_source_files_properties(cpfe ${ALL_C_FILES} PROPERTIES LANGUAGE CXX)

# add the executable
add_executable(cdisp ${ALL_C_FILES} ${TXT_FILES})
add_dependencies(cdisp error-info)

add_softfloat_to_target(cdisp)
