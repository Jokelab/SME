<?php
//classic_blacklisting.php
$addr = $_GET["addr"];
# Blacklisting command injection separators.
$blacklisting = array(
  ';' => '',
  '&&'=> '',
  '|' => '',
  '`' => ''
 );
$addr = str_replace(array_keys($blacklisting),$blacklisting,$addr);
if( stristr(php_uname('s'), 'Windows NT')){
  # Windows-based command execution.
  echo ('ping '.$addr);
} else {
  # Unix-based command execution.
  echo ("/bin/ping -c 4 ".$addr);
}

//classic_b64.php
if (base64_encode(base64_decode($_GET["addr"])) === $_GET["addr"]){
	if( stristr(php_uname('s'), 'Windows NT')){
	# Windows-based command execution.
	echo ('ping '.base64_decode($_GET["addr"]));
	} else {
	# Execute command!
	echo ("/bin/ping -c 4 ".base64_decode($_GET["addr"]));
	}
} else {
	echo 'Please, encode your input to Base64 format.';
}
					
//classic str_replace.php
$user = str_replace(array("\\","'", '"'), "", $_GET["user"]); 
eval("echo(\"$user\");"); 