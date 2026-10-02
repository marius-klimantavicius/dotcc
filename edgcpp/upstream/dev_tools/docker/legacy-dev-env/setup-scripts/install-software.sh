#!/bin/bash

# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

# Install EPEL to expand the available package set.
yum -y install epel-release

packages=()

# Add supported compilers
packages+=("gcc-c++" "clang")

# Add core build utilities
packages+=("make" "ninja-build")

# Add build dependencies
packages+=("glibc-devel.x86_64" "glibc-devel.i686" "libquadmath-devel")

# Add testing dependencies
packages+=("sharutils")       # uudecode

# Add debuggers
packages+=("gdb" "rr")

# Add support for X11 based graphics
packages+=("xauth")

# Add debugger graphical frontends
packages+=("ddd")

# Add the python interpreter
packages+=("python3")

# Add Python based dependencies
packages+=("python3-pip")

# Add useful linux commands
packages+=("findutils")       # find, xargs
packages+=("util-linux")      # kill, runuser
packages+=("diffutils")       # diff, cmp, and friends
packages+=("ncurses")         # clear
packages+=("which")           # which
packages+=("procps-ng")       # top, free, ps
packages+=("less")            # less

# Add extra useful tools
packages+=("fish")

# Install the core packages (dnf fetches the package database on install,
# so forcing an update here, particularly for development is counter
# productive to image build time -- nearly doubling it on a MacBook Pro
# circa 2018).
yum install -y "${packages[@]}"
