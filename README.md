# IDS-Tool

This repository contains a tool used for quality assurance of IDS files according to the buildingSMART standard.

It is comprised of two major components:

- a reusalble dll that can be embedded in other applications, and 
- a command line tool for direct usage.

The tool itself is a .NET console application tha returns errors and warnings
on the files, in order to verify their correctness.

## Usage

Executing `ids-tool help` provides the following guidance for available commands.

```
=== ids-tool - utility tool for buildingSMART IDS files.
ids-tool 1.0.10
Claudio Benghi

  audit        Audits for ids schemas and ids files.

  errorcode    provides description of tool's error code, useful when providing useer feedback in batch commands.

  help         Display more information on a specific command.

  version      Display version information.
```

`ids-tool help <command>` provides guidance on the options available for that command.

### the Audit command

Options for the `audit` verb are as follows:

```
  -x, --xsd            XSD schema(s) to load, this is useful when testing changes in the schema (e.g. GitHub repo). If
                       this is not specified, an embedded schema is adopted depending on the each ids's declaration of
                       version.

  -s, --schema         (Default: false) Check validity of the xsd schema(s) passed with the `xsd` option. This is useful
                       for the development of the schema and it is in use in the official repository for quality
                       assurance purposes.

  -e, --extension      (Default: ids) When passing a folder as source, this defines which files to audit by extension.

  -c, --omitContent    Skips the audit of the agreed limitation of IDS contents.

  --help               Display this help screen.

  --version            Display version information.

  source (pos. 0)      Input IDS to be processed; it can be a file or a folder.
```

## File Auditing Examples

Simple usage: `ids-tool audit path-to-some-file` or `ids-tool audit path-to-some-folder`.

If no option is specificed all available audits are performed on the IDS files.

## Roadmap

We are planning to introduce more checks:

- [x] XSD Schema check
  - [x] Use Xsd from disk
  - [x] Use relevant Xsd from resource
- [ ] IFC Schema check 
	- [x] Cleanup of old auditing logic (based on pure xml)
	- [ ] IfcEntity
		- [ ] Predefined types
			- [ ] Predefined types Names are hardcoded in the library (Ifc2x3, Ifc4, Ifc4x3)
		- [x] IfcTypeNames
			- [x] IfcType Names are hardcoded in the library (Ifc2x3, Ifc4, Ifc4x3)
			- [x] Simple type names are audited
			- [x] More complex case of type match need to be audited
			  - [x] Regex matches
			  - [X] Multiple values allowed
	- [x] Attribute names
		- [x] Attribute Names are hardcoded in the library
		- [x] Simple attribute names are audited
		- [x] More complex case of type match need to be audited
		  - [x] Regex matches
		  - [x] Multiple values allowed
	
- [ ] PSets 
  - [ ] Standard PSet Names for types
	- [ ] Includes IFC type inheritance
  - [ ] Pset name validity -> No custom "PSET_*"
- [ ] Cardinality
  - [ ] Context
    - [x] For Specification
	- [ ] For facets
		- [ ] partOf
		- [ ] classification
		- [ ] attribute
		- [ ] property
		- [ ] material
  - Audit types
    - [x] Min and Max values are intrinsically valid
	- [ ] Min and Max values are limited to agreed patterns

    

- [ ] Measures
  - [ ] optional but if a value is provided then it needs to be checked
- [ ] DoorLiningProperties - discuss requirements
