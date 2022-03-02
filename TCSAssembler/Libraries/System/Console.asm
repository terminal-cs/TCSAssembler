System.Console.WriteLine:
    lodsb
    or al, al  ;zero=end of str
    jz .done    ;get out
    mov ah, 0x0E
    mov bh, 0
    int 0x10
    jmp System.Console.WriteLine
.done:
    ret
