<?php 

/**
Some sanitize function
*/
function escape_input($input){
	return strtolower ($input);
}

//sanitize name value
$name = escape_input($_GET["name"]);
$nameQuery = "SELECT * FROM users WHERE name='" . $name . "'";
mysql_query($nameQuery);

$id = escape_input($_GET["id"]);
if ($id > 5){
	$query = "DELETE * FROM users WHERE id='" . $id . "'";
	for($i=0; $i<$id; $i++){
		$query = mysql_real_escape_string("DELETE * FROM users WHERE id='" . $id . "'");
		mysql_query($query);
	}
}

