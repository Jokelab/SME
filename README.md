# SME

## Introduction
SME is a tool based on a dynamic analysis technique called *secure multi-execution*. The tool consists of several components, each with its own responsibilities:
  - SME.Cli: the command line interface.
  - SME.Transformer: performs static code transformations according to a customizable policy. 
  - SME.Scheduler: executes transformed code and captures channel values.
  - SME.Factory: creates transformer and scheduler instances based on the input language.
  - SME.Shared: contains all language independent type definitions and utility classes.

## Prerequisites
The tool is written in C# and all components are compliant with .NET standard 2.0. 
The [.NET Core SDK](https://dotnet.microsoft.com/download/dotnet-core/2.1) is required to compile the code. Any of the available platform specific runtimes can be used to execute the CLI (SME.Cli) application.

## Input language support
The primary input for this tool is a program. The tool is designed to be language independent, but currently the only supported input language is PHP. To integrate the PHP environment with .NET, the tool makes use of the [PeachPie compiler](https://github.com/peachpiecompiler/peachpie). In order to extend the tool for different programming languages, language specific classes for the the transformer and scheduler interfaces should be implemented.

## Basic usage
The following example shows how this tool can help to reveal and patch injection vulnerabilities.
- Create a file named **xss.php** with the following contents:
```php
<html>
<?php
$name = $_GET['name'];
$msg = 'welcome ' . $name;
?>
<head>
    <title><?php echo($name); /* XSS 1 */ ?></title>
</head>
<body>
<?php 
	echo "Hello, " . $msg; /* XSS 2 */ 
?>
</body>
</html>
```
According to the default [policy file](https://github.com/Jokelab/SME/blob/master/SME.Cli/policy.xml), the code contains one input channel ($GET_['name']) and two output channels (the echo statements). This code obviously suffers from injection vulnerabilities, because it will echo the input value without any restriction. Our goal is to detect the interferent channels in order to patch them.
- Open a command shell.
- Navigate to the SME.Cli directory.
- Execute the following command (with the correct path to xss.php)
```sh
dotnet run -input:yourdirectory\xss.php -get:name=Robert  
```
The tool should report the exact location of the interferent channels:
```
Observed 2 channel(s) with different output values between the SME exection and the original execution. 
The code is thus interferent.

=> Channel ID 1 (level H)
Code: echo($name);
Position in original code:  92..104 (Line 7)
Captured differences:
#1 Original     : Robert
#1 SME          :

=> Channel ID 2 (level H)
Code: echo "Hello, " . $msg;
Position in original code:  155..177 (Line 11)
Captured differences:
#1 Original     : Hello, welcome Robert
#1 SME          : Hello, welcome
```
Now patch the code, for example by wrapping the input and/or output channels with the [htmlspecialchars](http://php.net/manual/en/function.htmlspecialchars.php) function:
```php
htmlspecialchars(echo($name));
```
Run the tool again. It should now report no vulnerable output channels for the provided input values.

#### Parameters

The following command line parameters are supported:

| Parameter     | Description  | Default       
| ------------- |:-------------|:-------------:|
| -input:<path>      | Path to a program. If the path is not rooted, the current directory will be assumed. | [samples\sqli.php](https://github.com/Jokelab/SME/blob/master/SME.Cli/samples/sqli.php)
| -output:<path>      | Path to an output file. If the path is not rooted, the current directory will be assumed. If empty, no output file will be generated. | Empty
| -policy:<path>      | Path to XML policy file. If the path is not rooted, the current directory will be assumed. | [policy.xml](https://github.com/Jokelab/SME/blob/master/SME.Cli/policy.xml)
| -params:<path>      | Path to a parameters file which can be used to provide different input values for a single program. Each line in the parameters file represents a set of input values. If not empty, it will override the -get -post -cookie and -session options. | Empty
| -save| Persist the code transformations in the same directory as the input file.       | No
| -showverdict | Show detailed verdict information | No 
| -get:<values> | HTTP-get input values. Example: -get:key1=value1&key2=value2       | None
| -post:<values> | HTTP-post input values. Example: -post:key1=value1&key2=value2      | None
| -cookies:<values> | HTTP-cookie input values. Example: -cookies:key1=value1&key2=value2 | None 
| -session:<values> | Session input values. Example: -session:key1=value1&key2=value2 | None 



License
----

[MIT](https://github.com/Jokelab/SME/blob/master/LICENSE)
