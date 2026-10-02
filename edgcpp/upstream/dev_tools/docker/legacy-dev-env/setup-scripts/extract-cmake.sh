#!/bin/bash

# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

cd /edg/cmake/
# Extract the archive.
tar -xvzf setup/cmake.tar.gz
# Move/rename the contained folder to a "normalized" name.
mv cmake-3.19.8-Linux-x86_64 install
