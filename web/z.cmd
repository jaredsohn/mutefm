@echo off
call git commit -a -m %1
c:\pscp -r * jaredsohn_mutetunes@ssh.phx.nearlyfreespeech.net:

