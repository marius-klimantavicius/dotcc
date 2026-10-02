//type: s
//options: 
// { dg-additional-options -fopenmp }
// { dg-require-effective-target fopenmp } 
#pragma omp declare simd // { dg-error "" }
