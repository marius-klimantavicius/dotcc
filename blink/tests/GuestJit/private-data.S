/* Ordinary private data lifetime witness: static main ELF code, later mmap
 * data/stacks, and legal concurrent MAP_FIXED plus R/RW protection changes.
 * The reader's VA always stays readable; munmap of its pages follows join.
 * Default: concurrent parent/child. Argument "s": deterministic single worker. */
.global _start
.text
_start:
    xor %r15d, %r15d
    cmpq $2, (%rsp)
    jb concurrent
    mov 16(%rsp), %rax
    cmpb $'s', (%rax)
    je single
concurrent:
    mov $65536, %esi
    call allocate
    mov %rax, %r15
    lea 65536(%rax), %rsi
    mov $56, %eax
    mov $0x7d0f00, %edi
    lea child_tid(%rip), %rdx
    mov %rdx, %r10
    lea child_tls(%rip), %r8
    syscall
    test %rax, %rax
    js failed
    je child
    /* Publication occurs after clone: these bytes were absent at first child. */
    call allocate_data
    mov %r12, published(%rip)
    mov $1, %eax
    xchg %eax, go(%rip)
    lea go(%rip), %rdi
    call wake_word
wait_ready:
    mov ready(%rip), %edx
    test %edx, %edx
    jne remap_begin
    lea ready(%rip), %rdi
    mov $128, %esi
    call wait_word
    jmp wait_ready
remap_begin:
    mov $1, %ebx
    mov $0x13579bdf, %r14d
remap_loop:
    call replace_page
    mov %ebx, phase(%rip)
wait_seen:
    mov seen(%rip), %edx
    cmp %ebx, %edx
    je next_remap
    lea seen(%rip), %rdi
    mov $128, %esi
    call wait_word
    jmp wait_seen
next_remap:
    xor $0x373f373f, %r14d
    inc %ebx
    cmp $33, %ebx
    jne remap_loop
    movl $1, done(%rip)
join:
    mov child_tid(%rip), %edx
    test %edx, %edx
    je joined
    lea child_tid(%rip), %rdi
    xor %esi, %esi
    call wait_word
    jmp join
joined:
    mov (%r12), %rax
    cmp $32768, %rax
    jne failed
    mov (%r13), %rax
    cmp $0x2468ace0, %rax
    jne failed
    jmp cleanup
single:
    call allocate_data
    mov %r12, %rbx
    call memory_work
    mov $0x13579bdf, %r14d
    call replace_page
    mov %r13, %rbx
    call memory_work
    mov (%r12), %rax
    cmp $32768, %rax
    jne failed
cleanup:
    mov %r12, %rdi
    mov $8192, %esi
    call unmap
    test %r15, %r15
    je success
    mov %r15, %rdi
    mov $65536, %esi
    call unmap
success:
    mov $1, %eax
    mov $1, %edi
    lea message(%rip), %rsi
    mov $message_end-message, %edx
    syscall
    cmp $message_end-message, %rax
    jne failed
    xor %edi, %edi
    jmp exit_group
child:
    mov go(%rip), %edx
    test %edx, %edx
    jne child_work
    lea go(%rip), %rdi
    mov $128, %esi
    call wait_word
    jmp child
child_work:
    mov published(%rip), %rbx
    lea 4096(%rbx), %r13
    call memory_work
    mov $1, %eax
    xchg %eax, ready(%rip)
    lea ready(%rip), %rdi
    call wake_word
    xor %ebp, %ebp
reader:
    /* Read phase first, then acknowledge a load from that completed remap.
     * Concurrent replacement may expose old/new canaries or fresh zeroes. */
    mov phase(%rip), %r14d
    mov (%r13), %rax
    test %rax, %rax
    je value_ok
    cmp $0x13579bdf, %rax
    je value_ok
    cmp $0x2468ace0, %rax
    jne failed
value_ok:
    cmp %ebp, %r14d
    je reported
    mov %r14d, %ebp
    mov %r14d, seen(%rip)
    lea seen(%rip), %rdi
    call wake_word
reported:
    mov done(%rip), %eax
    test %eax, %eax
    je reader
    xor %edi, %edi
    mov $60, %eax
    syscall
    jmp failed
memory_work:
    /* Every hot iteration uses both later data and the worker's valid stack. */
    sub $32, %rsp
    mov $0x12345678, %edx
    mov %rdx, 16(%rsp)
    xor %eax, %eax
    mov %rax, (%rbx)
    mov $32768, %ecx
work_loop:
    mov (%rbx), %rax
    add $1, %rax
    mov %rax, (%rbx)
    mov %rax, 8(%rsp)
    mov 8(%rsp), %rdx
    cmp %rax, %rdx
    jne failed
    dec %ecx
    jne work_loop
    cmp $32768, %rax
    jne failed
    mov 16(%rsp), %rdx
    cmp $0x12345678, %rdx
    jne failed
    add $32, %rsp
    ret
replace_page:
    mov $9, %eax
    mov %r13, %rdi
    mov $4096, %esi
    mov $3, %edx
    mov $50, %r10d
    mov $-1, %r8
    xor %r9d, %r9d
    syscall
    cmp %r13, %rax
    jne failed
    mov %r14, (%r13)
    mov $10, %eax
    mov $1, %edx
    syscall
    test %rax, %rax
    jne failed
    mov $10, %eax
    mov $3, %edx
    syscall
    test %rax, %rax
    jne failed
    /* Retire an unrelated mapping while the reader remains active too. */
    mov $4096, %esi
    call allocate
    mov %r14, (%rax)
    mov %rax, %rdi
    mov $4096, %esi
    call unmap
    ret
allocate_data:
    mov $8192, %esi
    call allocate
    mov %rax, %r12
    lea 4096(%rax), %r13
    xor %eax, %eax
    mov %rax, (%r12)
    mov %rax, (%r13)
    ret
allocate:
    mov $9, %eax
    xor %edi, %edi
    mov $3, %edx
    mov $34, %r10d
    mov $-1, %r8
    xor %r9d, %r9d
    syscall
    cmp $-4095, %rax
    jae failed
    ret
unmap:
    mov $11, %eax
    syscall
    test %rax, %rax
    jne failed
    ret
wake_word:
    mov $129, %esi
    mov $1, %edx
    mov $202, %eax
    syscall
    test %rax, %rax
    js failed
    ret
wait_word:
    xor %r10d, %r10d
    xor %r8d, %r8d
    xor %r9d, %r9d
    mov $202, %eax
    syscall
    test %rax, %rax
    je wait_return
    cmp $-11, %rax
    je wait_return
    cmp $-4, %rax
    jne failed
wait_return:
    ret
failed:
    mov $1, %edi
exit_group:
    mov $231, %eax
    syscall
    jmp failed
.section .rodata
message: .ascii "private-data: 32768\n"
message_end:
.section .bss
.balign 16
child_tls: .zero 16
published: .zero 8
child_tid: .zero 4
go: .zero 4
ready: .zero 4
phase: .zero 4
seen: .zero 4
done: .zero 4
.section .note.GNU-stack,"",@progbits
