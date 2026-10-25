.global _start
.text
_start:
    xor %edi,%edi
    mov $34,%r10d
    call map
    test %rax,%rax
    js fail
    mov %rax,%r12
    movb $77,(%r12)
    mov $32,%ebp
again:
    xor %edi,%edi
    mov $33,%r10d              /* shared anonymous, no backing contract */
    call map
#ifdef PROFILE
    cmp $-95,%rax
    jne fail
#else
    test %rax,%rax
    js fail
    mov %rax,%rdi
    call unmap
#endif
    dec %ebp
    jne again
    mov %r12,%rdi
    mov $49,%r10d              /* rejection must precede MAP_FIXED removal */
    call map
#ifdef PROFILE
    cmp $-95,%rax
    jne fail
    cmpb $77,(%r12)
    jne fail
#else
    cmp %r12,%rax
    jne fail
    cmpb $0,(%r12)
    jne fail
#endif
    mov %r12,%rdi
    call unmap
    xor %edi,%edi
    mov $34,%r10d
    call map
    test %rax,%rax
    js fail
    movb $51,(%rax)            /* allocator/page tables still usable */
    mov %rax,%rdi
    call unmap
    xor %edi,%edi
    jmp exit
map:
    mov $9,%eax
    mov $4096,%esi
    mov $3,%edx
    mov $-1,%r8
    xor %r9d,%r9d
    syscall
    ret
unmap:
    mov $11,%eax
    mov $4096,%esi
    syscall
    test %rax,%rax
    jne fail
    ret
fail:
    mov $99,%edi
exit:
    mov $231,%eax
    syscall
.data
.byte 0
.section .note.GNU-stack,"",@progbits
