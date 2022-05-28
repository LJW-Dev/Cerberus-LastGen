# Cerberus
A fork of Cerberus for last gen files (Xbox / PS3). Does not support Black Ops 2.

# Usage
`Cerberus.CLI [options] <file / folder (.gsc|.csc|.gscc|.cscc|.ff)>` \
Options: 
* --disassemble - Disassembles the script/s.

# Features
* Extracts GSC files from FastFiles.
* Updated hash table.

# Info
* Can decompile custom scripts, but the output might not be 100% correct.
* Last Gen doesn't have DevBlock Opcodes, and instead replaces them with a jump. If a function starts with a "continue" or always jumps over a block it is likely a DevBlock.
* Older GSC files don't have the same opcodes and can't be decompiled unless the opcode list is updated.
