#!/bin/bash -e

# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

# Install extra-software/opt/ to /opt/
fd --type file . /edg/opt-software/ | xargs -I {} \
  tar xvzf {} --directory=/opt/

# Install extra-software/usr-local/ to /usr/local/
fd --type file . /edg/usr-local-software/ | xargs -I {} \
  tar xvzf {} --directory=/usr/local/
