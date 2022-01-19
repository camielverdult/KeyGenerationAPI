#!/usr/bin/tclsh

set type [ lindex $argv 0 ]
set temp_dir [ lindex $argv 1 ]
set comment [ lindex $argv 2 ]
set passphrase [ lindex $argv 3 ]

exec ./ssh-keygen -t $type -f $temp_dir/id_$type -C $comment -N $passphrase