// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Ordinary runtime language acceptance; no required constant evaluation.
int global=1;
constexpr int write(int& value){value=2; return value;}
constexpr int& expired(int& safe,bool leave){if(leave){int value=1; return value;} return safe;}
constexpr int read(bool run){int value; if(run) value+=1; else value=3; return value;}
int runtime_global(){return write(global);}
int runtime_expired(){expired(global,true)=2; return 1;}
int runtime_uninitialized(){return read(true);}
