#!/bin/bash -e

# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

arch=$(uname -m)

packages=()
debug_packages=()

# Request additional debug information for glibc.
debug_packages+=("glibc-devel")

# Add debuggers
packages+=("gdb" "rr" "lldb")

# Add support for X11 based graphics
packages+=("xauth")

# Add debugger graphical frontends
packages+=("ddd")

# Add Python based dependencies
packages+=("python3-pip" "python3-mypy" "python3-devel")

# Add useful linux commands
packages+=("findutils")       # find, xargs
packages+=("fd-find")         # fd
packages+=("util-linux")      # kill, runuser
packages+=("diffutils")       # diff, cmp, and friends
packages+=("ncurses")         # clear
packages+=("jq")              # jq
packages+=("plocate")         # locate
packages+=("which")           # which
packages+=("procps-ng")       # top, free, ps
packages+=("less")            # less
packages+=("time")            # GNU time command

# Add performance tuning tooling.
packages+=("hyperfine")                # hyperfine (benchmarking software)
packages+=("perf" "js-d3-flame-graph") # hardware based profiler and dependency
                                       # for flamegraph generation

# Add the aspell shell for testing checking spelling.
packages+=("aspell-en")

# Add extra useful tools:
packages+=("fish" "tcsh")

# Add direnv for contextual environment files.
packages+=("direnv")

# Add cvise:
packages+=("cvise")

# Add valgrind:
packages+=("valgrind")

# Install the core packages (dnf fetches the package database on install,
# so forcing an update here, particularly for development is counter
# productive to image build time -- nearly doubling it on a MacBook Pro
# circa 2018).
dnf install -y "${packages[@]}"

# Install the debug packages
dnf debuginfo-install -y "${debug_packages[@]}"

