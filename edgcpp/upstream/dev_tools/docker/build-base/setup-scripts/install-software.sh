#!/bin/bash -e

# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

arch=$(uname -m)

packages=()

# Add supported compilers
packages+=("gcc-c++" "clang")

# Add core build utilities
packages+=("make" "cmake" "ninja-build")

# Add build dependencies
if [[ "$arch" == "x86_64" ]] ; then
  packages+=(
    "libstdc++-static.x86_64"
    "libstdc++-static.i686"
    "glibc-static.x86_64"
    "glibc-static.i686"
    "glibc-devel.x86_64"
    "glibc-devel.i686"
    "libasan.x86_64"
    "libasan.i686"
    "libquadmath-devel"
  )
else
  packages+=(
    "glibc-devel"
  )
fi
packages+=("libunwind-devel")

# Add dependencies for building programs with eccp
if [[ "$arch" == "x86_64" ]] ; then
  packages+=(
    "libstdc++.x86_64"
    "libstdc++.i686"
  )
else
  packages+=(
    "libstdc++"
  )
fi

# Add optimal linkers
packages+=("mold" "lld")

# Add testing dependencies
packages+=("sharutils")       # uudecode

# Add git
packages+=("git")

# Add the dash shell for testing sh scripts.
packages+=("dash")

# Install the core packages (dnf fetches the package database on install,
# so forcing an update here, particularly for development is counter
# productive to image build time -- nearly doubling it on a MacBook Pro
# circa 2018).
dnf install -y "${packages[@]}"
