# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

project(process_unicode_names)

add_executable(process-unicode-names main.cpp)

install(TARGETS process-unicode-names)
