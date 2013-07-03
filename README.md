Floomberg
=========

Some F# based demos of the BEmu Bloomberg emulator

Background
==========

This repo contains some simple F# demos using the BEmu Bloomberg emulator.  The emulator itself is hosted here:

http://bemu.codeplex.com/

This is not (currently) a good example of F# coding.  It's just a way of exploring the Bloomberg API.

Getting Started
===============

- Download this repo as normal.

- Download a zip of the source for bemu from here: http://bemu.codeplex.com/SourceControl/latest.

- Unzip the BEmu folder at the same level as the Floomberg.sln file.  Your directory structure should look like this:

BEmu  <-- Folder you unzipped from the download
  bin
  BloombergTypes
  HistoricDataRequest
  (etc.)
  
Floomberg
  bin
  obj
  Examples.fsproj
  SendRequestExample.fs
  
.gitattributes
Floomberg.sln

- Run Visual Studio 2012 as admin.  (This is necessary because the first compilation of Bemu does a some kind of COM install.)

- Open Floomberg.sln and compile.

- Run the Examples project.

Future
======

There are plans for Bloomberg to release its own emulator (presumably with more realistic data) some time in Summer 2013.

At that point we are likely to refactor this project to use the official emulator.
