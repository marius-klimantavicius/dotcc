# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

FROM fedora:43

# Setup working directories
RUN mkdir /edg && \
    mkdir /edg/softfloat && \
    mkdir /edg/softfloat/include && \
    mkdir /edg/softfloat/lib && \
    mkdir /edg/softfloat/lib/x86_64/ && \
    mkdir /edg/softfloat/lib/x86/ && \
    mkdir /edg/softfloat-src

# Install required software
RUN dnf install -y \
    make \
    automake \
    gcc \
    gcc-c++ \
    glibc-devel.x86_64 \
    glibc-devel.i686

# Copy and extract the current native-tools.tar.gz archive
COPY softfloat.tar.gz /edg/softfloat.tar.gz
RUN tar -xvf /edg/softfloat.tar.gz --directory=/edg/softfloat-src

# Build the softfloat library
RUN cd /edg/softfloat-src/build/Linux-x86_64-GCC/ && make -j
RUN cd /edg/softfloat-src/build/Linux-386-SSE2-GCC/ && make -j

# Copy the include files and softfloat library to a normalized location
RUN cp /edg/softfloat-src/build/Linux-x86_64-GCC/softfloat.a /edg/softfloat/lib/x86_64/ && \
    cp /edg/softfloat-src/build/Linux-386-SSE2-GCC/softfloat.a /edg/softfloat/lib/x86/ && \
    cp /edg/softfloat-src/source/include/softfloat.h /edg/softfloat/include/ && \
    cp /edg/softfloat-src/source/include/softfloat_types.h /edg/softfloat/include/

