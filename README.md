# IDS-Tool

This repository contains a crude tool used for quality assurance of IDS files.

The tool itself is a .NET console application tha returns errors and warnings
on the published files, in order to verify their correctness.

## Usage

```
id-tool.exe check <options> source

ids-tool 1.0.0
Copyright (C) 2021 ids-tool
  check      check files for issues.
  help       Display more information on a specific command.
  version    Display version information.
```

### File Checking Options

Simple usage: `ids-tool check foldername`

If no checking option is specificed then all checks are performed.

```
  -s, --schema            Check XSD schema compliance against the relevant version.

  source                  Required. Input source to be processed can be file or folder
```

## Roadmap

We are planning to introduce more checks:

- [ ] IFC check - IfcTypeNames
- [ ] PSets 
  - [ ] Standard PSet Names for types
	- [ Includes IFC type inheritance
  - [ ] Pset name validity -> No custom "PSET_*"
- [ ] Cardinality
	- Min and Max values are valid
	
- we want all classes of IFC (and attributes)
- DoorLiningiProperties (fake properties of 2x3)
- Measure is optional but if value then becomes mandatory