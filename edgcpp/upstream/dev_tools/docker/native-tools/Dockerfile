# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

FROM fedora:43

# Setup working directories
RUN mkdir /edg && \
    mkdir /edg/native-tools && \
    mkdir /edg/native-tools-src

# Install required software
RUN dnf install -y \
    ninja-build \
    cmake \
    gcc-c++

# Copy and extract the current native-tools.tar.gz archive
COPY native-tools.tar.gz /edg/native-tools.tar.gz
RUN tar -xvf /edg/native-tools.tar.gz --directory=/edg/native-tools-src

# Build the native version of the tools
RUN cd /edg/native-tools-src && \
  cmake -DCMAKE_BUILD_TYPE=Release \
        -G Ninja \
        -DCMAKE_INSTALL_PREFIX:PATH=/edg/native-tools \
        -B build/ . && \
  cmake --build build --target install
