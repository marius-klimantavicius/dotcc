# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

project(check_closing_comments)

add_executable(check-closing-comments main.cpp)

install(TARGETS check-closing-comments)
