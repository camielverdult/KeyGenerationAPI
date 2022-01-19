#!/usr/bin/tclsh

set type [ lindex $argv 0 ]
set file [ lindex $argv 1 ]
set comment [ lindex $argv 2 ]
set passphrase [ lindex $argv 3 ]

exec ./ssh-keygen -t $type -N $passphrase -C $comment -f $file