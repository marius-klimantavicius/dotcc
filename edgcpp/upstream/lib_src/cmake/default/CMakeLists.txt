# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

cmake_minimum_required(VERSION 3.19)

add_edg_cpp_rt_target("edg-cpp-rt")

# Normalize the name based on the configuration/expectations of ECCP.
set_target_properties(edg-cpp-rt PROPERTIES
                      ARCHIVE_OUTPUT_NAME
                      "$CACHE{EDG_RUNTIME_LIB}$CACHE{EDG_LIB_SUFFIX}"
                      LIBRARY_OUTPUT_NAME
                      "$CACHE{EDG_RUNTIME_LIB}$CACHE{EDG_LIB_SUFFIX}")

# Our C library depends on the cpfe build
add_dependencies(edg-cpp-rt cpfe edg_prelink edg_decode)

# edg-cpp-rt-libs (obviously) depends on edg-cpp-rt
add_dependencies(edg-cpp-rt-libs edg-cpp-rt)
