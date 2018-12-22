<?php

function sanitize($x){
	return $x;
}
function output($x){
}

output("Hello, ");
$name = $_GET["name"];
$sane = sanitize($name);
output($sane);